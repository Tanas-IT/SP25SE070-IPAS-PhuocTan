import { Button, Flex, Tag, Tooltip } from "antd";
import style from "./Details.module.scss";
import { Icons } from "@/assets";
import { ActionMenuGraftedPlant, CustomButton, CuttingGraftedModal } from "@/components";
import { useGraftedPlantStore } from "@/stores";
import { useModal } from "@/hooks";
import { GetGraftedPlantDetail } from "@/payloads";
import { GRAFTED_STATUS, healthStatusColors } from "@/constants";
import { useState } from "react";
import { graftedPlantService } from "@/services";
import { toast } from "react-toastify";

const GraftedPlantSectionHeader = ({
  onApplyCriteria,
  formModal,
  deleteConfirmModal,
  markAsDeadModal,
  onAddToLot,
  removeFromLotConfirm,
  convertToPlantModal,
  onAddNewIssue,
  onExport,
}: {
  onApplyCriteria?: () => void;
  formModal?: ReturnType<typeof useModal<GetGraftedPlantDetail>>;
  deleteConfirmModal?: ReturnType<typeof useModal<{ id: number }>>;
  markAsDeadModal?: ReturnType<typeof useModal<{ id: number }>>;
  onAddToLot?: ReturnType<typeof useModal<{ id: number }>>;
  removeFromLotConfirm?: ReturnType<typeof useModal<{ id: number }>>;
  convertToPlantModal?: ReturnType<typeof useModal<{ id: number }>>;
  onAddNewIssue?: () => void;
  onExport?: () => void;
}) => {
  const { graftedPlant, setGraftedPlant, markForRefetch } = useGraftedPlantStore();
  const cuttingGraftedModal = useModal();
  const [isLoading, setIsLoading] = useState(false);

  if (!graftedPlant) return;

  const handleCompleteAndCut = async (lotId: number) => {
    setIsLoading(true);
    try {
      var res = await graftedPlantService.updateIsCompletedAndCutting(
        graftedPlant.graftedPlantId,
        lotId,
      );
      if (res.statusCode === 200) {
        setGraftedPlant({ ...graftedPlant, isCompleted: true });
        markForRefetch();
        toast.success(res.message);
      } else {
        toast.warning(res.message);
      }
    } finally {
      setIsLoading(false);
      cuttingGraftedModal.hideModal();
    }
  };

  return (
    <Flex className={style.contentSectionHeader}>
      <Flex className={style.contentSectionTitle}>
        <Flex className={style.contentSectionTitleLeft}>
          <label className={style.title}>{graftedPlant.graftedPlantName}</label>
          <Tooltip title="Grafted Plant">
            <Icons.tag className={style.iconTag} />
          </Tooltip>
          <Tag
            className={style.statusTag}
            color={healthStatusColors[graftedPlant.status] || "default"}
          >
            {graftedPlant.status || "Unknown"}
          </Tag>
          <Flex className={style.actionButtons} gap={20}>
            {!graftedPlant.isDead &&
              (!graftedPlant.isCompleted ? (
                <Button type="primary" onClick={cuttingGraftedModal.showModal} ghost>
                  <Icons.check /> Mark as Completed
                </Button>
              ) : (
                <Flex gap={10}>
                  <Tag color="green" className={style.passedTag}>
                    ✅ Grafted Plant Completed
                  </Tag>
                </Flex>
              ))}
          </Flex>
        </Flex>

        {!onApplyCriteria && formModal && (
          <Flex>
            <ActionMenuGraftedPlant
              isDetail={true}
              graftedPlant={graftedPlant}
              onEdit={() => formModal?.showModal(graftedPlant)}
              onDelete={() => deleteConfirmModal?.showModal({ id: graftedPlant.graftedPlantId })}
              onMarkAsDead={() => markAsDeadModal?.showModal({ id: graftedPlant.graftedPlantId })}
              onAddToLot={() => onAddToLot?.showModal({ id: graftedPlant.graftedPlantId })}
              onRemoveFromLot={() =>
                removeFromLotConfirm?.showModal({ id: graftedPlant.graftedPlantId })
              }
              onConvertToPlant={() =>
                convertToPlantModal?.showModal({ id: graftedPlant.graftedPlantId })
              }
            />
          </Flex>
        )}
        <Flex className={style.actionBtns}>
          {onExport && (
            <CustomButton label="Export" icon={<Icons.download />} handleOnClick={onExport} />
          )}
          {onAddNewIssue && !graftedPlant.isDead && graftedPlant.status !== GRAFTED_STATUS.USED && (
            <CustomButton
              label="Add New Issue"
              icon={<Icons.plus />}
              handleOnClick={onAddNewIssue}
            />
          )}
          {onApplyCriteria && !graftedPlant.isDead && (
            <CustomButton
              label="Add New Criteria"
              icon={<Icons.plus />}
              handleOnClick={onApplyCriteria}
              disabled={graftedPlant.isCompleted}
            />
          )}
        </Flex>
      </Flex>
      <label className={style.subTitle}>Code: {graftedPlant.graftedPlantCode}</label>
      <CuttingGraftedModal
        isOpen={cuttingGraftedModal.modalState.visible}
        onClose={cuttingGraftedModal.hideModal}
        onSave={handleCompleteAndCut}
        isLoadingAction={isLoading}
      />
    </Flex>
  );
};

export default GraftedPlantSectionHeader;
