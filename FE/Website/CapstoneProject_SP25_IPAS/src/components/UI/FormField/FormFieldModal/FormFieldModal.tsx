import { useStyle } from "@/hooks";
import {
  Select,
  Form,
  Flex,
  DatePicker,
  Input,
  Switch,
  ColorPicker,
  Image,
  Button,
  Upload,
  TimePicker,
  Radio,
} from "antd";
import style from "./FormFieldModal.module.scss";
import { TagRender } from "@/components";
import { DATE_FORMAT } from "@/utils";
import { ReactNode, useEffect, useState } from "react";
import { Icons } from "@/assets";
import { toast } from "react-toastify";
import dayjs from "dayjs";

interface FormFieldModalProps {
  label: string;
  description?: string;
  name?: string | (string | number)[];
  rules?: any[];
  type?:
    | "text"
    | "textarea"
    | "date"
    | "dateRange"
    | "select"
    | "switch"
    | "colorPicker"
    | "image"
    | "time"
    | "radio"
    | "password";
  options?: { value: string | number; label: string | ReactNode }[];
  value?: string | string[] | number | undefined;
  image?: File | string;
  readonly?: boolean;
  onChange?: (value: any) => void;
  isLoading?: boolean;
  isSearch?: boolean;
  isCheck?: boolean;
  isSingleRadio?: boolean;
  hasFeedback?: boolean;
  placeholder?: string;
  radioLabels?: { yes: string; no: string };
  direction?: "row" | "col";
  dependencies?: string[];
  checkedChildren?: string;
  unCheckedChildren?: string;
  disable?: boolean;
  multiple?: boolean;
}

const FormFieldModal: React.FC<FormFieldModalProps> = ({
  label,
  description,
  name,
  rules = [],
  type = "text",
  options = [],
  value,
  image,
  readonly = false,
  onChange,
  isLoading = false,
  isSearch = true,
  isCheck = false,
  isSingleRadio = false,
  hasFeedback = true,
  placeholder = `Enter ${label.toLowerCase()}`,
  radioLabels = { yes: "Yes", no: "No" },
  direction = "col",
  dependencies,
  checkedChildren,
  unCheckedChildren,
  disable = false,
  multiple = false,
}) => {
  const { styles } = useStyle();
  const isRequired = rules.some((rule) => rule.required);
  const [imageUrl, setImageUrl] = useState<string | null>();

  useEffect(() => {
    if (typeof image === "string" && image) {
      setImageUrl(image);
    } else {
      setImageUrl(null);
    }
  }, [image]);

  const handleUpload = (file: File) => {
    const isImage = file.type.startsWith("image/");
    if (!isImage) {
      toast.warning("Only image files (JPG, PNG, GIF, WEBP) are allowed!");
      return Upload.LIST_IGNORE;
    }
    // Đọc file và cập nhật logo tạm thời
    const reader = new FileReader();
    reader.onload = (e) => {
      if (e.target?.result) {
        const newLogoUrl = e.target.result as string;
        setImageUrl(newLogoUrl);
        onChange?.(file);
      }
    };
    reader.readAsDataURL(file);

    return false;
  };

  const handleRemove = () => {
    setImageUrl(null);
    onChange?.(null);
  };

  const renderInput = () => {
    switch (type) {
      case "image":
        return (
          <div className={style.imageUpload}>
            {imageUrl ? (
              <div className={style.previewContainer}>
                <Image
                  crossOrigin="anonymous"
                  src={imageUrl}
                  alt="Uploaded"
                  className={style.previewImage}
                />
                <Button
                  type="text"
                  icon={<Icons.delete />}
                  onClick={handleRemove}
                  className={style.deleteButton}
                />
              </div>
            ) : (
              <Upload
                showUploadList={false}
                beforeUpload={handleUpload}
                maxCount={1}
                accept="image/png, image/jpeg, image/jpg, image/gif, image/webp"
              >
                <Button icon={<Icons.plus />}>Upload Image</Button>
              </Upload>
            )}
          </div>
        );
      case "radio":
        return (
          <Radio.Group onChange={(e) => onChange?.(e.target.value)} value={value}>
            <Radio value={true}>{radioLabels.yes}</Radio>
            {!isSingleRadio && <Radio value={false}>{radioLabels.no}</Radio>}
          </Radio.Group>
        );
      case "textarea":
        return (
          <Input.TextArea
            placeholder={placeholder}
            onChange={onChange}
            readOnly={readonly}
            maxLength={255}
          />
        );
      case "date":
        return (
          <DatePicker
            className={style.date}
            format={DATE_FORMAT}
            onChange={(date) => onChange?.(date)}
          />
        );
      case "dateRange":
        return (
          <DatePicker.RangePicker
            format={DATE_FORMAT}
            onChange={(dates) => onChange?.(dates)}
            className={style.date}
          />
        );
      case "select":
        return (
          <Select
            placeholder={placeholder ?? `Select ${label.toLowerCase()}`}
            className={`${style.select} ${styles.customSelect}`}
            tagRender={TagRender}
            options={options}
            allowClear
            showSearch={isSearch}
            loading={isLoading}
            onChange={onChange}
            disabled={disable}
            mode={multiple ? "multiple" : undefined}
          />
        );
      case "switch":
        return (
          <Switch
            checkedChildren={checkedChildren || "Active"}
            unCheckedChildren={unCheckedChildren || "Inactive"}
            checked={isCheck}
            onChange={onChange}
            className={`${styles.customSwitch} ${isCheck ? style.active : style.inActive}`}
          />
        );
      case "colorPicker":
        return (
          <ColorPicker
            defaultValue="#1677ff"
            format="hex"
            size="small"
            showText
            className={`${style.colorPicker}`}
          />
        );
      case "time":
        return (
          <TimePicker.RangePicker
            onChange={(time) => onChange?.(time)}
            defaultOpenValue={dayjs("00:00:00", "HH:mm:ss")}
            className={style.timePicker}
          />
        );
      case "password":
        return (
          <Input.Password
            placeholder={placeholder}
            onChange={onChange}
            value={value}
            readOnly={readonly}
            maxLength={255}
          />
        );
      default:
        return (
          <Input
            placeholder={placeholder}
            onChange={onChange}
            value={value}
            readOnly={readonly}
            maxLength={255}
          />
        );
    }
  };

  return (
    <Flex className={`${style.formSection} ${style[direction]}`}>
      <Flex className={style.formSectionTitle}>
        <label className={style.formTitle}>
          {label.endsWith(":") ? label.slice(0, -1) : label}:{" "}
          {isRequired && <span style={{ color: "red" }}>*</span>}
        </label>
        {description && <span className={style.formDescription}>{description}</span>}
      </Flex>
      <Form.Item
        name={name}
        rules={rules}
        hasFeedback={!!rules?.length}
        className={`${type === "text" || type === "textarea" ? styles.customInput2 : ""}`}
        dependencies={dependencies}
        style={{ width: "100%" }}
      >
        {renderInput()}
      </Form.Item>
    </Flex>
  );
};

export default FormFieldModal;
