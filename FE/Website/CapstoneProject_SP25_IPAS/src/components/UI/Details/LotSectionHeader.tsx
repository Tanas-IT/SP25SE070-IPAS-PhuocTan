import { Button, Flex, Tag, Tooltip } from "antd";
import style from "./Details.module.scss";
import { Icons } from "@/assets";
import { ActionMenuLot, ConfirmModal, CustomButton } from "@/components";
import { usePlantLotStore } from "@/stores";
import { plantLotService } from "@/services";
import { toast } from "react-toastify";
import { useModal } from "@/hooks";
import { GetPlantLotDetail } from "@/payloads";
import FillPlantsModal from "./FillPlantsModal";
import { LOT_STATUS, lotStatusColors } from "@/constants";

const LotSectionHeader = ({
  isCriteria = false,
  onApplyCriteria,
  formModal,
  deleteConfirmModal,
  onExport,
}: {
  isCriteria?: boolean;
  onApplyCriteria?: () => void;
  formModal?: ReturnType<typeof useModal<GetPlantLotDetail>>;
  deleteConfirmModal?: ReturnType<typeof useModal<{ id: number }>>;
  onExport?: () => void;
}) => {
  const { lot, setLot, markForRefetch } = usePlantLotStore();
  const updateConfirmModal = useModal();
  const updateUsedConfirmModal = useModal();
  const fillPlantsModal = useModal();

  if (!lot) return;
  const handleMarkAsPassed = async () => {
    try {
      var res = await plantLotService.updateIsCompletedLot(lot.plantLotId, true);
      if (res.statusCode === 200) {
        if (lot.inputQuantity == null || lot.lastQuantity == null) {
          markForRefetch();
        } else {
          setLot({ ...lot, isPassed: true });
        }
        toast.success(`Lot ${lot.plantLotName} marked as Passed!`);
      } else {
        toast.warning(res.message);
      }
    } finally {
      updateConfirmModal.hideModal();
    }
  };

  const handleMarkAsUsed = async () => {
    try {
      var res = await plantLotService.updateIsUsedLot(lot.plantLotId);
      if (res.statusCode === 200) {
        setLot({ ...lot, status: LOT_STATUS.USED });
        toast.success(res.message);
        updateUsedConfirmModal.hideModal();
      } else {
        toast.warning(res.message);
      }
    } finally {
      updateConfirmModal.hideModal();
    }
  };

  return (
    <Flex className={style.contentSectionHeader}>
      <Flex className={style.contentSectionTitle}>
        <Flex className={style.contentSectionTitleLeft}>
          <label className={style.title}>{lot.plantLotName}</label>
          <Tooltip title="Plant Lot">
            <Icons.tag className={style.iconTag} />
          </Tooltip>
          <Tag className={style.statusTag} color={lotStatusColors[lot.status] || "default"}>
            {lot.status || "Unknown"}
          </Tag>
          <Flex className={style.actionButtons} gap={20}>
            {!lot.isPassed ? (
              <Button type="primary" onClick={updateConfirmModal.showModal} ghost>
                <Icons.check /> Mark as Completed
              </Button>
            ) : (
              <Flex gap={10}>
                <Tag color="green" className={style.passedTag}>
                  ✅ Lot Completed
                </Tag>
                {lot.lastQuantity !== lot.usedQuantity && lot.status !== LOT_STATUS.USED && (
                  <CustomButton
                    label="Fill Empty Plots"
                    icon={<Icons.plantFill />}
                    handleOnClick={fillPlantsModal.showModal}
                  />
                )}

                {lot.isFromGrafted && lot.status !== LOT_STATUS.USED && (
                  <Button type="primary" onClick={updateUsedConfirmModal.showModal} ghost>
                    <Icons.check /> Mark as Used
                  </Button>
                )}
              </Flex>
            )}
          </Flex>
        </Flex>

        <Flex className={style.actionBtns}>
          {onExport && (
            <CustomButton label="Export" icon={<Icons.download />} handleOnClick={onExport} />
          )}
          {isCriteria && (
            <CustomButton
              label="Add New Criteria"
              icon={<Icons.plus />}
              handleOnClick={onApplyCriteria}
              disabled={lot.isPassed}
            />
          )}
        </Flex>
        {!isCriteria && (
          <Flex>
            <ActionMenuLot
              isCompleted={lot.isPassed}
              onEdit={() => formModal?.showModal(lot)}
              onDelete={() => deleteConfirmModal?.showModal({ id: lot.plantLotId })}
            />
          </Flex>
        )}
      </Flex>
      <label className={style.subTitle}>Code: {lot.plantLotCode}</label>
      <ConfirmModal
        visible={updateConfirmModal.modalState.visible}
        onConfirm={() => handleMarkAsPassed()}
        onCancel={updateConfirmModal.hideModal}
        actionType="update"
        title="Mark as Completed"
        description="Are you sure you want to mark this Lot as completed? This action cannot be undone."
      />
      <ConfirmModal
        visible={updateUsedConfirmModal.modalState.visible}
        onConfirm={() => handleMarkAsUsed()}
        onCancel={updateUsedConfirmModal.hideModal}
        actionType="update"
        title="Mark as Used"
        description="Are you sure you want to mark this Lot as used? This action cannot be undone and will update the status accordingly."
      />
      <FillPlantsModal
        isOpen={fillPlantsModal.modalState.visible}
        onClose={fillPlantsModal.hideModal}
        onSave={fillPlantsModal.hideModal}
        isLoadingAction={false}
      />
    </Flex>
  );
};

export default LotSectionHeader;
