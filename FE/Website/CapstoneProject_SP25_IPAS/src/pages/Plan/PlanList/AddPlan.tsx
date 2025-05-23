import React, { useEffect, useState } from "react";
import { Form, DatePicker, Select, Row, Col, Divider, Modal, Flex } from "antd";
import { CustomButton, InfoField, Section, Tooltip } from "@/components";
import style from "./PlanList.module.scss";
import dayjs, { Dayjs } from "dayjs";
import DaySelector from "./DaySelector";
import AssignEmployee from "./AssignEmployee";
import { Icons } from "@/assets";
import { useNavigate } from "react-router-dom";
import { PATHS } from "@/routes";
import {
  useCropCurrentOption,
  useGrowthStageOptions,
  useLocalStorage,
  useMasterTypeOptions,
  useUnsavedChangesWarning,
  usePlantOfRowOptions,
  useGraftedPlantOptions,
} from "@/hooks";
import {
  fetchProcessesOfFarm,
  getFarmId,
  getGrowthStageOfProcess,
  getTypeOfProcess,
  getUserId,
  isDayInRange,
  planTargetOptions,
  RulesManager,
} from "@/utils";
import { addPlanFormFields, frequencyOptions, MASTER_TYPE } from "@/constants";
import {
  cropService,
  masterTypeService,
  planService,
  plantLotService,
  processService,
  worklogService,
} from "@/services";
import { toast } from "react-toastify";
import { PlanRequest } from "@/payloads/plan/requests/PlanRequest";
import PlanTarget from "./PlanTarget";
import isBetween from "dayjs/plugin/isBetween";
import { SelectOption } from "@/types";

import { EmployeeWithSkills } from "@/payloads/worklog";

dayjs.extend(isBetween);

type OptionType<T = string | number> = { value: T; label: string };

const AddPlan = () => {
  const navigate = useNavigate();
  const [form] = Form.useForm();
  const userId = Number(getUserId());
  const farmId = Number(getFarmId());
  const { getAuthData } = useLocalStorage();
  const authData = getAuthData();

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [isFormDirty, setIsFormDirty] = useState(false);
  const [selectedIds, setSelectedIds] = useState<number[]>([]);
  const [selectedEmployees, setSelectedEmployees] = useState<EmployeeWithSkills[]>([]);
  const [selectedReporter, setSelectedReporter] = useState<number | null>(null);
  const [errorMessage, setErrorMessage] = useState("");
  const [processFarmOptions, setProcessFarmOptions] = useState<OptionType<number>[]>([]);
  const [employee, setEmployee] = useState<EmployeeWithSkills[]>([]);
  const [assignorId, setAssignorId] = useState<number>();
  const [frequency, setFrequency] = useState<string>("none");
  const [customDates, setCustomDates] = useState<Dayjs[]>([]); // Frequency: none
  const [dayOfWeek, setDayOfWeek] = useState<number[]>([]); // Frequency: weekly
  const [dayOfMonth, setDayOfMonth] = useState<number[]>([]); // Frequency: monthly
  const [selectedGrowthStage, setSelectedGrowthStage] = useState<number[]>([]);
  const [isLockedGrowthStage, setIsLockedGrowthStage] = useState<boolean>(false);
  const [checked, setChecked] = useState<boolean>(false);
  const [targetType, setTargetType] = useState<string>();
  const [isTargetDisabled, setIsTargetDisabled] = useState<boolean>(true);
  const [isCropDisabled, setIsCropDisabled] = useState<boolean>(false);
  const [masterTypeGrafting, setMasterTypeGrafting] = useState<number[]>([]);

  const { options: growthStageOptions } = useGrowthStageOptions(false);

  const { options: plantsOptions } = usePlantOfRowOptions(null);
  const { options: graftedPlantsOptions } = useGraftedPlantOptions(farmId);
  const { options: cropOptions } = useCropCurrentOption();
  const { options: processTypeOptions } = useMasterTypeOptions(MASTER_TYPE.WORK, false);

  const [selectedCrop, setSelectedCrop] = useState<number | null>(null);
  const [landPlotOfCropOptions, setLandPlotOfCropOptions] = useState<SelectOption[]>([]);
  const [plantLotOptions, setPlantLotOptions] = useState<SelectOption[]>([]);

  const [dateRange, setDateRange] = useState<[Dayjs, Dayjs] | null>(null);
  const [dateError, setDateError] = useState<string | null>(null);
  const [showWorkTypeWarning, setShowWorkTypeWarning] = useState(false);

  useEffect(() => {
    if (selectedCrop) {
      form.setFieldValue("listLandPlotOfCrop", []);
      cropService.getLandPlotOfCrop(selectedCrop).then((data) => {
        setLandPlotOfCropOptions(
          data.data.map((item) => ({ value: item.landPlotId, label: item.landPlotName })),
        );
      });
    } else {
      setLandPlotOfCropOptions([]);
    }
  }, [selectedCrop]);

  useEffect(() => {
    const updateProcessFarmOptions = async () => {
      if (targetType === "graftedPlant") {
        const result = await masterTypeService.getProcessTypeSelect("Grafting");

        if (result.statusCode === 200) {
          setMasterTypeGrafting(result.data.map((m) => m.id));
          setProcessFarmOptions(
            await processService.getProcessOfFarmByMasterType(result.data.map((m) => m.id)),
          );
        } else {
          setProcessFarmOptions(await processService.getProcessOfFarmByMasterType([]));
        }
      } else if (targetType === "plantLot") {
        const result = await masterTypeService.getProcessTypeSelect("PlantLot");

        if (result.statusCode === 200) {
          setProcessFarmOptions(
            await processService.getProcessOfFarmByMasterType(result.data.map((m) => m.id)),
          );
        } else {
          setProcessFarmOptions(await processService.getProcessOfFarmByMasterType([]));
        }
      } else {
        setProcessFarmOptions(await fetchProcessesOfFarm(farmId, true));
      }
    };

    updateProcessFarmOptions();
  }, [targetType]);

  const handlePlanTargetChange = async (target: string) => {
    setTargetType(target);
    form.setFieldsValue({
      planTargetModel: [],
    });

    if (target === "graftedPlant") {
      setProcessFarmOptions(await processService.getProcessOfFarmByMasterType(masterTypeGrafting));
      form.setFieldValue("growthStageId", undefined);
      form.setFieldValue("processId", undefined);
      setIsLockedGrowthStage(false);
      setIsTargetDisabled(true);
      setSelectedGrowthStage([]);
    } else if (target === "plantLot") {
      form.setFieldValue("cropId", undefined);
      setIsCropDisabled(true);
      setProcessFarmOptions(await fetchProcessesOfFarm(farmId, true));
      setIsTargetDisabled(true);
      form.setFieldValue("processId", undefined);
      // setIsLockedGrowthStage(false);
    } else {
      // form.setFieldValue("processId", undefined);
      // setIsLockedGrowthStage(false);
      setIsCropDisabled(false);
      setProcessFarmOptions(await fetchProcessesOfFarm(farmId, true));
      setIsTargetDisabled(false);
    }
  };

  const handleChangeProcess = async (processId: number | undefined) => {
    if (processId) {
      const growthStageId = await getGrowthStageOfProcess(processId);

      form.setFieldValue("processId", processId);
      const masterTypeId = await getTypeOfProcess(processId);
      form.setFieldValue("masterTypeId", Number(masterTypeId));
      setIsLockedGrowthStage(true);
      // neu process co target la grafting
      if ((await masterTypeService.IsMasterTypeHasTarget(masterTypeId, "Grafting")).data) {
        setTargetType("graftedPlant");
        form.setFieldValue("planTarget", "graftedPlant");
        form.setFieldValue("growthStageId", undefined);
        // setProcessTypeOptions((await masterTypeService.getProcessTypeSelect("Grafting")).data.map((pt) => ({
        //   value: pt.id,
        //   label: pt.name
        // })))
        setIsLockedGrowthStage(true);
      } else if ((await masterTypeService.IsMasterTypeHasTarget(masterTypeId, "PlantLot")).data) {
        setTargetType("plantLot");
        form.setFieldValue("planTarget", "plantLot");
        form.setFieldValue("growthStageId", undefined);

        // setProcessTypeOptions((await masterTypeService.getProcessTypeSelect("PlantLot")).data.map((pt) => ({
        //   value: pt.id,
        //   label: pt.name
        // })))
        setIsLockedGrowthStage(true);
      } else {
        form.setFieldValue("growthStageId", [growthStageId]);
        if (growthStageId) {
          setSelectedGrowthStage([growthStageId]);
        }
        form.setFieldsValue({ planTargetModel: [] });
      }
    } else {
      form.setFieldValue("processId", undefined);
      form.setFieldValue("growthStageId", undefined);
      form.setFieldValue("masterTypeId", undefined);
      setIsLockedGrowthStage(false);
      // setProcessTypeOptions([]);
    }
  };
  const handleDateRangeChange = (dates: (Dayjs | null)[] | null) => {
    if (!dates || dates.some((date) => date === null)) {
      setDateRange(null);
      setDateError("Please select a valid date range!");
      form.setFieldsValue({ dateRange: null });
      return;
    }

    const [startDate, endDate] = dates as [Dayjs, Dayjs];
    setDateRange([startDate, endDate]);
    setDateError(null);
    form.setFieldsValue({ dateRange: [startDate, endDate] });

    if (frequency === "None" && customDates.length === 1) {
      Modal.confirm({
        title: "Adjust Date Range",
        content:
          "You have selected only one custom date. Do you want to adjust the date range to match this date?",
        onOk: () => {
          const newDateRange = [customDates[0], customDates[0]] as [Dayjs, Dayjs];
          setDateRange(newDateRange);
          form.setFieldsValue({ dateRange: newDateRange });
        },
        onCancel: () => {
          setCustomDates([]);
        },
      });
    }
  };

  const handleDateChange = (dates: Dayjs[]) => {
    if (!dateRange) {
      setDateError("Please select the date range first!");
      return;
    }
    const [startDate, endDate] = dateRange;
    const validDates = dates.filter((date) => date.isBetween(startDate, endDate, "day", "[]"));

    if (validDates.length === 0) {
      setDateError("Selected dates must be within the date range.");
      return;
    }

    setDateError(null);
    setCustomDates(validDates);
    if (frequency === "None" && validDates.length === 1) {
      const isDateRangeAdjusted =
        startDate.isSame(validDates[0], "day") && endDate.isSame(validDates[0], "day");
      if (!isDateRangeAdjusted) {
        Modal.confirm({
          title: "Adjust Date Rangetttttt",
          content:
            "You have selected only one custom date. Do you want to adjust the date range to match this date?",
          onOk: () => {
            const newDateRange = [validDates[0], validDates[0]] as [Dayjs, Dayjs];
            setDateRange(newDateRange);
            form.setFieldsValue({ dateRange: newDateRange });
          },
          onCancel: () => {
            setCustomDates([]);
          },
        });
      }
    }
  };

  const handleFrequencyChange = (value: string) => {
    setFrequency(value);

    if (dateRange && dateRange[0].isSame(dateRange[1], "day")) {
      Modal.confirm({
        title: "Adjust Date Range",
        content: "The selected date range is too short. Do you want to adjust it?",
        onOk: () => {
          const newEndDate =
            value === "Weekly" ? dateRange[0].add(6, "day") : dateRange[0].add(1, "month");
          setDateRange([dateRange[0], newEndDate]);
        },
        onCancel: () => {
          // Không làm gì
        },
      });
    }
  };

  const { isModalVisible, handleCancelNavigation, handleConfirmNavigation } =
    useUnsavedChangesWarning(isFormDirty);

  const handleReporterChange = (userId: number) => {
    setSelectedReporter(userId);
  };

  // const handleAssignMember = () => setIsModalOpen(true);
  const handleAssignClick = () => {
    if (!form.getFieldValue("masterTypeId")) {
      setShowWorkTypeWarning(true);
      return;
    }
    setIsModalOpen(true);
  };

  const handleConfirmAssign = () => {
    setAssignorId(userId);
    // if (selectedIds.length === 0) {
    //   setErrorMessage("Please select at least one employee.");
    //   return;
    // }

    setSelectedEmployees(employee.filter((m) => selectedIds.includes(Number(m.userId))));
    setIsModalOpen(false);
  };

  const handleWeeklyDaySelection = (days: number[]) => {
    setDayOfWeek(days);
  };

  const handleMonthlyDaySelection = (days: number[]) => {
    setDayOfMonth(days);
  };

  const handleSaveDays = async (days: number[], type: "weekly" | "monthly"): Promise<boolean> => {
    if (!dateRange) {
      setDateError("Please select the date range first!");
      return false;
    }
    const [startDate, endDate] = dateRange;

    const validDays = days.filter((day) => isDayInRange(day, startDate, endDate, type));

    if (validDays.length === 0) {
      setDateError(
        `All selected ${
          type === "weekly" ? "days" : "dates"
        } are not within the date range. Please select again.`,
      );
      return false;
    }

    // Có ngày không hợp lệ
    if (validDays.length < days.length) {
      setDateError(
        `Some selected ${
          type === "weekly" ? "days" : "dates"
        } are not within the date range. Only valid ${
          type === "weekly" ? "days" : "dates"
        } will be saved.`,
      );
      if (type === "weekly") {
        setDayOfWeek(validDays);
      } else if (type === "monthly") {
        setDayOfMonth(validDays);
      }
    } else {
      setDateError(null);
    }

    if (validDays.length === 1) {
      const selectedDay = validDays[0];
      let targetDate = startDate.clone();
      if (type === "weekly") {
        while (targetDate.day() !== selectedDay) {
          targetDate = targetDate.add(1, "day");
        }
      } else if (type === "monthly") {
        targetDate = startDate.date(selectedDay);
      }
      const isDateRangeAdjusted =
        startDate.isSame(targetDate, "day") && endDate.isSame(targetDate, "day");

      if (!isDateRangeAdjusted) {
        return new Promise((resolve) => {
          Modal.confirm({
            title: "Adjust Date Range",
            content: `You have selected only one ${
              type === "weekly" ? "day" : "date"
            }. Do you want to adjust the date range to match this ${
              type === "weekly" ? "day" : "date"
            }?`,
            onOk: () => {
              const selectedDay = validDays[0];
              let targetDate = startDate.clone();

              if (type === "weekly") {
                while (targetDate.day() !== selectedDay) {
                  targetDate = targetDate.add(1, "day");
                }
              } else if (type === "monthly") {
                targetDate = startDate.date(selectedDay);
              }

              const newDateRange = [targetDate, targetDate] as [Dayjs, Dayjs];
              setDateRange(newDateRange);
              form.setFieldsValue({ dateRange: newDateRange });
              resolve(true);
            },
            onCancel: () => {
              if (type === "weekly") {
                setDayOfWeek([]);
                resolve(false);
              } else if (type === "monthly") {
                setDayOfMonth([]);
                resolve(false);
              }
            },
          });
        });
      }
    }

    return true;
  };

  const handleSubmit = async (values: any) => {
    try {
      const { dateRange, timeRange, planTargetModel, frequency, graftedPlant, plantLot } = values;
      const startDate = new Date(dateRange?.[0]);
      const endDate = new Date(dateRange?.[1]);

      const adjustedStartDate = new Date(
        startDate.getTime() - startDate.getTimezoneOffset() * 60000,
      );
      const adjustedEndDate = new Date(endDate.getTime() - endDate.getTimezoneOffset() * 60000);

      const startTime = timeRange?.[0]?.toDate().toLocaleTimeString();
      const endTime = timeRange?.[1]?.toDate().toLocaleTimeString();
      if (frequency === "Weekly" && dayOfWeek.length === 0) {
        toast.warning("Please select at least one custom date for Weekly frequency.");
        return;
      }

      if (frequency === "Monthly" && dayOfMonth.length === 0) {
        toast.warning("Please select at least one day for Monthly frequency.");
        return;
      }

      if (assignorId === undefined) {
        toast.warning("Please select at least one employee.");
        return;
      }

      if (!selectedCrop && planTargetModel.length === 0 && targetType === "regular") {
        toast.warning("Please select at least one plan target.");
        return;
      }
      const graftedPlantIDs = graftedPlant || [];
      const plantLotIDs = plantLot || [];

      let formattedPlanTargetModel;
      // Format planTargetModel
      if (targetType === "regular") {
        formattedPlanTargetModel = planTargetModel.map((target: any) => {
          return {
            landPlotID: target.landPlotID ?? 0,
            landRowID: target.landRowID
              ? Array.isArray(target.landRowID)
                ? target.landRowID
                : [target.landRowID]
              : [],
            plantID: target.plantID ?? [],
            graftedPlantID: [],
            plantLotID: [],
            unit: target.unit,
          };
        });
      } else if (targetType === "plantLot") {
        formattedPlanTargetModel = [
          {
            landPlotID: undefined,
            landRowID: [],
            plantID: [],
            graftedPlantID: [],
            plantLotID: plantLotIDs,
            unit: targetType,
          },
        ];
      } else if (targetType === "graftedPlant") {
        formattedPlanTargetModel = [
          {
            landPlotID: undefined,
            landRowID: [],
            plantID: [],
            graftedPlantID: graftedPlantIDs,
            plantLotID: [],
            unit: targetType,
          },
        ];
      }

      const planData: PlanRequest = {
        planName: values.planName,
        planDetail: values.planDetail,
        notes: values.notes || "",
        cropId: values.cropId,
        processId: values.processId,
        growthStageId: values.growthStageId,
        frequency: values.frequency,
        isActive: values.isActive,
        masterTypeId: values.masterTypeId,
        assignorId,
        responsibleBy: values.responsibleBy || "",
        pesticideName: values.pesticideName || "",
        maxVolume: values.maxVolume || 0,
        minVolume: values.minVolume || 0,
        isDelete: values.isDelete || false,
        listEmployee: selectedEmployees.map((employee) => ({
          userId: employee.userId,
          isReporter: employee.userId === selectedReporter,
        })),
        dayOfWeek,
        dayOfMonth,
        customDates: customDates.map(date => date.format('YYYY-MM-DD')),
        startDate: adjustedStartDate.toISOString(),
        endDate: adjustedEndDate.toISOString(),
        startTime: startTime,
        endTime: endTime,
        planTargetModel: formattedPlanTargetModel,
        listLandPlotOfCrop: values.listLandPlotOfCrop,
      };

      const result = await planService.addPlan(planData);

      if (result.statusCode === 200) {
        await toast.success(result.message);
        navigate(`${PATHS.PLAN.PLAN_LIST}?sf=createDate&sd=desc`);
        form.resetFields();
      } else {
        toast.error(result.message);
      }

      setIsFormDirty(false);
    } catch (error) {
      toast.error("Failed to create plan. Please try again later.");
    } finally {
      setIsLoading(false);
    }
  };

  const fetchData = async () => {
    setProcessFarmOptions(await fetchProcessesOfFarm(farmId, true));

    // setPlantLotOptions((await usePlantLotOptions()).options);
    const plantLots = await plantLotService.getPlantLotSelected();
    setPlantLotOptions(plantLots);
  };

  const fetchEmployee = async () => {
    const response = await worklogService.getEmployeesByWorkSkill(Number(farmId));
    if (response.statusCode === 200) {
      setEmployee(response.data);
    }
  };

  const workTypeId = Form.useWatch(addPlanFormFields.masterTypeId, form);

  useEffect(() => {
    const fetchEmployee = async () => {
      if (workTypeId) {
        const response = await worklogService.getEmployeesByWorkSkill(Number(farmId), workTypeId);
        if (response.statusCode === 200) {
          setEmployee(response.data);
        }
      } else {
        setEmployee([]);
      }
    };

    fetchEmployee();
  }, [workTypeId]);

  useEffect(() => {
    fetchData();
  }, []);

  return (
    <div className={style.contentSectionBody}>
      <Flex gap={40} align="center">
        <Tooltip title="Back to List">
          <Icons.back
            className={style.backIcon}
            size={20}
            onClick={() => {
              if (isFormDirty) {
                Modal.confirm({
                  title: "Are you sure you want to leave?",
                  content: "All unsaved changes will be lost.",
                  onOk: () => navigate(PATHS.PLAN.PLAN_LIST),
                });
              } else {
                navigate(PATHS.PLAN.PLAN_LIST);
              }
            }}
          />
        </Tooltip>
        <Flex justify="space-between" style={{width: "100%"}}>
          <h2 className={style.title}>Add Plan</h2>
          {/* FORM ACTIONS */}
          <Flex gap={10} justify="end" className={style.btnGroup}>
            <CustomButton label="Clear" isCancel handleOnClick={() => form.resetFields()} />
            <CustomButton
              label="Add Plan"
              htmlType="submit"
              isLoading={isLoading}
              disabled={isLoading}
            />
          </Flex>
        </Flex>
      </Flex>
      <Divider />
      <Form
        form={form}
        layout="vertical"
        className={style.form}
        onFinish={handleSubmit}
        initialValues={{ isActive: true }}
      >
        {/* BASIC INFORMATION */}
        <Section
          title="Basic Information"
          subtitle="Enter the basic information for the care plan."
        >
          <Row gutter={16}>
            <Col span={12}>
              <Flex vertical>
                <InfoField
                  label="Process Name"
                  name={addPlanFormFields.processId}
                  options={processFarmOptions}
                  isEditing={true}
                  type="select"
                  hasFeedback={false}
                  onChange={(value) => handleChangeProcess(value)}
                />
                <div
                  style={{ marginTop: "-20px", textAlign: "right" }}
                  onClick={async () => {
                    const masterTypeId = await getTypeOfProcess(form.getFieldValue("processId"));
                    if (masterTypeId === 14) {
                      form.setFieldValue("planTarget", undefined);
                      form.setFieldValue("graftedPlant", undefined);
                      setTargetType(undefined);
                    }
                    handleChangeProcess(undefined);
                  }}
                >
                  <a style={{ fontSize: "14px", color: "blueviolet", textDecoration: "underline" }}>
                    Clear
                  </a>
                </div>
              </Flex>
            </Col>
            <Col span={12}>
              <InfoField
                label="Growth Stage"
                name={addPlanFormFields.growthStageID}
                options={growthStageOptions}
                isEditing={!isLockedGrowthStage}
                onChange={(value) => {
                  setSelectedGrowthStage(value);
                  if (targetType === "regular") {
                    form.setFieldsValue({ planTargetModel: [] });
                  }
                }}
                type="select"
                multiple
                hasFeedback={false}
              />
            </Col>
          </Row>
          <Row gutter={16}>
            <Col span={12}>
              <InfoField
                label="Select Plan Target"
                name={addPlanFormFields.planTarget}
                options={planTargetOptions}
                rules={RulesManager.getPlanTargetRules()}
                isEditing={true}
                type="select"
                hasFeedback={false}
                onChange={(value) => handlePlanTargetChange(value)}
              />
            </Col>
            {targetType === "plantLot" && (
              <Col span={12}>
                <InfoField
                  label="Select Plant Lot"
                  name={addPlanFormFields.plantLot}
                  options={plantLotOptions}
                  multiple
                  // rules={RulesManager.getPlantLotRules()}
                  isEditing={true}
                  type="select"
                  hasFeedback={false}
                />
              </Col>
            )}

            {targetType === "graftedPlant" && (
              <Col span={12}>
                <InfoField
                  label="Select Grafted Plant"
                  name={addPlanFormFields.graftedPlant}
                  options={graftedPlantsOptions}
                  // rules={RulesManager.getGraftedPlantRules()}
                  isEditing={true}
                  type="select"
                  multiple
                  hasFeedback={false}
                />
              </Col>
            )}
          </Row>
          <Row gutter={16}>
            <Col span={12}>
              <Flex vertical>
                <InfoField
                  label="Crop Name"
                  name={addPlanFormFields.cropId}
                  // rules={RulesManager.getCropRules()}
                  options={cropOptions}
                  isEditing={!isCropDisabled}
                  type="select"
                  hasFeedback={false}
                  onChange={(value) => {
                    setSelectedCrop(value);
                    if (targetType === "regular") {
                      form.setFieldValue("planTarget", "regular");
                    }

                    setIsTargetDisabled(true);
                    form.setFieldsValue({ [addPlanFormFields.listLandPlotOfCrop]: undefined });
                    form.setFieldsValue({ planTargetModel: [] });
                  }}
                />
                <div
                  style={{ marginTop: "-20px", textAlign: "right" }}
                  onClick={() => {
                    setSelectedCrop(null);
                    form.setFieldValue("cropId", undefined);
                    form.setFieldValue("listLandPlotOfCrop", []);
                    setIsTargetDisabled(false);
                  }}
                >
                  <a style={{ fontSize: "14px", color: "blueviolet", textDecoration: "underline" }}>
                    Clear
                  </a>
                </div>
              </Flex>
            </Col>
            <Col span={12}>
              <InfoField
                label="Land Plot"
                name={addPlanFormFields.listLandPlotOfCrop}
                rules={[
                  {
                    validator: (_: any, value: any) => {
                      if (selectedCrop && (!value || value.length === 0)) {
                        return Promise.reject(
                          new Error("Please select at least one Land Plot for the Crop!"),
                        );
                      }
                      return Promise.resolve();
                    },
                  },
                ]}
                options={landPlotOfCropOptions}
                isEditing={selectedCrop ? true : false}
                type="select"
                multiple
                hasFeedback={false}
              />
            </Col>
          </Row>
          <InfoField
            label="Name"
            name={addPlanFormFields.planName}
            rules={RulesManager.getPlanNameRules()}
            isEditing={true}
            hasFeedback={true}
            placeholder="Enter care plan name"
          />
          <InfoField
            label="Detail"
            name={addPlanFormFields.planDetail}
            isEditing={true}
            hasFeedback={false}
            type="textarea"
            placeholder="Enter care plan details"
          />
          <InfoField
            label="Note"
            name={addPlanFormFields.notes}
            isEditing={true}
            hasFeedback={false}
            type="textarea"
            placeholder="Enter care plan notes"
          />

          <InfoField
            label="Active"
            name={addPlanFormFields.isActive}
            isEditing={true}
            type="switch"
            value={checked}
            hasFeedback={false}
            onChange={(value) => setChecked(value)}
          />
        </Section>

        <Divider className={style.divider} />

        {/* TASK ASSIGNMENT */}
        <Section title="Task Assignment" subtitle="Assign tasks and define work type.">
          <InfoField
            label="Type of Work"
            name={addPlanFormFields.masterTypeId}
            options={processTypeOptions}
            rules={RulesManager.getPlanTypeRules()}
            isEditing={true}
            type="select"
            hasFeedback={false}
          />
          <AssignEmployee
            members={selectedEmployees}
            onAssign={handleAssignClick}
            onReporterChange={handleReporterChange}
            selectedReporter={selectedReporter}
          />
          <Modal
            title="No Plan Selected"
            open={showWorkTypeWarning}
            onOk={() => setShowWorkTypeWarning(false)}
            onCancel={() => setShowWorkTypeWarning(false)}
          >
            <p>Please select type of work before assigning employees.</p>
          </Modal>
          {errorMessage && <div style={{ color: "red", marginTop: 8 }}>{errorMessage}</div>}
          <Modal
            title="Assign Members"
            open={isModalOpen}
            onOk={handleConfirmAssign}
            onCancel={() => setIsModalOpen(false)}
          >
            <Select
              mode="multiple"
              style={{ width: "100%" }}
              placeholder="Select employees"
              value={selectedIds}
              onChange={setSelectedIds}
              optionLabelProp="label"
            >
              {employee.map((emp) => (
                <Select.Option key={emp.userId} value={emp.userId} label={emp.fullName}>
                  <div
                    style={{
                      display: "flex",
                      alignItems: "center",
                      gap: 12,
                      padding: "8px 12px",
                      borderRadius: 8,
                      transition: "all 0.2s",
                    }}
                  >
                    {/* Avatar */}
                    <div
                      style={{
                        position: "relative",
                        width: 32,
                        height: 32,
                        flexShrink: 0,
                      }}
                    >
                      <img
                        src={emp.avatarURL}
                        alt={emp.fullName}
                        style={{
                          width: "100%",
                          height: "100%",
                          borderRadius: "50%",
                          objectFit: "cover",
                          border: "2px solid #e6f7ff",
                        }}
                        crossOrigin="anonymous"
                      />
                    </div>

                    <div
                      style={{
                        flex: 1,
                        minWidth: 0,
                      }}
                    >
                      <div
                        style={{
                          fontWeight: 500,
                          color: "rgba(0, 0, 0, 0.88)",
                          whiteSpace: "nowrap",
                          overflow: "hidden",
                          textOverflow: "ellipsis",
                        }}
                      >
                        {emp.fullName}
                      </div>

                      <div
                        style={{
                          display: "flex",
                          gap: 6,
                          marginTop: 4,
                          flexWrap: "wrap",
                        }}
                      >
                        {emp.skillWithScore.slice(0, 3).map((skill) => (
                          <div
                            key={skill.skillName}
                            style={{
                              display: "flex",
                              alignItems: "center",
                              background: skill.score >= 7 ? "#f6ffed" : "#fafafa",
                              border: `1px solid ${skill.score >= 7 ? "#b7eb8f" : "#d9d9d9"}`,
                              borderRadius: 4,
                              padding: "2px 6px",
                              fontSize: 12,
                              lineHeight: 1,
                            }}
                          >
                            <Icons.grade
                              width={12}
                              height={12}
                              style={{
                                marginRight: 4,
                                color: "yellow",
                              }}
                            />
                            <span
                              style={{
                                color: "rgba(0, 0, 0, 0.65)",
                              }}
                            >
                              {skill.skillName} <strong>{skill.score}</strong>
                            </span>
                          </div>
                        ))}
                        {emp.skillWithScore.length > 3 && (
                          <div
                            style={{
                              background: "#f0f0f0",
                              borderRadius: 4,
                              padding: "2px 6px",
                              fontSize: 12,
                            }}
                          >
                            +{emp.skillWithScore.length - 3}
                          </div>
                        )}
                      </div>
                    </div>
                  </div>
                </Select.Option>
              ))}
            </Select>
          </Modal>
          <label className={style.createdBy}>
            {" "}
            <span>Created by: </span>
            {authData.fullName}
          </label>
        </Section>

        <Divider className={style.divider} />

        {/* SCHEDULE */}
        <Section title="Schedule" subtitle="Define the schedule for the care plan.">
          <InfoField
            label="Date Range"
            name="dateRange"
            type="dateRange"
            rules={[{ required: true, message: "Please select the date range!" }]}
            isEditing
            onChange={handleDateRangeChange}
          />

          <InfoField
            label="Time Range"
            name="timeRange"
            type="timeRange"
            rules={[{ required: true, message: "Please select the time range!" }]}
            isEditing
          />

          <InfoField
            label="Frequency"
            name={addPlanFormFields.frequency}
            options={frequencyOptions}
            rules={[{ required: true, message: "Please select the frequency!" }]}
            isEditing
            type="select"
            hasFeedback={false}
            onChange={handleFrequencyChange}
          />

          {frequency === "Weekly" && (
            <Form.Item
              label="Select Days of Week"
              rules={[{ required: true, message: "Please select the days of week!" }]}
              validateStatus={dateError ? "error" : ""}
              help={dateError}
            >
              <DaySelector
                onSelectDays={handleWeeklyDaySelection}
                onSave={async (days) => {
                  const isSuccess = await handleSaveDays(days, "weekly");
                  return isSuccess;
                }}
                selectedDays={dayOfWeek}
                type="weekly"
              />
            </Form.Item>
          )}

          {frequency === "Monthly" && (
            <Form.Item
              label="Select Dates"
              rules={[{ required: true, message: "Please select the dates!" }]}
              validateStatus={dateError ? "error" : ""}
              help={dateError}
            >
              <DaySelector
                onSelectDays={handleMonthlyDaySelection}
                onSave={(days) => handleSaveDays(days, "monthly")}
                selectedDays={dayOfMonth}
                type="monthly"
              />
            </Form.Item>
          )}

          {frequency === "None" && (
            <Form.Item
              label="Select Specific Dates"
              rules={[{ required: true, message: "Please select the dates!" }]}
              validateStatus={dateError ? "error" : ""}
              help={dateError}
            >
              <DatePicker
                format="YYYY-MM-DD"
                multiple
                value={customDates}
                onChange={handleDateChange}
                disabledDate={(current) => current && current.isBefore(dayjs().endOf("day"))}
              />
            </Form.Item>
          )}
        </Section>

        <Divider className={style.divider} />
        <PlanTarget
          plants={plantsOptions}
          plantLots={plantLotOptions}
          graftedPlants={graftedPlantsOptions}
          selectedGrowthStage={selectedGrowthStage}
          hasSelectedCrop={isTargetDisabled}
          onClearTargets={() => form.setFieldsValue({ planTargetModel: [] })}
        />

        <Divider className={style.divider} />

        {/* FORM ACTIONS */}
        <Flex gap={10} justify="end" className={style.btnGroup}>
          <CustomButton label="Clear" isCancel handleOnClick={() => form.resetFields()} />
          <CustomButton
            label="Add Plan"
            htmlType="submit"
            isLoading={isLoading}
            disabled={isLoading}
          />
        </Flex>
      </Form>
      {isModalVisible && (
        <Modal
          title="Are you sure you want to leave this page?"
          visible={isModalVisible}
          onOk={handleConfirmNavigation}
          onCancel={handleCancelNavigation}
        >
          <p>Every changes will be lost.</p>
        </Modal>
      )}
    </div>
  );
};

export default AddPlan;
