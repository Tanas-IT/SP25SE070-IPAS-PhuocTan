import { axiosAuth } from "@/api";
import {
  AdminFarmRequest,
  ApiResponse,
  FarmDocumentRequest,
  FarmRequest,
  GetData,
  GetFarmDocuments,
  GetFarmInfo,
  GetFarmPicker,
  GetUser,
  GetUserInFarm,
} from "@/payloads";
import { buildParams, getFileFormat, getUserId } from "@/utils";

export const getFarms = async (
  currentPage?: number,
  rowsPerPage?: number,
  sortField?: string,
  sortDirection?: string,
  searchValue?: string,
  additionalParams?: Record<string, any>,
): Promise<GetData<GetFarmInfo>> => {
  const params = buildParams(
    currentPage,
    rowsPerPage,
    sortField,
    sortDirection,
    searchValue,
    additionalParams,
  );
  const res = await axiosAuth.axiosJsonRequest.get("farms", { params });
  const apiResponse = res.data as ApiResponse<GetData<GetFarmInfo>>;
  return apiResponse.data as GetData<GetFarmInfo>;
};

export const getFarmsOfUser = async (): Promise<ApiResponse<GetFarmPicker[]>> => {
  const userId = getUserId();
  const res = await axiosAuth.axiosJsonRequest.get(`farms/get-farm-of-user/${userId}`);
  const apiResponse = res.data as ApiResponse<GetFarmPicker[]>;
  return apiResponse;
};

export const getFarm = async (farmId: string): Promise<ApiResponse<GetFarmInfo>> => {
  const res = await axiosAuth.axiosJsonRequest.get(`farms/${farmId}`);
  const apiResponse = res.data as ApiResponse<GetFarmInfo>;
  return apiResponse;
};

export const updateFarmInfo = async (farm: FarmRequest): Promise<ApiResponse<GetFarmInfo>> => {
  const res = await axiosAuth.axiosJsonRequest.put("farms/update-farm-info", farm);
  const apiResponse = res.data as ApiResponse<GetFarmInfo>;
  return apiResponse;
};

export const updateFarm = async (farm: AdminFarmRequest): Promise<ApiResponse<GetFarmInfo>> => {
  const res = await axiosAuth.axiosJsonRequest.put("farms/update-farm-info", farm);
  const apiResponse = res.data as ApiResponse<GetFarmInfo>;
  return apiResponse;
};

export const updateFarmLogo = async (image: File): Promise<ApiResponse<{ logoUrl: string }>> => {
  const formData = new FormData();
  formData.append("FarmLogo", image);
  const res = await axiosAuth.axiosMultipartForm.patch("farms/update-farm-logo", formData);
  const apiResponse = res.data as ApiResponse<{ logoUrl: string }>;
  return apiResponse;
};

export const createFarm = async (farm: FarmRequest): Promise<ApiResponse<Object>> => {
  const formData = new FormData();

  formData.append("FarmName", farm.farmName);
  formData.append("LogoUrl", farm.farmLogo);
  formData.append("Description", farm.description);
  formData.append("Area", farm.area);
  formData.append("SoilType", farm.soilType);
  formData.append("ClimateZone", farm.climateZone);
  formData.append("Address", farm.address);
  formData.append("Province", farm.province);
  formData.append("District", farm.district);
  formData.append("Ward", farm.ward);
  formData.append("Longitude", farm.longitude.toString());
  formData.append("Latitude", farm.latitude.toString());

  const res = await axiosAuth.axiosMultipartForm.post(`farms`, formData);
  const apiResponse = res.data as ApiResponse<Object>;
  return apiResponse;
};

export const deleteFarm = async (ids: number[] | string[]): Promise<ApiResponse<GetFarmInfo>> => {
  const res = await axiosAuth.axiosJsonRequest.patch(`farms/softed-delete-farm/${ids[0]}`);
  const apiResponse = res.data as ApiResponse<GetFarmInfo>;
  return apiResponse;
};

export const updateStatusFarm = async (ids: number[] | string[]): Promise<ApiResponse<Object>> => {
  const res = await axiosAuth.axiosJsonRequest.put(`farms/activate`, ids);
  const apiResponse = res.data as ApiResponse<Object>;
  return apiResponse;
};

export const getFarmDocuments = async (
  farmId: string,
): Promise<ApiResponse<GetFarmDocuments[]>> => {
  const res = await axiosAuth.axiosJsonRequest.get(
    `legal-documents/get-legal-document-of-farm/${farmId}`,
  );
  const apiResponse = res.data as ApiResponse<GetFarmDocuments[]>;
  return apiResponse;
};

export const createFarmDocuments = async (
  doc: FarmDocumentRequest,
): Promise<ApiResponse<Object>> => {
  const formData = new FormData();
  formData.append("LegalDocumentType", doc.legalDocumentType);
  formData.append("LegalDocumentName", doc.legalDocumentName);

  if (doc.resources && doc.resources.length > 0) {
    doc.resources.forEach((fileResource, index) => {
      const format = getFileFormat(fileResource.file.type);
      if (format) {
        formData.append(`Resources[${index}].fileFormat`, format);
        formData.append(`Resources[${index}].file`, fileResource.file);
      }
    });
  }

  const res = await axiosAuth.axiosMultipartForm.post(`legal-documents`, formData);
  const apiResponse = res.data as ApiResponse<Object>;
  return apiResponse;
};

export const deleteFarmDocuments = async (docId: number | string): Promise<ApiResponse<Object>> => {
  const res = await axiosAuth.axiosJsonRequest.delete(`legal-documents/${docId}`);
  const apiResponse = res.data as ApiResponse<Object>;
  return apiResponse;
};

export const updateFarmDocuments = async (
  doc: FarmDocumentRequest,
): Promise<ApiResponse<Object>> => {
  const formData = new FormData();
  formData.append("LegalDocumentId", doc.LegalDocumentId);
  formData.append("LegalDocumentType", doc.legalDocumentType);
  formData.append("LegalDocumentName", doc.legalDocumentName);

  if (doc.resources && doc.resources.length > 0) {
    doc.resources.forEach((fileResource, index) => {
      if (fileResource.file) {
        formData.append(`Resources[${index}].fileFormat`, fileResource.file.type);
        formData.append(`Resources[${index}].file`, fileResource.file);
      } else {
        formData.append(`Resources[${index}].resourceID`, fileResource.resourceID);
      }
    });
  }
  const res = await axiosAuth.axiosMultipartForm.put(`legal-documents`, formData);
  const apiResponse = res.data as ApiResponse<Object>;
  return apiResponse;
};

export const getUserInFarmByRole = async (
  farmId: string,
  listRole: number[],
): Promise<ApiResponse<GetUserInFarm[]>> => {
  const queryParams = listRole.map((role) => `listRole=${role}`).join("&");

  const res = await axiosAuth.axiosJsonRequest.get(
    `farms/get-users-farm-by-role?farmId=${farmId}&${queryParams}`,
  );

  const apiResponse = res.data as ApiResponse<GetUserInFarm[]>;
  return apiResponse;
};
