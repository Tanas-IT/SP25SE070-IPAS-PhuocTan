import { Form } from "antd";
import { FormFieldModal, ModalForm } from "@/components";
import { feedbackFormFields } from "@/constants";
import { Flex } from "antd";
import { useEffect, useState } from "react";
import { getUserId, RulesManager } from "@/utils";
import { CreateFeedbackRequest } from "@/payloads";
import { feedbackService, worklogService } from "@/services";
import { toast } from "react-toastify";
import { TaskFeedback } from "@/payloads/worklog";

type FeedbackModalProps = {
  isOpen: boolean;
  onClose: () => void;
  onSave: (values: { feedback: string; status: string }) => void;
  worklogId: number;
  managerId: number;
  onSuccess: () => void;
  feedbackData?: TaskFeedback;
  statuss?: string;
};

const FeedbackModal = ({
  isOpen,
  onClose,
  onSave,
  worklogId,
  managerId,
  onSuccess,
  feedbackData,
  statuss,
}: FeedbackModalProps) => {
  const [form] = Form.useForm();
  const [status, setStatus] = useState<string>("");
  const [statusOptions, setStatusOptions] = useState<{ value: string; label: string }[]>([]);
  const [loadingStatuses, setLoadingStatuses] = useState<boolean>(false);
  const isUpdate = feedbackData !== undefined && Object.keys(feedbackData).length > 0;
  console.log("feedbackData", feedbackData);
  

  useEffect(() => {
    const fetchStatusOptions = async () => {
      if (isOpen && statusOptions.length === 0) {
        setLoadingStatuses(true);
        try {
          const response = await worklogService.getWorklogStatus();
          if (response.statusCode === 200) {
            const statuses = (response.data as { status: string[] }).status;
            const options = statuses.map(status => ({
              value: status,
              label: status,
            }));
            setStatusOptions(options);
          } else {
            toast.warning(response.message);
          }
        } catch (error) {
          console.error('Error fetching worklog statuses:', error);
          toast.warning('Failed to load status options');
        } finally {
          setLoadingStatuses(false);
        }
      }
    };
    fetchStatusOptions();
  }, [isOpen]);

  useEffect(() => {
    if (isOpen) {
      if (isUpdate && feedbackData) {
        form.setFieldsValue({
          content: feedbackData.content,
          status: statuss || "Redo",
          reason: feedbackData.reasonDelay,
        });
        setStatus(feedbackData.status || "Redo");
      } else {
        form.resetFields();
        // form.setFieldsValue({ status: "Done" });
        // setStatus("Done");
      }
    }
  }, [isOpen, feedbackData, form]);

  const handleSave = async () => {
    const values = await form.validateFields();
    let result;
    if (isUpdate) {
      const payloadUpdate: CreateFeedbackRequest = {
        taskFeedbackId: feedbackData.taskFeedbackId,
        content: values.content,
        managerId: Number(getUserId()),
        worklogId: feedbackData.workLogId,
        status: values.status,
        reason: values.reason,
      };
      console.log("payloadUpdate", payloadUpdate);
      result = await feedbackService.updateFeedback(payloadUpdate);
    } else {
      const payload: CreateFeedbackRequest = {
        content: values.content,
        managerId,
        worklogId,
        status: values.status,
        reason: values.reason,
      };
      result = await feedbackService.createFeedback(payload);
    }

    if (result.statusCode === 200) {
      toast.success(result.message);
      form.resetFields();
      onSuccess();
    } else {
      toast.warning(result.message);
    }
    onClose();
  };

  return (
    <ModalForm
      isOpen={isOpen}
      onClose={onClose}
      isUpdate={isUpdate}
      title={isUpdate ? "Update Feedback" : "Add New Feedback"}
      onSave={handleSave}
    >
      <Form form={form} layout="vertical">
        <Flex vertical gap={10}>
          <FormFieldModal
            label="Content"
            type="textarea"
            rules={RulesManager.getContentFeedbackRules()}
            placeholder="Enter the content"
            name={feedbackFormFields.content}
          />
          <FormFieldModal
            label="Worklog Status"
            rules={RulesManager.getStatusWorklogFeedbackRules()}
            name={feedbackFormFields.status}
            options={statusOptions}
            type="select"
            onChange={(value) => setStatus(value)}
            placeholder={loadingStatuses ? "Loading statuses..." : "Select status"}
          />
          {status === "Redo" && (
            <FormFieldModal
              label="Reason for Redo"
              type="textarea"
              rules={[{ required: true, message: "Please provide a reason" }]}
              placeholder="Enter the reason"
              name={feedbackFormFields.reason}
            />
          )}
          {status === "Failed" && (
            <FormFieldModal
              label="Reason for Failed"
              type="textarea"
              rules={[{ required: true, message: "Please provide a reason" }]}
              placeholder="Enter the reason"
              name={feedbackFormFields.reason}
            />
          )}
        </Flex>
      </Form>
    </ModalForm>
  );
};

export default FeedbackModal;