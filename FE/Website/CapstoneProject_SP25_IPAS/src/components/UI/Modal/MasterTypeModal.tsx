import { Flex, Form } from "antd";
import { useState, useEffect } from "react";
import { FormFieldModal, ModalForm } from "@/components";
import { RulesManager } from "@/utils";
import { MASTER_TYPE_SHOW_TABLE, masterTypeFormFields, SYSTEM_CONFIG_GROUP } from "@/constants";
import { GetMasterType, MasterTypeRequest } from "@/payloads";
import { useSystemConfigOptions } from "@/hooks";

type MasterTypeModelProps = {
  isProduct?: boolean;
  isOpen: boolean;
  onClose: (values: MasterTypeRequest, isUpdate: boolean) => void;
  onSave: (values: MasterTypeRequest) => void;
  isLoadingAction?: boolean;
  masterTypeData?: GetMasterType;
  typeCurrent: string;
};

const MasterTypeModel = ({
  isProduct = false,
  isOpen,
  onClose,
  onSave,
  masterTypeData,
  isLoadingAction,
  typeCurrent,
}: MasterTypeModelProps) => {
  const [form] = Form.useForm();
  const [checked, setChecked] = useState<boolean>(false);
  const isUpdate = masterTypeData !== undefined && Object.keys(masterTypeData).length > 0;
  const { options: workTargetOptions, loading } = useSystemConfigOptions(
    SYSTEM_CONFIG_GROUP.WORK,
  );

  const handleSwitchChange = (newChecked: boolean) => setChecked(newChecked);

  const resetForm = () => {
    form.resetFields();
    setChecked(false);
  };

  useEffect(() => {
    resetForm();
    if (isOpen) {
      if (isUpdate && masterTypeData) {
        form.setFieldsValue({ ...masterTypeData, [masterTypeFormFields.typeName]: typeCurrent });
        setChecked(masterTypeData.isActive);
      } else {
        form.setFieldsValue({ [masterTypeFormFields.typeName]: typeCurrent });
      }
    }
  }, [isOpen, masterTypeData]);

  const getFormData = (): MasterTypeRequest => ({
    masterTypeId: form.getFieldValue(masterTypeFormFields.masterTypeId),
    masterTypeName: form.getFieldValue(masterTypeFormFields.masterTypeName),
    masterTypeDescription: form.getFieldValue(masterTypeFormFields.masterTypeDescription),
    typeName: form.getFieldValue(masterTypeFormFields.typeName),
    minTime: form.getFieldValue(masterTypeFormFields.minTime),
    maxTime: form.getFieldValue(masterTypeFormFields.maxTime),
    isActive: checked,
    isConflict: form.getFieldValue(masterTypeFormFields.isConflict),
    target: form.getFieldValue(masterTypeFormFields.target),
    ...(form.getFieldValue(masterTypeFormFields.backgroundColor) && {
      backgroundColor:
        typeof form.getFieldValue(masterTypeFormFields.backgroundColor).toHexString === "function"
          ? form.getFieldValue(masterTypeFormFields.backgroundColor).toHexString()
          : form.getFieldValue(masterTypeFormFields.backgroundColor), // Nếu không có toHexString() thì giữ nguyên giá trị
    }),
    ...(form.getFieldValue(masterTypeFormFields.textColor) && {
      textColor:
        typeof form.getFieldValue(masterTypeFormFields.textColor).toHexString === "function"
          ? form.getFieldValue(masterTypeFormFields.textColor).toHexString()
          : form.getFieldValue(masterTypeFormFields.textColor),
    }),
    ...(form.getFieldValue(masterTypeFormFields.characteristic)?.trim() && {
      characteristic: form.getFieldValue(masterTypeFormFields.characteristic).trim(),
    }),
  });

  const handleOk = async () => {
    await form.validateFields();
    onSave(getFormData());
  };

  const handleCancel = () => onClose(getFormData(), isUpdate);

  return (
    <ModalForm
      isOpen={isOpen}
      onClose={handleCancel}
      onSave={handleOk}
      isUpdate={isUpdate}
      isLoading={isLoadingAction}
      title={
        isUpdate
          ? isProduct
            ? "Update Product"
            : "Update Type"
          : isProduct
          ? "Add New Product"
          : "Add New Type"
      }
      size="large"
    >
      <Form form={form} layout="vertical">
        <Flex gap={20}>
          <FormFieldModal
            label={isProduct ? "Product Name" : "Type Name"}
            name={masterTypeFormFields.masterTypeName}
            rules={
              isProduct
                ? RulesManager.getRequiredRules("Product Name")
                : RulesManager.getTypeNameRules()
            }
            placeholder={isProduct ? "Enter the product name" : "Enter the type name"}
          />
          <FormFieldModal label="Type" readonly={true} name={masterTypeFormFields.typeName} />
        </Flex>

        <FormFieldModal
          label="Description"
          type="textarea"
          name={masterTypeFormFields.masterTypeDescription}
          rules={RulesManager.getFarmDescriptionRules()}
          placeholder="Enter the description"
        />

        {typeCurrent === MASTER_TYPE_SHOW_TABLE.WORK && (
          <>
            <Flex justify="space-between" gap={40}>
              <FormFieldModal
                type="select"
                label="Target"
                name={masterTypeFormFields.target}
                rules={RulesManager.getTargetRules()}
                isLoading={loading}
                options={workTargetOptions}
              />
              <FormFieldModal
                type="radio"
                label="Can Overlap"
                name={masterTypeFormFields.isConflict}
                rules={RulesManager.getIsConflictRules()}
              />
            </Flex>

            <Flex justify="space-between" gap={40}>
              <FormFieldModal
                label="Min Time (Minutes)"
                name={masterTypeFormFields.minTime}
                placeholder="Enter min time"
                rules={RulesManager.getTimeRangeRules()}
              />
              <FormFieldModal
                label="Max Time (Minutes)"
                name={masterTypeFormFields.maxTime}
                placeholder="Enter max time"
                rules={[
                  ...RulesManager.getTimeRangeRules(),
                  {
                    validator: async (_: any, value: number | string) => {
                      const minValue = form.getFieldValue(masterTypeFormFields.minTime);
                      if (
                        minValue !== undefined &&
                        value !== undefined &&
                        Number(value) <= Number(minValue)
                      ) {
                        return Promise.reject(new Error("Max Time must be greater than Min Time"));
                      }
                      return Promise.resolve();
                    },
                  },
                ]}
                dependencies={[masterTypeFormFields.minTime]}
              />
            </Flex>

            <Flex justify="space-between" gap={40}>
              <FormFieldModal
                type="colorPicker"
                label="Background Color"
                name={masterTypeFormFields.backgroundColor}
                placeholder="Enter background color"
                direction="row"
              />
              <FormFieldModal
                type="colorPicker"
                label="Text Color"
                name={masterTypeFormFields.textColor}
                placeholder="Enter text color"
                direction="row"
              />
            </Flex>
          </>
        )}

        {/* {typeCurrent === MASTER_TYPE_SHOW_TABLE.CRITERIA && (
          <>
            <FormFieldModal
              type="select"
              label="Target"
              name={masterTypeFormFields.target}
              rules={RulesManager.getTargetRules()}
              options={criteriaTargetOptions}
            />
          </>
        )} */}

        {typeCurrent === MASTER_TYPE_SHOW_TABLE.CULTIVAR && (
          <>
            <FormFieldModal
              type="textarea"
              label="Characteristic"
              name={masterTypeFormFields.characteristic}
              rules={RulesManager.getCharacteristicRules()}
              placeholder="Enter characteristic"
            />
          </>
        )}

        <FormFieldModal
          type="switch"
          label="Status"
          name={masterTypeFormFields.isActive}
          onChange={handleSwitchChange}
          isCheck={checked}
          direction="row"
        />
      </Form>
    </ModalForm>
  );
};

export default MasterTypeModel;
