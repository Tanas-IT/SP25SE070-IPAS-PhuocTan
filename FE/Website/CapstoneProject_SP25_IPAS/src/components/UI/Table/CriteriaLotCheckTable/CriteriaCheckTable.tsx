import { useStyle } from "@/hooks";
import { Table, Checkbox, InputNumber } from "antd";
import style from "./CriteriaCheckTable.module.scss";
import { GetCriteriaCheck } from "@/payloads";
import { formatDateAndTime } from "@/utils";
import { usePlantLotStore } from "@/stores";
import { CRITERIA_TARGETS } from "@/constants";

interface CriteriaCheckTableProps {
  data: GetCriteriaCheck[];
  target: string;
  handleValueCheckChange: (criteriaId: number, valueCheck: number | null) => void;
}

const CriteriaCheckTable: React.FC<CriteriaCheckTableProps> = ({
  data,
  target,
  handleValueCheckChange,
}) => {
  const { styles } = useStyle();
  const isCondition = target === CRITERIA_TARGETS["Plantlot Condition"] ? true : false;
  const { lot } = usePlantLotStore();
  if (!lot) return;

  const columns = [
    {
      title: "Priority",
      dataIndex: "priority",
      key: "priority",
      align: "center" as const,
    },
    {
      title: "Name",
      dataIndex: "criteriaName",
      key: "criteriaName",
      align: "center" as const,
      width: 200,
    },
    {
      title: "Description",
      dataIndex: "description",
      key: "description",
      align: "center" as const,
      width: 300,
    },
    {
      title: "Min Value",
      dataIndex: "minValue",
      key: "minValue",
      align: "center" as const,
      width: 120,
    },
    {
      title: "Max Value",
      dataIndex: "maxValue",
      key: "maxValue",
      align: "center" as const,
      width: 120,
    },
    {
      title: "Unit",
      dataIndex: "unit",
      key: "unit",
      align: "center" as const,
    },
    {
      title: "Value Check",
      dataIndex: "valueChecked",
      key: "valueChecked",
      align: "center" as const,
      render: (_: any, record: GetCriteriaCheck, index: number) => (
        <InputNumber
          placeholder="Enter number..."
          // value={
          //   record.valueChecked !== undefined && record.valueChecked !== 0
          //     ? record.valueChecked
          //     : null
          // }
          value={record.valueChecked !== undefined ? record.valueChecked : null}
          readOnly={isCondition && lot.inputQuantity !== undefined && lot.inputQuantity !== null}
          onChange={(value) => handleValueCheckChange(record.criteriaId, value ?? null)}
          min={0}
        />
      ),
      width: 100,
    },
    {
      title: "Check Date",
      dataIndex: "checkedDate",
      key: "checkedDate",
      align: "center" as const,
      render: (_: any, record: GetCriteriaCheck) =>
        record.checkedDate ? formatDateAndTime(record.checkedDate) : "-",
    },
    {
      title: "Check Interval Days ",
      dataIndex: "frequencyDate",
      key: "frequencyDate",
      align: "center" as const,
    },
    {
      title: "Is Passed",
      key: "isPassed",
      align: "center" as const,
      width: 100,
      render: (_: any, record: GetCriteriaCheck) => (
        <Checkbox className={styles.customCheckbox} checked={record.isPassed} disabled />
      ),
    },
  ].filter((col): col is Exclude<typeof col, null> => col !== null);

  return (
    <div className={style.criteriaTableWrapper}>
      <Table
        className={`${style.criteriaTable} ${styles.customeTable2}`}
        columns={columns}
        dataSource={data}
        pagination={false}
        bordered
        rowKey="criteriaId"
      />
    </div>
  );
};

export default CriteriaCheckTable;
