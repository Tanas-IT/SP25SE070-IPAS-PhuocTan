import { FileResource } from "@/types";

interface GraftedPlantSummary {
  graftedPlantID: number;
  graftedPlantCode: string;
  graftedPlantName: string;
  status: string;
  graftedDate: string;
  isCompleted: boolean;
  plantLotID: number;
}

interface HarvestSummary {
  productHarvestHistoryId: number;
  harvestDate: string;
  quantity: number;
  unit: string;
  harvestCount: number;
  productType: string;
  marketValue: number;
}

export interface GetPlant {
  plantId: number;
  plantCode: string;
  plantName: string; // ko bắt buộc
  plantIndex: number;
  healthStatus: string;
  createDate: Date;
  plantingDate: string;
  plantReferenceId: number;
  plantReferenceCode: string;
  description: string;
  masterTypeId: number;
  masterTypeName: string;
  growthStageName: string;
  imageUrl: string | File;
  landPlotId: number;
  landRowId: number;
  rowIndex: number;
  landPlotName: string;
  isDead: boolean;
  isPassed: boolean;
  graftedPlants: GraftedPlantSummary[];
  harvestHistory: HarvestSummary[];
}

export interface GetPlantDetail extends GetPlant {
  plantReferenceName: string;
  plantLotName: string;
  characteristic: string;
  passedDate: string;
}

export interface GetPlantSelect {
  id: string;
  code: string;
  name: string;
}

export interface plantSimulate {
  plantId: number;
  plantCode: string;
  plantIndex: number;
  healthStatus: string;
}

export interface GetPlantOfRowSelect {
  id: number;
  code: string;
  name: string;
}

export interface Plant {
  plantId: number;
  plantName: string;
}

export interface LandRow {
  landRowId: number;
  rowIndex: number;
  plants: Plant[];
}

export interface GetPlantTargetResponse {
  landPlotId: number;
  landPlotName: string;
  unit: string;
  rows: LandRow[];
  plants: Plant[];
  plantLots: any[];
  graftedPlants: any[];
}

export interface GetPlantGrowthHistory {
  plantId: number;
  plantGrowthHistoryId: number;
  plantGrowthHistoryCode: string;
  issueName: string;
  content: string;
  noteTakerName: string;
  noteTakerAvatar: string;
  createDate: string;
  numberImage: number;
  numberVideos: number;
  resources: FileResource[];
  userId: number;
}

export interface GetPlantRecord {
  productHarvestHistoryId: number;
  unit: string;
  actualQuantity: number;
  recordBy: string;
  avartarRecord: string;
  recordDate: string;
  harvestDate: string;
  productName: string;
  cropName: string;
}
