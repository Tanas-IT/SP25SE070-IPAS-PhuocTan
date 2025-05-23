import { Flex } from "antd";
import style from "./ProcessList.module.scss";
import { ActionMenuPlant, ConfirmModal, NavigationDot, SectionTitle, Table } from "@/components";
import { GetPlant } from "@/payloads";
import { useFetchData, useModal, useTableAdd, useTableDelete } from "@/hooks";
import { useEffect, useState } from "react";
import { getOptions } from "@/utils";
import { processService, userService } from "@/services";
import { GetProcess, GetProcessList } from "@/payloads/process";
import { processColumns } from "./ProcessColumns";
import ActionMenuProcess from "@/components/UI/ActionMenu/ActionMenuProcess";
import ProcessFilter from "./ProcessFilter";
import TableTitle from "./TableTitle";
import ProcessModal from "./AddProcessModal";
import { useNavigate } from "react-router-dom";
import AIGenerateModal from "./AIGenerateModal";
import { ROUTES } from "@/constants";


function ProcessList() {
  const navigate = useNavigate();
  const formModal = useModal<GetProcessList>();
  const aiModal = useModal();
  const deleteConfirmModal = useModal<{ ids: number[] }>();
  const [filters, setFilters] = useState({
    createDateFrom: "",
    createDateTo: "",
    growthStage: [] as string[],
    masterTypeName: [] as string[],
    isActive: [] as string[],
  });

  const {
    data,
    fetchData,
    totalRecords,
    totalPages,
    sortField,
    rotation,
    handleSortChange,
    currentPage,
    rowsPerPage,
    searchValue,
    handlePageChange,
    handleRowsPerPageChange,
    handleSearch,
    isLoading,
  } = useFetchData<GetProcessList>({
    fetchFunction: (page, limit, sortField, sortDirection, searchValue) =>
      processService.getProcesses(page, limit, sortField, sortDirection, searchValue, "21", filters),
  });

  useEffect(() => {
    fetchData();
  }, [currentPage, rowsPerPage, sortField, searchValue]);

  const updateFilters = (key: string, value: any) => {
    setFilters((prev) => ({ ...prev, [key]: value }));
  };

  const handleApply = () => {
    fetchData();
  };

  const handleClear = () => {
    setFilters({
      createDateFrom: "",
      createDateTo: "",
      growthStage: [],
      masterTypeName: [],
      isActive: [],
    });
  };

  const { handleAdd } = useTableAdd({
    addService: processService.createProcess,
    fetchData: fetchData,
    onSuccess: () => formModal.hideModal(),
  });

  const filterContent = (
    <ProcessFilter
      filters={filters}
      updateFilters={updateFilters}
      onClear={handleClear}
      onApply={handleApply}
    />
  );
  const { handleDelete } = useTableDelete(
    {
      deleteFunction: processService.deleteProcess,
      fetchData,
      onSuccess: () => {
        deleteConfirmModal.hideModal();
      },
    },
    {
      currentPage,
      rowsPerPage,
      totalRecords,
      handlePageChange,
    },
  );

  const handleAIGenerate = (processName: string, isSample: boolean, masterTypeId: number, planTargetInProcess: number) => {
    navigate(`/ai-process-details`, { state: { processName, isSample, masterTypeId, planTargetInProcess } });
    aiModal.hideModal();
  };


  return (
    <Flex className={style.container}>
      <SectionTitle title="Process Management" totalRecords={totalRecords} />
      <Flex className={style.table}>
        <Table
          columns={processColumns}
          rows={data}
          rowKey="processCode"
          title={
            <TableTitle
              onSearch={handleSearch}
              filterContent={filterContent}
              addLabel="Add New Process"
              onAdd={() => formModal.showModal()}
              onAIGenerate={() => aiModal.showModal()}
            />
          }
          handleSortClick={handleSortChange}
          selectedColumn={sortField}
          rotation={rotation}
          currentPage={currentPage}
          rowsPerPage={rowsPerPage}
          isLoading={isLoading}
          caption="Process Management Table"
          notifyNoData="No data to display"
          renderAction={(process: GetProcessList) => <ActionMenuProcess id={process.processId} onDelete={() => deleteConfirmModal.showModal({ ids: [process.processId] })} />}
          isOnRowEvent={true}
          onRowDoubleClick={(record) => navigate(ROUTES.PROCESS_DETAIL(record.processId))}
        />

        <NavigationDot
          totalPages={totalPages}
          currentPage={currentPage}
          rowsPerPage={rowsPerPage}
          onPageChange={handlePageChange}
          rowsPerPageOptions={getOptions(totalRecords)}
          onRowsPerPageChange={handleRowsPerPageChange}
        />
      </Flex>
      <ProcessModal
        isOpen={formModal.modalState.visible}
        onClose={formModal.hideModal}
        onSave={handleAdd} />
      <AIGenerateModal
        isOpen={aiModal.modalState.visible}
        onClose={aiModal.hideModal}
        onGenerate={handleAIGenerate}
      />
      <ConfirmModal
        visible={deleteConfirmModal.modalState.visible}
        onConfirm={() => handleDelete(deleteConfirmModal.modalState.data?.ids)}
        onCancel={deleteConfirmModal.hideModal}
        itemName="Process"
        actionType="delete"
      />
    </Flex>
  );
}

export default ProcessList;
