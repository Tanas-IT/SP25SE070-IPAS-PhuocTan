﻿using CapstoneProject_SP25_IPAS_BussinessObject.Entities;
using CapstoneProject_SP25_IPAS_Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CapstoneProject_SP25_IPAS_Repository.IRepository
{
    public interface IPlantRepository
    {
        public Task<IEnumerable<Plant>> GetPlantsInFarmPagin(int farmId, string? Search, int? PageIndex, int? PageSize, string? SortBy, string? Direction);
        public Task<IEnumerable<Plant>> GetPlantsInPlotPagin(int landPlotId, string? search, int? pageIndex, int? pageSize, string? sortBy, string? direction);
        public Task<Plant?> getById(int plantId);
        public Task<List<Plant>> getPlantInclude();
        public Task<IEnumerable<Plant>> GetAllForDelete(
          Expression<Func<Plant, bool>> filter = null!,
          Func<IQueryable<Plant>, IOrderedQueryable<Plant>> orderBy = null!);

        public Task<List<int>> getPlantByRowId(int rowId);
        public Task<bool> CheckIfPlantCanBeInTargetAsync(int plantId, string targetType);
        public Task<List<Plant>> GetAllPlantByFarmId(int farmId);

    }
}
