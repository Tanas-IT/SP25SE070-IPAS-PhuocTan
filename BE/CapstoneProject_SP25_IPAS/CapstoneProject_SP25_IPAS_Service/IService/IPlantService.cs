﻿using CapstoneProject_SP25_IPAS_BussinessObject.RequestModel.PlantRequest;
using CapstoneProject_SP25_IPAS_Common.Utils;
using CapstoneProject_SP25_IPAS_Service.Base;

namespace CapstoneProject_SP25_IPAS_Service.IService
{
    public interface IPlantService
    {
        public Task<BusinessResult> getById(int plantId);
        public Task<BusinessResult> getByCode(string plantCode);
        //public Task<BusinessResult> getAllPlantOfPlot(int landplotId, PaginationParameter paginationParameter);
        //public Task<BusinessResult> getAllPlantOfFarm(int farmId, PaginationParameter paginationParameter);
        public Task<BusinessResult> createPlant(PlantCreateRequest plantCreateRequest);
        public Task<BusinessResult> updatePlant(PlantUpdateRequest plantUpdateRequest);
        public Task<BusinessResult> deletePlant(int plantId);
        public Task<BusinessResult> deleteMultiplePlant(List<int> ids);
        public Task<BusinessResult> ImportPlantAsync(ImportExcelRequest request);
        public Task<BusinessResult> getPlantInPlotForSelected(int plotId);
        public Task<BusinessResult> getPlantInRowForSelected(int rowId);
        public Task<BusinessResult> getPlantActFuncionForSelected(int farmId, int? plotid, int? rowId, string? actFunction);
        public Task<BusinessResult> getPlantPagin(GetPlantPaginRequest request, PaginationParameter paginationParameter);
        public Task<BusinessResult> SoftedMultipleDelete(List<int> plantIdList);
        public Task<BusinessResult> getPlantNotYetPlanting(int farmId);
        public Task<BusinessResult> getPlantByGrowthActiveFunc(int farmId, string activeFunction);
        public Task<BusinessResult> DeadPlantMark(int plantId);
        public Task<BusinessResult> ExportExcel(GetPlantPaginRequest request);

    }
}
