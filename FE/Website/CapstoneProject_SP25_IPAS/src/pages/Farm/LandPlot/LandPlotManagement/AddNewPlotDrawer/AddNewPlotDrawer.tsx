import { Drawer, Steps, Flex, Form } from "antd";
import { useEffect, useState } from "react";
import {
  GetLandPlot,
  landRowSimulate,
  LandPlotUpdateRequest,
  LandPlotUpdateCoordinationRequest,
  GetLandPlotSimulate,
  LandPlotRequest,
  LandPlotSimulateRequest,
} from "@/payloads";
import { ConfirmModal, EditActions } from "@/components";
import { DraggableRow, LandPlotCreate, RowConfiguration } from "@/pages";
import style from "./AddNewPlotDrawer.module.scss";
import { useModal, useStyle, useSystemConfigOptions } from "@/hooks";
import { useLoadingStore, useMapStore, useVirtualPlotConfigStore } from "@/stores";
import { toast } from "react-toastify";
import { fakeRowsData } from "../DraggableRow/fakeRowsData";
import {
  createPlotFormFields,
  MESSAGES,
  SYSTEM_CONFIG_GROUP,
  SYSTEM_CONFIG_KEY,
} from "@/constants";
import { DEFAULT_LAND_PLOT, isPlantOverflowing, validatePolygonBeforeSave } from "@/utils";
import { landPlotService } from "@/services";

const { Step } = Steps;

interface AddNewPlotDrawerProps {
  latitude: number;
  longitude: number;
  selectedPlot?: GetLandPlot | null;
  plotSimulate?: GetLandPlotSimulate;
  landPlots?: GetLandPlot[];
  fetchLandPlots?: () => Promise<void>;
  fetchSimulateLandPlots?: () => Promise<void>;
  isOpen: boolean;
  onClose: () => void;
}

const AddNewPlotDrawer: React.FC<AddNewPlotDrawerProps> = ({
  latitude,
  longitude,
  selectedPlot,
  plotSimulate,
  landPlots,
  fetchLandPlots,
  fetchSimulateLandPlots,
  isOpen,
  onClose,
}) => {
  const { styles } = useStyle();
  const [currentStep, setCurrentStep] = useState(0);
  const [form] = Form.useForm();
  const updateConfirmModal = useModal();
  const [plotData, setPlotData] = useState<LandPlotRequest>(DEFAULT_LAND_PLOT); // Lưu dữ liệu từ bước 1
  const [rowsData, setRowsData] = useState<landRowSimulate[]>(fakeRowsData);
  const {
    isDirty,
    setIsDirty,
    isPolygonDirty,
    setIsPolygonDirty,
    clearPolygons,
    isOverlapping,
    currentPolygon,
    setCurrentPolygon,
    setPolygonReady,
    // setPolygonDimensions,
    width,
    length,
  } = useMapStore();
  const isHorizontal =
    form.getFieldValue(createPlotFormFields.rowOrientation) === "Horizontal" ? true : false;
  const { isLoading, setIsLoading } = useLoadingStore();
  const isUpdate = !!selectedPlot;
  const isSimulateUpdate = !!plotSimulate;
  const { setConfigs } = useVirtualPlotConfigStore();
  const { options: virtualOptions, loading } = useSystemConfigOptions(
    SYSTEM_CONFIG_GROUP.VIRTUAL_PLOT,
    undefined,
    true,
  );
  const getValueFromConfig = (code: string) =>
    virtualOptions.find((opt) => opt.value === code)?.label;
  useEffect(() => {
    if (!loading && virtualOptions.length > 0) {
      const config = {
        metricUnit: String(getValueFromConfig(SYSTEM_CONFIG_KEY.METRIC_UNIT) || "m"),
        sizePlant: Number(getValueFromConfig(SYSTEM_CONFIG_KEY.DEFAULT_PLANT_SIZE)) || 0,
        rowWidth: Number(getValueFromConfig(SYSTEM_CONFIG_KEY.ROW_WIDTH)) || 0,
        distance: Number(getValueFromConfig(SYSTEM_CONFIG_KEY.PLANT_DISTANCE)) || 0,
        lineSpacing: Number(getValueFromConfig(SYSTEM_CONFIG_KEY.LINE_SPACING)) || 0,
        rowSpacing: Number(getValueFromConfig(SYSTEM_CONFIG_KEY.ROW_SPACING)) || 0,
      };
      setConfigs(config);
    }
  }, [virtualOptions]);

  const handleSave = async () => {
    setIsLoading(true);
    const plotDataRequest: LandPlotRequest = {
      ...plotData,
      numberOfRows: rowsData.length,
      minLength: 0,
      maxLength: 0,
      minWidth: 0,
      maxWidth: 0,
      landRows: rowsData.map((row) => ({
        rowIndex: row.rowIndex,
        treeAmount: row.treeAmount,
        distance: row.distance,
        length: row.length,
        width: row.width,
        direction: form.getFieldValue(createPlotFormFields.rowOrientation),
        description: form.getFieldValue(createPlotFormFields.description),
      })),
    };

    setPlotData(plotDataRequest);
    try {
      var result = await landPlotService.createLandPlot(plotDataRequest);

      if (result.statusCode === 200 || result.statusCode === 201) {
        resetForm();
        onClose();
        await fetchLandPlots?.();
        toast.success(result.message);
      } else {
        toast.warning(result.message);
      }
    } finally {
      setIsLoading(false);
    }
  };

  const handleNext = async () => {
    if (currentStep === 0) {
      await form.validateFields();
      const isValid = validatePolygonBeforeSave(
        currentPolygon,
        isOverlapping,
        width,
        length,
        isUpdate,
      );
      if (!isValid) return;
      setPlotData((prev) => ({
        ...prev,
        landPlotName: form.getFieldValue(createPlotFormFields.landPlotName),
        area: form.getFieldValue(createPlotFormFields.area),
        plotLength: form.getFieldValue(createPlotFormFields.length),
        plotWidth: form.getFieldValue(createPlotFormFields.width),
        soilType: form.getFieldValue(createPlotFormFields.soilType),
        description: form.getFieldValue(createPlotFormFields.description),
        targetMarket: form.getFieldValue(createPlotFormFields.targetMarket),
        landPlotCoordinations: currentPolygon!.coordinates[0].map(([longitude, latitude]) => ({
          longitude,
          latitude,
        })),
      }));

      setCurrentStep((prev) => prev + 1);
    } else if (currentStep === 1) {
      const values = await form.validateFields();
      const {
        [createPlotFormFields.rowLength]: rowLength,
        [createPlotFormFields.rowWidth]: rowWidth,
        [createPlotFormFields.numberOfRows]: numberOfRows,
        [createPlotFormFields.plantsPerRow]: treeAmount,
        [createPlotFormFields.plantSpacing]: distance,
        [createPlotFormFields.rowOrientation]: rowOrientation,
        [createPlotFormFields.lineSpacing]: lineSpacing,
        [createPlotFormFields.rowsPerLine]: rowsPerLine,
        [createPlotFormFields.rowSpacing]: rowSpacing,
      } = values;

      // if (isPlantOverflowing(distance, treeAmount, rowLength)) {
      //   toast.warning(MESSAGES.OUT_PLANT);
      //   return;
      // }

      const generatedRows: landRowSimulate[] = Array.from({ length: numberOfRows }, (_, index) => ({
        landRowId: index + 1,
        landRowCode: "",
        length: rowLength,
        width: rowWidth,
        treeAmount,
        distance,
        rowIndex: index + 1,
        plants: [],
      }));

      setRowsData(generatedRows);
      const isHorizontal = rowOrientation === "Horizontal" ? true : false;
      setPlotData((prev) => ({
        ...prev,
        isRowHorizontal: isHorizontal,
        lineSpacing: lineSpacing,
        rowPerLine: rowsPerLine,
        rowSpacing: rowSpacing,
      }));

      setCurrentStep((prev) => prev + 1);
    } else {
      handleSave();
    }
  };

  const handlePrev = () => setCurrentStep((prev) => prev - 1);

  const handleSaveUpdate = async () => {
    await form.validateFields();
    const isValid = validatePolygonBeforeSave(
      currentPolygon,
      isOverlapping,
      width,
      length,
      isUpdate,
    );
    if (!isValid || !selectedPlot || !currentPolygon) return;

    // Nếu không có thay đổi gì, reset và đóng modal luôn
    if (!isDirty && !isPolygonDirty) {
      resetForm();
      onClose();
      return;
    }

    setIsLoading(true);
    try {
      // Chuẩn bị request cập nhật thông tin thửa
      const plotDataUpdateRequest: LandPlotUpdateRequest | null = isDirty
        ? {
            landPlotId: selectedPlot.landPlotId,
            landPlotName: form.getFieldValue(createPlotFormFields.landPlotName),
            area: form.getFieldValue(createPlotFormFields.area),
            length: form.getFieldValue(createPlotFormFields.length),
            width: form.getFieldValue(createPlotFormFields.width),
            soilType: form.getFieldValue(createPlotFormFields.soilType),
            targetMarket: form.getFieldValue(createPlotFormFields.targetMarket),
            description: form.getFieldValue(createPlotFormFields.description),
            // status: form.getFieldValue(createPlotFormFields.status),
          }
        : null;

      // Chuẩn bị request cập nhật tọa độ polygon
      const plotCoordUpdateRequest: LandPlotUpdateCoordinationRequest | null = isPolygonDirty
        ? {
            landPlotId: selectedPlot.landPlotId,
            coordinationsUpdateModel: currentPolygon!.coordinates[0].map(
              ([longitude, latitude]) => ({
                longitude,
                latitude,
              }),
            ),
          }
        : null;

      // Gọi API song song nếu có thay đổi
      const [plotRes, coordRes] = await Promise.allSettled([
        plotDataUpdateRequest
          ? landPlotService.updateLandPlotInfo(plotDataUpdateRequest)
          : Promise.resolve(null),
        plotCoordUpdateRequest
          ? landPlotService.updateLandPlotCoordination(plotCoordUpdateRequest)
          : Promise.resolve(null),
      ]);

      // Kiểm tra kết quả API
      const isPlotUpdated = plotRes.status === "fulfilled" && plotRes.value?.statusCode === 200;
      const isCoordUpdated = coordRes.status === "fulfilled" && coordRes.value?.statusCode === 200;

      const plotError =
        plotRes.status === "fulfilled" && plotRes.value && plotRes.value.statusCode !== 200;
      const coordError =
        coordRes.status === "fulfilled" && coordRes.value && coordRes.value.statusCode !== 200;

      if (isPlotUpdated || isCoordUpdated) {
        toast.success(
          isPlotUpdated && isCoordUpdated
            ? MESSAGES.PLOT_AND_COORD_UPDATE_SUCCESS
            : isPlotUpdated
            ? MESSAGES.PLOT_UPDATE_SUCCESS
            : MESSAGES.COORD_UPDATE_SUCCESS,
        );
        resetForm();
        onClose();
        await fetchLandPlots?.();
      }

      if (plotError) toast.warning(MESSAGES.PLOT_UPDATE_FAILED);
      if (coordError) toast.warning(MESSAGES.COORD_UPDATE_FAILED);
    } catch (error) {
      toast.warning("Failed to update plot. Please try again!");
    } finally {
      setIsLoading(false);
    }
  };

  const handleSaveSimulate = async () => {
    if (!plotSimulate) return;
    const payload: LandPlotSimulateRequest = {
      landPlotId: plotSimulate.landPlotId,
      numberOfRows: rowsData.length,
      landRows: rowsData,
    };
    setIsLoading(true);
    try {
      const res = await landPlotService.updateLandRowOfPlot(payload);
      if (res.statusCode === 200) {
        toast.success(res.message);
        resetForm();
        onClose();
        await fetchSimulateLandPlots?.();
      } else {
        toast.warning(res.message);
      }
    } finally {
      setIsLoading(false);
    }
  };

  const resetForm = () => {
    form.resetFields();
    setCurrentStep(0);
    setCurrentPolygon(null);
    setPolygonReady(false);
    // setPolygonDimensions(0, 0, 0);
    setIsDirty(false);
    setIsPolygonDirty(false);
  };

  const confirmClose = () => {
    if (isDirty || (!isUpdate && currentPolygon) || isPolygonDirty) {
      updateConfirmModal.showModal();
    } else {
      resetForm();
      onClose();
    }
  };
  useEffect(() => {
    if (isSimulateUpdate && plotSimulate) {
      form.setFieldsValue({
        landRowCode: plotSimulate.landPlotCode,
        rowOrientation: plotSimulate.isRowHorizontal ? "Horizontal" : "Vertical",
        rowSpacing: plotSimulate.rowSpacing,
        rowsPerLine: plotSimulate.rowPerLine,
        lineSpacing: plotSimulate.lineSpacing,
      });
      setRowsData(plotSimulate.landRows);
      setCurrentStep(2);
    }
  }, [isOpen, isSimulateUpdate, plotSimulate]);

  return (
    <>
      <Drawer
        title={
          <Flex className={style.stepHeader}>
            {isUpdate || isSimulateUpdate ? (
              <Flex className={style.updateTitle}>
                <h3 className={style.editTitle}>Update Land Plot</h3>
              </Flex>
            ) : (
              <Steps
                current={currentStep}
                size="small"
                className={`${style.steps} ${styles.customSteps}`}
              >
                <Step title="Draw Plot" />
                <Step title="Set Up Rows" />
                <Step title="Confirm" />
              </Steps>
            )}

            <EditActions
              handleBtn1={currentStep === 0 || isSimulateUpdate ? confirmClose : handlePrev}
              handleBtn2={
                isSimulateUpdate ? handleSaveSimulate : isUpdate ? handleSaveUpdate : handleNext
              }
              labelBtn1={currentStep === 0 || isSimulateUpdate ? "Cancel" : "Previous"}
              labelBtn2={
                isUpdate || isSimulateUpdate
                  ? "Save Changes"
                  : currentStep === 2
                  ? "Finish"
                  : "Next"
              }
              isLoading={isLoading}
            />
          </Flex>
        }
        className={styles.customDrawer}
        placement="right"
        onClose={confirmClose}
        open={isOpen}
        width="100%"
        height="100%"
      >
        {currentStep === 0 && (
          <LandPlotCreate
            isOpen={isOpen}
            selectedPlot={selectedPlot}
            landPlots={landPlots ?? []}
            longitude={longitude}
            latitude={latitude}
            form={form}
          />
        )}
        {currentStep === 1 && (
          <RowConfiguration
            form={form}
            plotName={form.getFieldValue(createPlotFormFields.landPlotName)}
            plotLength={Number(form.getFieldValue(createPlotFormFields.length))}
            plotWidth={Number(form.getFieldValue(createPlotFormFields.width))}
          />
        )}
        {currentStep === 2 && (
          <DraggableRow
            rowsData={rowsData}
            setRowsData={setRowsData}
            isHorizontal={isHorizontal}
            rowsPerLine={Number(form.getFieldValue(createPlotFormFields.rowsPerLine))}
            // rowSpacing={Number(form.getFieldValue(createPlotFormFields.rowSpacing))}
            // lineSpacing={Number(form.getFieldValue(createPlotFormFields.lineSpacing))}
            // isHorizontal={true}
            // rowsPerLine={5}
            // rowSpacing={50}
            // lineSpacing={50}
          />
        )}
      </Drawer>
      <ConfirmModal
        visible={updateConfirmModal.modalState.visible}
        actionType="unsaved"
        onConfirm={() => {
          updateConfirmModal.hideModal();
          resetForm();
          clearPolygons();
          onClose();
        }}
        onCancel={updateConfirmModal.hideModal}
      />
    </>
  );
};

export default AddNewPlotDrawer;
