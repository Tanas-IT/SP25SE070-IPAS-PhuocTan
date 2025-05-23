﻿using AutoMapper;
using CapstoneProject_SP25_IPAS_BussinessObject.BusinessModel.FarmBsModels;
using CapstoneProject_SP25_IPAS_BussinessObject.BusinessModel.OrderModels;
using CapstoneProject_SP25_IPAS_BussinessObject.BusinessModel.ReportModel;
using CapstoneProject_SP25_IPAS_BussinessObject.BusinessModel.UserBsModels;
using CapstoneProject_SP25_IPAS_BussinessObject.Entities;
using CapstoneProject_SP25_IPAS_BussinessObject.ProgramSetUpObject.Weather;
using CapstoneProject_SP25_IPAS_BussinessObject.RequestModel.ReportModel;
using CapstoneProject_SP25_IPAS_Common.Constants;
using CapstoneProject_SP25_IPAS_Common.Enum;
using CapstoneProject_SP25_IPAS_Common.Utils;
using CapstoneProject_SP25_IPAS_Repository.UnitOfWork;
using CapstoneProject_SP25_IPAS_Service.Base;
using CapstoneProject_SP25_IPAS_Service.IService;
using GenerativeAI.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Net.Mime.MediaTypeNames;

namespace CapstoneProject_SP25_IPAS_Service.Service
{
    public class ReportService : IReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public ReportService(IUnitOfWork unitOfWork, IMapper mapper, IConfiguration configuration, HttpClient httpClient)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task<BusinessResult> CropCareReport(int landPlotId, int year)
        {
            try
            {
                var totalTrees = _unitOfWork.PlanRepository.GetTotalTrees(landPlotId, year);
                var workLogs = await _unitOfWork.PlanRepository.GetWorkLogs(landPlotId, year);
                var totalTasks = workLogs.Count;
                var completedTasks = workLogs.Count(w => w.Status.ToLower() == "completed");

                var tasksByMonth = workLogs
                    .GroupBy(w => w.Date.Value.Month)
                    .Select(g => new TasksByMonthModel
                    {
                        Month = new DateTime(year, g.Key, 1).ToString("MMM"),
                        Completed = g.Count(w => w.Status.ToLower() == "completed"),
                        Remained = g.Count(w => w.Status.ToLower() != "completed")
                    }).ToList();

                var treeHealthStatus = _unitOfWork.PlanRepository.GetTreeHealthStatus(landPlotId);
                var treeNotes = _unitOfWork.PlanRepository.GetTreeNotes(landPlotId);

                var result = new CropCareReportModel
                {
                    LandPlotId = landPlotId,
                    Year = year,
                    TotalTrees = totalTrees,
                    TotalTasks = totalTasks,
                    CompletedTasks = completedTasks,
                    TasksByMonth = tasksByMonth,
                    TreeHealthStatus = treeHealthStatus,
                    TreeNotes = treeNotes
                };
                if (result != null)
                {
                    return new BusinessResult(Const.SUCCESS_GET_CROP_CARE_REPORT_CODE, Const.SUCCESS_GET_CROP_CARE_REPORT_MSG, result);
                }
                return new BusinessResult(Const.FAIL_GET_CROP_CARE_REPORT_CODE, Const.FAIL_GET_CROP_CARE_REPORT_MSG);
            }
            catch (Exception ex)
            {

                return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
            }
        }

        public async Task<BusinessResult> Dashboard(int? year, int? month, int? farmId)
        {
            try
            {
                if (year == 0 || year == null) year = DateTime.Now.Year;
                if (month == 0 || month == null) month = DateTime.Now.Month;
                var totalPlant = await _unitOfWork.PlantRepository.getPlantInclude();
                var totalEmployee = await _unitOfWork.UserRepository.GetAllEmployeeByFarmId(farmId);
                var toltalTask = await _unitOfWork.WorkLogRepository.GetWorkLogInclude();
                var growthStagePercentage = totalPlant
                                               .Where(p => p.FarmId == farmId && p.GrowthStage != null) // Bỏ cây không có GrowthStage
                                               .GroupBy(p => p.GrowthStage.GrowthStageName)
                                               .ToDictionary(
                                                   g => g.Key!,
                                                   g => Math.Round(((double)g.Count() / totalPlant.Where(p => p.FarmId == farmId).Count()) * 100, 2) // Làm tròn 2 số thập phân
                                               );
                var plantHeathStatus = totalPlant
                                               .Where(p => p.FarmId == farmId && !string.IsNullOrEmpty(p.HealthStatus)) // Bỏ cây không có Status
                                               .GroupBy(p => p.HealthStatus)
                                               .ToDictionary(
                                                   g => g.Key!,
                                                   g => g.Count() // Làm tròn 2 số thập phân
                                               );

                var filteredTask = toltalTask
                                 .Where(p => !string.IsNullOrEmpty(p.Status)) // Bỏ task không có status
                                 .Where(p => p.Schedule != null && p.Schedule.CarePlan != null) // Lọc trước khi gọi thuộc tính bên trong
                                 .Where(p =>
                                     p.Schedule.CarePlan.PlanTargets.Any(pt =>
                                         (pt.LandPlot != null && pt.LandPlot.FarmId == farmId) ||
                                         (pt.Plant != null && pt.LandRow != null && pt.LandRow.FarmId == farmId)
                                     )
                                 )
                                 .ToList();
                var totalFilteredTask = filteredTask.Count(); // Đếm số Task phù hợp

                var listTaskStatusDistribution = filteredTask
                    .GroupBy(x => x.Status)
                    .ToDictionary(
                        g => g.Key!,
                        g => Math.Round(((double)g.Count() / totalFilteredTask) * 100, 2)
                    );
                var taskStatusDistribution = new TaskStatusDistribution()
                {
                    TotalTask = toltalTask.Count(),
                    TaskStatus = listTaskStatusDistribution
                };

                var getAllWorkLog = await _unitOfWork.WorkLogRepository.GetListWorkLogByFarmId(farmId.Value);
                var getStatusDone = await _unitOfWork.SystemConfigRepository
                                        .GetConfigValue(SystemConfigConst.DONE.Trim(), "Done");
                var totalCount = getAllWorkLog.Count();
                var doneCount = getAllWorkLog.Count(x => x.Status == getStatusDone);
                var percentComplete = totalCount == 0 ? 0 : Math.Round((double)doneCount / totalCount * 100, 2);
                var getFarm = await _unitOfWork.FarmRepository.GetFarmById(farmId.Value);
                string url = $"https://api.openweathermap.org/data/2.5/weather?lat={getFarm.Latitude}&lon={getFarm.Longitude}&appid={_configuration["SystemDefault:API_KEY_WEATHER"]}&units=metric";

                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                JObject weatherData = JObject.Parse(responseBody);
                var weatherProperty = new WeatherPropertyModel
                {
                    CurrentTemp = weatherData["main"]["temp"].Value<double>(),
                    TempMax = weatherData["main"]["temp_max"].Value<double>(),
                    TempMin = weatherData["main"]["temp_min"].Value<double>(),
                    Status = weatherData["weather"][0]["main"].Value<string>(),
                    Description = weatherData["weather"][0]["description"].Value<string>(),
                    Humidity = weatherData["main"]["humidity"].Value<double>(),
                    Visibility = weatherData["visibility"].Value<int>(),
                    Clouds = weatherData["clouds"]["all"].Value<double>(),
                    WindSpeed = weatherData["wind"]["speed"].Value<double>() + " m/s",
                };

                var dashboardModel = new DashboardModel()
                {
                    TotalPlant = totalPlant.Where(p => p.FarmId == farmId).ToList().Count(),
                    TotalEmployee = totalEmployee,
                    TotalTask = totalFilteredTask,
                    TaskComplete = percentComplete,
                    PlantDevelopmentDistribution = growthStagePercentage,
                    PlantDevelopmentStages = growthStagePercentage,
                    PlantHealthStatus = plantHeathStatus,
                    TaskStatusDistribution = taskStatusDistribution,
                    WeatherPropertyModel = weatherProperty,
                    MaterialsInStoreModels = await GetDataForMaterialsInStore(year.Value, farmId),
                    PomeloQualityBreakDowns = await GetDataForPomeloQualityBreakDown(year.Value, farmId),
                    ProductivityByPlots = await GetDataForProductivityByPlot(year.Value, farmId),
                    SeasonalYields = await GetDataForSeasonYield(year.Value, farmId),
                    WorkProgressOverviews = await GetDataForWorkprogressOverview(year.Value, month.Value, farmId),

                };

                if (dashboardModel != null)
                {
                    return new BusinessResult(Const.SUCCESS_GET_DASHBOARD_REPORT_CODE, Const.SUCCESS_GET_DASHBOARD_REPORT_MSG, dashboardModel);
                }
                return new BusinessResult(Const.FAIL_GET_DASHBOARD_REPORT_CODE, Const.FAIL_GET_DASHBOARD_REPORT_MSG, null);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
            }
        }

        private async Task<List<MaterialsInStoreModel>> GetDataForMaterialsInStore(int year, int? farmId)
        {
            if (year == 0 || year == null) year = DateTime.Now.Year;
            var getListHarvestHistoryTemp = await _unitOfWork.HarvestHistoryRepository.GetHarvestHistoryInclude(farmId);

            var getListHarvestHistory = getListHarvestHistoryTemp.Where(x => x.DateHarvest.HasValue && x.DateHarvest.Value.Year == year)
                                    .GroupBy(p => p.DateHarvest.Value.ToString("MM-yyyy")) // Nhóm theo mùa
                                    .Select(g => new MaterialsInStoreModel
                                    {
                                        Month = g.Key,  // Key là mùa
                                        Materials = g.SelectMany(h => h.ProductHarvestHistories)
                                        .GroupBy(ht => new { ht.MasterTypeId })
                                        .Select(x => new Materials
                                        {
                                            ProductType = x.FirstOrDefault().Product.MasterTypeName,
                                            UnitOfMaterials = new UnitOfMaterials()
                                            {
                                                Unit = x.FirstOrDefault().Unit,
                                                Value = x.FirstOrDefault().ActualQuantity
                                            }
                                        }).ToList()
                                        /*
                                           ,
                                                                                Count = g.SelectMany(h => h.ProductHarvestHistories)
                                                                                         .Where(x => x.Plant != null
                                                                                                  && x.Plant.LandRow != null
                                                                                                  && x.Plant.LandRow.LandPlot != null
                                                                                                  && x.Plant.LandRow.LandPlot.Farm != null
                                                                                                  && x.Plant.LandRow.LandPlot.Farm.FarmId == farmId)
                                                                                         .Sum(ht => ht.ActualQuantity),
                                                                                TypeOfProduct = g.SelectMany(h => h.ProductHarvestHistories)
                                                                                .Where(x => x.Plant != null
                                                                                         && x.Plant.LandRow != null
                                                                                         && x.Plant.LandRow.LandPlot != null
                                                                                         && x.Plant.LandRow.LandPlot.Farm != null
                                                                                         && x.Plant.LandRow.LandPlot.Farm.FarmId == farmId)
                                                                                .GroupBy(ht => new { ht.Plant.PlantName, ht.Product.MasterTypeName }) // Nhóm theo tên cây và loại cây
                                                                                .Select(plantGroup => new TypeOfProduct
                                                                                {
                                                                                    PlantName = plantGroup.Key.PlantName, // Tên cây
                                                                                    MasterTypeName = plantGroup.Key.MasterTypeName, // Loại cây
                                                                                    TotalQuantity = plantGroup.Sum(p => p.ActualQuantity) // Tổng số lượng
                                                                                })
                                                                                .ToList()
                                         */
                                    }).ToList();
            return getListHarvestHistory;
        }

        private async Task<List<PomeloQualityBreakDown>> GetDataForPomeloQualityBreakDown(int? year, int? farmId)
        {
            if (year == 0 || year == null) year = DateTime.Now.Year;
            var rawData = await _unitOfWork.ProductHarvestHistoryRepository.GetHarvestDataByYear(year.Value, farmId);

            // Nhóm dữ liệu theo mùa vụ
            var groupedData = rawData
                .GroupBy(ht => new { ht.HarvestHistory.Crop.HarvestSeason, ht.Product.MasterTypeName })
                .Select(g => new
                {
                    HarvestSeason = g.Key.HarvestSeason ?? "Không xác định",
                    QualityType = g.Key.MasterTypeName ?? "Không xác định",
                    Quantity = g.Sum(ht => ht.ActualQuantity ?? 0)
                })
                .ToList();

            // Tính tổng sản lượng theo từng mùa
            var totalBySeason = groupedData
                .GroupBy(g => g.HarvestSeason)
                .ToDictionary(g => g.Key, g => g.Sum(q => q.Quantity));

            // Xây dựng danh sách kết quả
            var result = groupedData
                .GroupBy(g => g.HarvestSeason)
                .Select(g => new PomeloQualityBreakDown
                {
                    HarvestSeason = g.Key,
                    QualityStats = g.Select(q => new QualityStat
                    {
                        QualityType = q.QualityType,
                        Percentage = Math.Round(totalBySeason[g.Key] == 0 ? 0 : ((double)q.Quantity / totalBySeason[g.Key]) * 100, 2)
                    }).ToList()
                })
                .OrderBy(s => s.HarvestSeason)
                .ToList();
            return result;
        }

        private async Task<List<SeasonalYieldModel>> GetDataForSeasonYield(int? year, int? farmId)
        {
            var rawData = await _unitOfWork.ProductHarvestHistoryRepository.GetHarvestDataByYear(year.Value, farmId);

            // Nhóm dữ liệu theo mùa vụ và loại sản phẩm


            // Nhóm dữ liệu theo mùa vụ
            var result = rawData
                      .GroupBy(x => x.HarvestHistoryId)
                      .Select(g => new SeasonalYieldModel
                      {
                          HarvestSeason = g.FirstOrDefault()?.HarvestHistory?.Crop?.CropName ?? "",

                          QualityStats = g
                              .GroupBy(ht => ht.MasterTypeId)
                              .Select(q => new QualityYieldStat
                              {
                                  QualityType = q.FirstOrDefault()?.Product?.MasterTypeName ?? "",
                                  QuantityYield = q.Sum(x => x.ActualQuantity ?? 0)
                              }).ToList()
                      })
                      .OrderBy(s => s.HarvestSeason)
                      .ToList();
            return result;
        }

        private async Task<List<WorkProgressOverview>> GetDataForWorkprogressOverview(int? year, int? month, int? farmId)
        {
            var getListWorkLogByYearAndMonth = await _unitOfWork.WorkLogRepository.GetListWorkLogByYearAndMonth(year.Value, month.Value, farmId);
            var result = getListWorkLogByYearAndMonth.Select(wl => new WorkProgressOverview()
            {
                TaskId = wl.WorkLogId,
                TaskName = wl.WorkLogName,
                Status = wl.Status,
                DueDate = wl.Date,
                listEmployee = wl.UserWorkLogs.Select(uwl => new EmployeeWorkProgressModel
                {
                    UserId = uwl.UserId,
                    FullName = uwl.User.FullName,
                    IsReporter = uwl.IsReporter,
                    AvatarURL = uwl.User.AvatarURL
                })
                .Take(5)
                .ToList()
            })
            .OrderBy(x => x.DueDate)
            .ToList();
            return result;
        }

        private async Task<List<ProductivityByPlotModel>> GetDataForProductivityByPlot(int? year, int? farmId)
        {

            var getListLandPlot = await _unitOfWork.LandPlotRepository.GetLandPlotInclude();

            var result = getListLandPlot
                                    .Where(lp => lp.Farm.FarmId == farmId && lp.LandPlotCrops.Any(x => x.Crop.StartDate.Value.Year == year))
                                    .SelectMany(lp => lp.LandPlotCrops, (lp, lpc) => new
                                    {
                                        //Year = lpc.Crop.Year ?? 0,
                                        Year = lpc.Crop.StartDate.Value.Year,
                                        HarvestSeason = lpc.Crop.CropName ?? "Không xác định",
                                        LandPlotId = lp.LandPlotId,
                                        LandPlotName = lp.LandPlotName,
                                        Status = lp.Status,
                                        Quantity = lpc.Crop.HarvestHistories
                                                    .SelectMany(hh => hh.ProductHarvestHistories)
                                                    .Where(phh => phh.Plant != null && phh.Plant.LandRow.LandPlotId == lp.LandPlotId)
                                                    .Sum(hth => hth.ActualQuantity ?? 0)
                                    })
                                     .GroupBy(x => new { x.Year, x.HarvestSeason })
                                    .Select(group => new ProductivityByPlotModel
                                    {
                                        Year = group.Key.Year,
                                        HarvestSeason = group.Key.HarvestSeason,
                                        LandPlots = group.GroupBy(lp => lp.LandPlotId)
                                                        .Select(g =>
                                                        {
                                                            var landPlot = getListLandPlot.FirstOrDefault(p => p.LandPlotId == g.Key);

                                                            var allPlants = landPlot?.LandRows.SelectMany(r => r.Plants).ToList() ?? new List<Plant>();

                                                            // Lấy toàn bộ PlantId đã có trong ProductHarvestHistory qua các Crop của LandPlot
                                                            var harvestedPlantIds = landPlot?.LandPlotCrops
                                                                .SelectMany(crop => crop.Crop.HarvestHistories)
                                                                .SelectMany(hh => hh.ProductHarvestHistories)
                                                                .Where(phh => phh.PlantId != null)
                                                                .Select(phh => phh.PlantId.Value)
                                                                .Distinct()
                                                                .ToHashSet() ?? new HashSet<int>();

                                                            var harvestedCount = allPlants.Count(p => harvestedPlantIds.Contains(p.PlantId));
                                                            var notHarvestedCount = allPlants.Count - harvestedCount;

                                                            return new LandPlotResult
                                                            {
                                                                LandPlotId = g.Key,
                                                                LandPlotName = g.First().LandPlotName,
                                                                Status = g.First().Status,
                                                                TotalPlantOfLandPlot = allPlants.Count,
                                                                HarvestedPlantCount = harvestedCount,
                                                                NotHarvestedPlantCount = notHarvestedCount,
                                                                Quantity = g.Sum(lp => lp.Quantity)
                                                            };
                                                        })
                                                        .ToList()
                                    })
                                    .OrderByDescending(x => x.Year).ToList();
            return result;
        }

        public async Task<BusinessResult> MaterialsInStore(int year, int? farmId)
        {
            try
            {
                var result = await GetDataForMaterialsInStore(year, farmId);
                if (result != null)
                {
                    if (result.Count > 0)
                    {
                        return new BusinessResult(Const.SUCCESS_GET_MATERIALS_IN_STORE_REPORT_CODE, Const.SUCCESS_GET_MATERIALS_IN_STORE_REPORT_MSG, result);
                    }
                    return new BusinessResult(Const.WARNING_GET_MATERIALS_IN_STORE_REPORT_CODE, Const.WARNING_GET_MATERIALS_IN_STORE_REPORT_MSG, new List<MaterialsInStoreModel>());

                }
                return new BusinessResult(Const.FAIL_GET_MATERIALS_IN_STORE_REPORT_REPORT_CODE, Const.FAIL_GET_MATERIALS_IN_STORE_REPORT_MSG);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
            }
        }

        public async Task<BusinessResult> PomeloQualityBreakDown(int year, int? farmId)
        {
            try
            {
                var result = await GetDataForPomeloQualityBreakDown(year, farmId);
                if (result != null)
                {
                    if (result.Count > 0)
                    {
                        return new BusinessResult(Const.SUCCESS_GET_POMELO_QUALITY_BREAK_DOWN_REPORT_CODE, Const.SUCCESS_GET_POMELO_QUALITY_BREAK_DOWN_REPORT_MSG, result);
                    }
                    return new BusinessResult(Const.WARNING_GET_POMELO_QUALITY_BREAK_DOWN_REPORT_CODE, Const.SUCCESS_GET_POMELO_QUALITY_BREAK_DOWN_REPORT_MSG);
                }
                return new BusinessResult(Const.FAIL_GET_POMELO_QUALITY_BREAK_DOWN_REPORT_CODE, Const.FAIL_GET_POMELO_QUALITY_BREAK_DOWN_REPORT_MSG);

            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);

            }
        }

        public async Task<BusinessResult> ProductivityByPlot(int year, int? farmId)
        {
            try
            {
                var result = await GetDataForProductivityByPlot(year, farmId);

                if (result != null)
                {
                    if (result.Count > 0)
                    {
                        return new BusinessResult(Const.SUCCESS_GET_PRODUCTIVITY_BY_PLOT_REPORT_CODE, Const.SUCCESS_GET_PRODUCTIVITY_BY_PLOT_REPORT_MSG, result);
                    }
                    return new BusinessResult(Const.WARNING_GET_PRODUCTIVITY_BY_PLOT_REPORT_CODE, Const.WARNING_GET_PRODUCTIVITY_BY_PLOT_REPORT_MSG);

                }
                return new BusinessResult(Const.FAIL_GET_PRODUCTIVITY_BY_PLOT_REPORT_CODE, Const.FAIL_GET_PRODUCTIVITY_BY_PLOT_REPORT_MSG);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
            }
        }

        public async Task<BusinessResult> SeasonYield(int year, int? farmId)
        {
            try
            {

                var result = await GetDataForSeasonYield(year, farmId);

                if (result != null)
                {
                    if (result.Count > 0)
                    {
                        return new BusinessResult(Const.SUCCESS_GET_SEASON_YIELD_REPORT_CODE, Const.SUCCESS_GET_SEASON_YIELD_REPORT_MSG, result);
                    }
                    return new BusinessResult(Const.WARNING_GET_SEASON_YIELD_REPORT_CODE, Const.WARNING_GET_SEASON_YIELD_REPORT_MSG);
                }
                return new BusinessResult(Const.FAIL_GET_SEASON_YIELD_REPORT_CODE, Const.FAIL_GET_SEASON_YIELD_REPORT_MSG);
            }
            catch (Exception ex)
            {

                return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
            }
        }

        public async Task<BusinessResult> WorkProgressOverview(int year, int month, int? farmId)
        {
            try
            {
                var result = await GetDataForWorkprogressOverview(year, month, farmId);
                if (result != null)
                {
                    if (result.Count > 0)
                    {
                        return new BusinessResult(Const.SUCCESS_GET_WORK_PROGRESS_OVERVIEW_REPORT_CODE, Const.SUCCESS_GET_WORK_PROGRESS_OVERVIEW_REPORT_MSG, result);
                    }
                    return new BusinessResult(Const.WARNING_GET_WORK_PROGRESS_OVERVIEW_REPORT_CODE, Const.WARNING_GET_WORK_PROGRESS_OVERVIEW_REPORT_MSG);
                }
                return new BusinessResult(Const.FAIL_GET_WORK_PROGRESS_OVERVIEW_REPORT_CODE, Const.FAIL_WORK_PROGRESS_OVERVIEW_REPORT_MSG);

            }
            catch (Exception ex)
            {

                return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
            }
        }

        private string GetSeasonFromDate(DateTime date)
        {
            int month = date.Month;
            return month switch
            {
                1 or 2 or 3 => "Spring " + date.Year.ToString(),
                4 or 5 or 6 => "Summer " + date.Year.ToString(),
                7 or 8 or 9 => "Fall " + date.Year.ToString(),
                _ => "Winter"
            };
        }

        public async Task<BusinessResult> GetWeatherOfFarm(int farmId)
        {
            try
            {
                var getFarm = await _unitOfWork.FarmRepository.GetFarmById(farmId);
                string url = $"https://api.openweathermap.org/data/2.5/weather?lat={getFarm.Latitude}&lon={getFarm.Longitude}&appid={_configuration["SystemDefault:API_KEY_WEATHER"]}&units=metric";

                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                JObject weatherData = JObject.Parse(responseBody);

                return new BusinessResult(200, "Get Weather Of Farm Success", weatherData);
            }
            catch (Exception ex)
            {

                return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
            }
        }

        public async Task<BusinessResult> StatisticEmployee(int farmID)
        {
            try
            {
                var toltalTask = await _unitOfWork.WorkLogRepository.GetWorkLogInclude();
                var filteredTask = toltalTask
                               .Where(p => !string.IsNullOrEmpty(p.Status)) // Bỏ task không có status
                               .Where(p => p.Schedule != null && p.Schedule.CarePlan != null)
                               .Where(x => x.Schedule.FarmID == farmID || x.Schedule.CarePlan.FarmID == farmID)// Lọc trước khi gọi thuộc tính bên trong
                               .ToList();
                var totalFilteredTask = filteredTask.Count(); // Đếm số Task phù hợp

                var listTaskStatusDistribution = filteredTask
                    .GroupBy(x => x.Status)
                    .ToDictionary(
                        g => g.Key!,
                        g => Math.Round(((double)g.Count() / totalFilteredTask) * 100, 2)
                    );
                var taskStatusDistribution = new TaskStatusDistribution()
                {
                    TotalTask = toltalTask.Count(),
                    TaskStatus = listTaskStatusDistribution
                };
                if (taskStatusDistribution != null)
                {
                    return new BusinessResult(200, "Statistic Employee Success", taskStatusDistribution);
                }
                return new BusinessResult(400, "Statistic Employee Failed");
            }
            catch (Exception ex)
            {

                return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
            }
        }

        public async Task<BusinessResult> StatisticPlan(int? month, int? year, int? farmID)
        {
            try
            {
                var query = _unitOfWork.PlanRepository.GetAllPlans();

                if (farmID.HasValue)
                {
                    query = query.Where(p => p.FarmID == farmID.Value);
                }

                if (year.HasValue)
                {
                    query = query.Where(p => p.StartDate.HasValue && p.StartDate.Value.Year == year.Value);

                    if (month.HasValue)
                    {
                        query = query.Where(p => p.StartDate.Value.Month == month.Value);
                    }
                }
                var plans = await query
                    .ToListAsync();

                var plansByMonth = plans
                    .GroupBy(p => p.StartDate!.Value.Month)
                    .Select(g => new MonthlyPlanStatsDto
                    {
                        Month = g.Key,
                        TotalPlans = g.Count()
                    })
                    .ToList();

                var statusDistribution = plans
                    .GroupBy(p => p.IsActive == true ? "Active" : "InActive")
                    .ToDictionary(g => g.Key, g => g.Count());

                var planByType = plans
                                 .Where(p => p.MasterTypeId.HasValue && p.IsSample == false)
                                 .GroupBy(p => p.MasterTypeId!.Value)
                                 .Select(g => new
                                 {
                                     TypeName = g.First().MasterType?.Target,
                                     Count = g.Count()
                                 })
                                 .GroupBy(x => x.TypeName ?? "Others")
                                 .ToDictionary(g => g.Key, g => g.Sum(x => x.Count));

                var groupedStatuses = plans
                                        .Where(p => !string.IsNullOrEmpty(p.Status))
                                        .GroupBy(p => p.Status!)
                                        .ToDictionary(g => g.Key, g => g.Count());

                var statusSummary = new PlanStatusSummaryDto
                {
                    Total = groupedStatuses.Values.Sum(),
                    Status = groupedStatuses
                };

                var result = new PlanStatisticsDto
                {
                    PlansByMonth = plansByMonth,
                    StatusDistribution = statusDistribution,
                    PlanByWorkType = planByType,
                    StatusSummary = statusSummary
                };
                if (result != null)
                {
                    return new BusinessResult(200, "Statistic plan success", result);
                }
                return new BusinessResult(400, "Statistic plan failed");
            }
            catch (Exception ex)
            {

                return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
            }
        }

        public async Task<BusinessResult> GetWorkPerformanceAsync(WorkPerformanceRequestDto request, int? farmId)
        {
            try
            {
                var userWorkLogs = await _unitOfWork.UserWorkLogRepository
                                .GetUserWorkLogsByEmployeeIds(request.Limit, farmId, request.Search);



                var grouped = userWorkLogs
                                 .Where(uw => uw != null && uw.User != null)
                                 .GroupBy(uw => uw.User.UserId)
                                 .Select(g =>
                                 {
                                     var user = g.First().User;
                                     var workLogs = user.UserWorkLogs?.Where(x => x.IsDeleted != true).ToList() ?? new List<UserWorkLog>();

                                     var totalTasks = workLogs.Count;
                                     var taskSuccess = workLogs.Count(x => x.WorkLog?.Status == "Done");
                                     var taskFail = workLogs.Count(x =>
                                         x.WorkLog?.Status == "Failed" || x.StatusOfUserWorkLog == "Redo");

                                     var score = taskSuccess;

                                     return new WorkPerformanceResponseDto
                                     {
                                         EmployeeId = user.UserId,
                                         Name = user.FullName ?? "Không rõ",
                                         TaskSuccess = taskSuccess,
                                         TaskFail = taskFail,
                                         TotalTask = totalTasks,
                                         Avatar = user.AvatarURL ?? "Không rõ",
                                         Score = score
                                     };
                                 })
                                 .OrderByDescending(x => x.Score)
                                 .ToList();
                // ✅ Lọc theo khoảng điểm
                if (request.MinScore.HasValue)
                    grouped = grouped.Where(x => x.Score >= request.MinScore.Value).ToList();

                if (request.MaxScore.HasValue)
                    grouped = grouped.Where(x => x.Score <= request.MaxScore.Value).ToList();

                // ✅ Sắp xếp theo loại: top hay bottom
                if (!string.IsNullOrEmpty(request.Type))
                {
                    if (request.Type.ToLower() == "top")
                    {
                        grouped = grouped.OrderByDescending(x => x.Score).ToList();
                    }
                    else if (request.Type.ToLower() == "bottom")
                    {
                        grouped = grouped.OrderBy(x => x.Score).ToList();
                    }
                }
                if (grouped.Any())
                {
                    return new BusinessResult(200, "Get Work PerFormance success", grouped);
                }
                return new BusinessResult(400, "Get Work Performance failed");
            }
            catch (Exception ex)
            {

                return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
            }
        }

        public async Task<BusinessResult> GetWorkPerformanceCompareAsync(WorkPerFormanceCompareDto request, int? farmId)
        {
            try
            {
                var query = await _unitOfWork.UserWorkLogRepository
                                .GetUserWorkLogsByEmployeeIds(null, farmId, null);

                if (request.ListEmployee != null)
                {
                    query = query.Where(x => request.ListEmployee.Contains(x.UserId)).ToList();
                }


                var getStatusDone = await _unitOfWork.SystemConfigRepository
                                   .GetConfigValue(SystemConfigConst.DONE.Trim(), "Done");
                var getStatusRedo = await _unitOfWork.SystemConfigRepository
                                   .GetConfigValue(SystemConfigConst.REDO.Trim(), "Redo");
                var getStatusFailed = await _unitOfWork.SystemConfigRepository
                                   .GetConfigValue(SystemConfigConst.FAILED.Trim(), "Failed");
                var userWorkLogs = query;

                var grouped = userWorkLogs
                                .Where(uw => uw != null && uw.User != null)
                                .GroupBy(uw => uw.User.UserId)
                                .Select(g =>
                                {
                                    var user = g.First().User;
                                    var workLogs = user.UserWorkLogs?.Where(x => x.IsDeleted != true).ToList() ?? new List<UserWorkLog>();

                                    var totalTasks = workLogs.Count;
                                    var taskSuccess = workLogs.Count(x => x.WorkLog?.Status == getStatusDone);
                                    var taskFail = workLogs.Count(x =>
                                        x.WorkLog?.Status == getStatusFailed || x.StatusOfUserWorkLog == getStatusRedo);

                                    var score = totalTasks > 0 ? Math.Round((double)taskSuccess / totalTasks * 10, 2) : 0;

                                    return new WorkPerformanceResponseDto
                                    {
                                        EmployeeId = user.UserId,
                                        Name = user.FullName ?? "N/A",
                                        TaskSuccess = taskSuccess,
                                        TaskFail = taskFail,
                                        TotalTask = totalTasks,
                                        Avatar = user.AvatarURL ?? "N/A",
                                        Score = score
                                    };
                                })
                                .OrderByDescending(x => x.Score)
                                .ToList();
                if (grouped.Any())
                {
                    return new BusinessResult(200, "Get Work PerFormance success", grouped);
                }
                return new BusinessResult(400, "Get Work Performance failed");
            }
            catch (Exception ex)
            {

                return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
            }
        }

        public async Task<BusinessResult> AdminDashBoard(GetAdminDashBoardRequest request)
        {
            try
            {

                if (!request.YearRevenue.HasValue)
                    request.YearRevenue = DateTime.Now.Year;
                if (!request.YearFarm.HasValue)
                    request.YearFarm = DateTime.Now.Year;
                int totalUser = await _unitOfWork.UserRepository.Count(x => x.IsDeleted == false);
                int totalFarm = await _unitOfWork.FarmRepository.Count(x => x.IsDeleted == false);
                double totalRevenue = await _unitOfWork.OrdersRepository.GetTotalRevenueAsync();
                var revenueStatictis = await GetStatisticRevenueYearAsync(request.YearRevenue);
                var farmStatictis = await GetStatisticFarmYearAsync(request.YearFarm);
                var NewestFarmsModels = await GetNewestFarmsAsync(request.TopNNewestFarm);
                var NewestOrderModels = await GetNewestOrdersAsync(request.TopNNewestOrder);
                var NewestUsersModels = await GetNewestUsersAsync(request.TopNNewestUser);

                var dashboard = new AdminDashBoardModel
                {
                    TotalUser = totalUser,
                    TotalFarm = totalFarm,
                    TotalRevenue = totalRevenue,
                    StatisticRevenueYear = revenueStatictis,
                    StatisticFarmYear = farmStatictis,
                    NewestUserModels = NewestUsersModels,
                    NewestOrdersModels = NewestOrderModels,
                    NewestFarmsModels = NewestFarmsModels,
                };
                return new BusinessResult(200, "Get data success", dashboard);
            }
            catch (Exception ex)
            {

                return new BusinessResult(Const.ERROR_EXCEPTION, Const.ERROR_MESSAGE);
            }
        }

        //private async Task<StatisticRevenueYear> GetStatisticRevenueYearAsync(int? year)
        //{
        //    //if (!year.HasValue)
        //    //    year = DateTime.Now.Year;

        //    var revenue = await _unitOfWork.OrdersRepository.GetAllNoPaging(o =>
        //    o.OrderDate.HasValue && o.OrderDate.Value.Year == year && o.TotalPrice.HasValue && o.Status!.ToLower().Equals(OrderStatusEnum.Paid.ToString().ToLower()));

        //    var revenueData = revenue
        //    .GroupBy(o => o.OrderDate.Value.Month)
        //    .Select(g => new RevenueMonth
        //    {
        //        Year = year,
        //        Month = g.Key,
        //        TotalRevenue = g.Sum(x => x.TotalPrice) ?? 0
        //    })
        //    .ToList();

        //    return new StatisticRevenueYear
        //    {
        //        TotalRevenueYear = revenueData.Sum(x => x.TotalRevenue),
        //        Year = year,
        //        revenueMonths = revenueData
        //    };
        //}

        private async Task<StatisticRevenueYear> GetStatisticRevenueYearAsync(int? year)
        {
            if (!year.HasValue)
                year = DateTime.Now.Year;

            // Lấy dữ liệu đơn hàng đã thanh toán trong năm
            var revenue = await _unitOfWork.OrdersRepository.GetAllNoPaging(o =>
                o.OrderDate.HasValue &&
                o.OrderDate.Value.Year == year &&
                o.TotalPrice.HasValue &&
                o.Status!.ToLower() == OrderStatusEnum.Paid.ToString().ToLower()
            );

            // Gom nhóm theo tháng có dữ liệu
            var revenueGrouped = revenue
                .GroupBy(o => o.OrderDate.Value.Month)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.TotalPrice ?? 0));

            // Tạo đủ 12 tháng
            var revenueData = Enumerable.Range(1, 12).Select(month => new RevenueMonth
            {
                Year = year,
                Month = month,
                TotalRevenue = revenueGrouped.ContainsKey(month) ? revenueGrouped[month] : 0
            }).ToList();

            return new StatisticRevenueYear
            {
                TotalRevenueYear = revenueData.Sum(x => x.TotalRevenue),
                Year = year,
                revenueMonths = revenueData
            };
        }

        //private async Task<StatisticFarmYear> GetStatisticFarmYearAsync(int? year)
        //{

        //    var farms = await _unitOfWork.FarmRepository.GetAllNoPaging(f => f.CreateDate.HasValue && f.CreateDate.Value.Year == year && f.IsDeleted == false);
        //    var farmData = farms
        //        .GroupBy(f => f.CreateDate.Value.Month)
        //        .Select(g => new RevenueMonth
        //        {
        //            Year = year,
        //            Month = g.Key,
        //            TotalRevenue = g.Count()
        //        })
        //        .ToList();

        //    return new StatisticFarmYear
        //    {
        //        TotalRevenueYear = farmData.Sum(x => x.TotalRevenue),
        //        Year = year,
        //        revenueMonths = farmData
        //    };
        //}

        private async Task<StatisticFarmYear> GetStatisticFarmYearAsync(int? year)
        {
            if (!year.HasValue)
                year = DateTime.Now.Year;

            var farms = await _unitOfWork.FarmRepository.GetAllNoPaging(f =>
                f.CreateDate.HasValue &&
                f.CreateDate.Value.Year == year &&
                f.IsDeleted == false
            );

            var farmGrouped = farms
                .GroupBy(f => f.CreateDate.Value.Month)
                .ToDictionary(g => g.Key, g => g.Count());

            var farmData = Enumerable.Range(1, 12).Select(month => new RevenueMonth
            {
                Year = year,
                Month = month,
                TotalRevenue = farmGrouped.ContainsKey(month) ? farmGrouped[month] : 0
            }).ToList();

            return new StatisticFarmYear
            {
                TotalRevenueYear = farmData.Sum(x => x.TotalRevenue),
                Year = year,
                revenueMonths = farmData
            };
        }

        private async Task<List<UserModel>> GetNewestUsersAsync(int? TopN)
        {
            if (!TopN.HasValue)
                TopN = 10;
            Expression<Func<User, bool>> filter = x => x.IsDeleted == false! && !x.Role!.RoleName!.ToLower().Equals(RoleEnum.ADMIN.ToString().ToLower());
            Func<IQueryable<User>, IOrderedQueryable<User>> orderBy = x => x.OrderByDescending(o => o.CreateDate).ThenByDescending(x => x.UserId);
            string includeProperties = "Role";
            var entities = await _unitOfWork.UserRepository.Get(filter, orderBy, includeProperties, pageIndex: 1, pageSize: TopN);
            var mappedResult = _mapper.Map<List<UserModel>>(entities).ToList();
            return mappedResult;
        }

        private async Task<List<FarmModel>> GetNewestFarmsAsync(int? TopN)
        {
            if (!TopN.HasValue)
                TopN = 10;
            Expression<Func<Farm, bool>> filter = x => x.IsDeleted == false;
            Func<IQueryable<Farm>, IOrderedQueryable<Farm>> orderBy = x => x.OrderByDescending(o => o.CreateDate).ThenByDescending(o => o.FarmId);
            var entities = await _unitOfWork.FarmRepository.Get(filter, orderBy, pageIndex: 1!, pageSize: TopN);
            var mappedResult = _mapper.Map<IEnumerable<FarmModel>>(entities).ToList();
            return mappedResult;
        }

        private async Task<List<OrderModel>> GetNewestOrdersAsync(int? TopN)
        {
            if (!TopN.HasValue)
                TopN = 10;
            Expression<Func<Order, bool>> filter = null!;
            Func<IQueryable<Order>, IOrderedQueryable<Order>> orderBy = x => x.OrderByDescending(o => o.OrderDate).ThenByDescending(o => o.OrderId);
            string includeProperties = "Package,Farm,Payment";
            var entities = await _unitOfWork.OrdersRepository.Get(filter, orderBy, includeProperties, pageIndex: 1!, pageSize: TopN);
            var mappedResult = _mapper.Map<IEnumerable<OrderModel>>(entities).ToList();
            return mappedResult;
        }

        public async Task<BusinessResult> EmployeeTodayTask(int userId)
        {
            try
            {
                var getEmployeeTodayTask = await _unitOfWork.UserWorkLogRepository.GetEmployeeToDayTask(userId);
                if (getEmployeeTodayTask.Any())
                {
                    var result = _mapper.Map<List<EmployeeTodayTask>>(getEmployeeTodayTask);

                    for (int i = 0; i < result.Count; i++)
                    {
                        var actualStart = getEmployeeTodayTask[i].WorkLog?.ActualStartTime;
                        var actualEnd = getEmployeeTodayTask[i].WorkLog?.ActualEndTime;

                        result[i].Time = $"{actualStart} - {actualEnd}";
                    }

                    return new BusinessResult(200, "Get employee today task success", result);
                }
                return new BusinessResult(200, "Do not have any task today", new List<EmployeeTodayTask>());
            }
            catch (Exception ex)
            {

                return new BusinessResult(Const.ERROR_EXCEPTION, Const.ERROR_MESSAGE);
            }
        }

        public async Task<BusinessResult> EmployeeProductivity(int userId, string? timeRange)
        {
            try
            {
                var now = DateTime.Now.Date;
                DateTime from, to;

                if (timeRange == "month")
                {
                    from = new DateTime(now.Year, now.Month, 1);
                    to = from.AddMonths(1).AddDays(-1); // đến cuối tháng
                }
                else // "week"
                {
                    // Lấy thứ Hai của tuần hiện tại
                    int diff = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
                    from = now.AddDays(-1 * diff);
                    to = from.AddDays(6); // Chủ nhật tuần đó
                }
                var getStatusDone = await _unitOfWork.SystemConfigRepository
                                       .GetConfigValue(SystemConfigConst.DONE.Trim(), "Done");
                var completed = await _unitOfWork.UserWorkLogRepository.GetTasksCompletedAsync(userId, getStatusDone, from, to);
                var hours = await _unitOfWork.UserWorkLogRepository.GetHoursWorkedAsync(userId, from, to);
                var skill = await _unitOfWork.UserWorkLogRepository.GetSkillScoreAsync(userId, getStatusDone, from, to);
                var aiReports = await _unitOfWork.UserWorkLogRepository.GetAiReportsSubmittedAsync(userId, from, to);
                var pendingToday = await _unitOfWork.UserWorkLogRepository.GetPendingTasksTodayAsync(userId, getStatusDone, now.Date);
                var chart = await _unitOfWork.UserWorkLogRepository.GetChartDataAsync(userId, from, to, timeRange);

                var result =  new EmployeeProductivityResponse
                {
                    TasksCompleted = completed,
                    HoursWorked = hours,
                    SkillScore = skill,
                    AiReportsSubmitted = aiReports,
                    TasksPendingToday = pendingToday,
                    ChartData = new ProductivityChart { Tasks = chart }
                };
                if(result != null)
                {
                    return new BusinessResult(200, "Get Employee Productivity Success", result);
                }
                return new BusinessResult(400, "Get Employee Productivity Failed");
            }
            catch (Exception ex)
            {

                return new BusinessResult(Const.ERROR_EXCEPTION, Const.ERROR_MESSAGE);
            }
        }

        public async Task<BusinessResult> StatisticPlantDeadAndAlive(int farmId)
        {
            try
            {
                var getAllPlantInFarm = await _unitOfWork.PlantRepository.GetAllPlantByFarmId(farmId);
                if(getAllPlantInFarm == null)
                {
                    return new BusinessResult(400, "Do not have any plant in farm", new List<Plan>());
                }
                var total = getAllPlantInFarm.Count;
                var deadCount = getAllPlantInFarm.Count(p => p.IsDead == true);
                var normalCount = total - deadCount;

                double deadPercentage = 0;
                double normalPercentage = 100;

                if (total > 0)
                {
                    deadPercentage = Math.Round((double)deadCount / total * 100, 2);
                    normalPercentage = 100 - deadPercentage;
                }
                else
                {
                    // Gán mặc định hoặc xử lý trường hợp không có dữ liệu
                    deadPercentage = 0;
                    normalPercentage = 0;
                }
                var result = new
                {
                    total = total,
                    normalPercentage = normalPercentage,
                    deadPercentage = deadPercentage
                };
                return new BusinessResult(200, "Statistic Plant Dead And Alive Success", result);

            }
            catch (Exception ex)
            {

                return new BusinessResult(Const.ERROR_EXCEPTION, Const.ERROR_MESSAGE);
            }
        }

        public async Task<BusinessResult> DashboardForEmployee(int userId, int farmId)
        {
            try
            {
                var getWorkLogOfEmployee = await _unitOfWork.WorkLogRepository.GetWorkLogByIdForDelete(userId, farmId);
                var getStatusDone = await _unitOfWork.SystemConfigRepository
                                        .GetConfigValue(SystemConfigConst.DONE.Trim(), "Done");

                // Tổng số
                int total = getWorkLogOfEmployee.Count;

                // Hoàn thành
                int done = getWorkLogOfEmployee.Count(x => x.Status == getStatusDone);

                // Chưa hoàn thành
                int notDone = total - done;

                // Upcoming: những worklog có ngày bắt đầu lớn hơn hôm nay
                var upcomingWorkLogs = getWorkLogOfEmployee
                    .Where(x => x.Date.Value.Date == DateTime.Now.Date)
                    .Select(x => new
                    {
                        WorlogId = x.WorkLogId,
                        WorkLogName = x.WorkLogName,
                        StartTime = x.ActualStartTime,
                        EndTime = x.ActualEndTime,
                        Status = x.Status
                    })
                    .ToList();

                // Kết quả trả về
                var result = new
                {
                    Total = total,
                    Done = done,
                    NotDone = notDone,
                    UpcomingList = upcomingWorkLogs
                };
                return new BusinessResult(200, "Get dashboard for employee success", result);

            }
            catch (Exception ex)
            {

                return new BusinessResult(Const.ERROR_EXCEPTION, Const.ERROR_MESSAGE);
            }
        }

        public async Task<BusinessResult> HomeMobileManager(int userId, int farmId)
        {
            try
            {
                var getFarm = await _unitOfWork.FarmRepository.GetFarmById(farmId);
                string url = "";
                if(getFarm == null)
                {
                    return new BusinessResult(400, "Do not have any data");
                }
                else
                {
                    url = $"https://api.openweathermap.org/data/2.5/weather?lat={getFarm.Latitude}&lon={getFarm.Longitude}&appid={_configuration["SystemDefault:API_KEY_WEATHER"]}&units=metric";
                }

                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                JObject weatherData = JObject.Parse(responseBody);
                var weatherProperty = new WeatherPropertyModel
                {
                    CurrentTemp = weatherData["main"]["temp"].Value<double>(),
                    TempMax = weatherData["main"]["temp_max"].Value<double>(),
                    TempMin = weatherData["main"]["temp_min"].Value<double>(),
                    Status = weatherData["weather"][0]["main"].Value<string>(),
                    Description = weatherData["weather"][0]["description"].Value<string>(),
                    Humidity = weatherData["main"]["humidity"].Value<double>(),
                    Visibility = weatherData["visibility"].Value<int>(),
                    Clouds = weatherData["clouds"]["all"].Value<double>(),
                    WindSpeed = weatherData["wind"]["speed"].Value<double>() + " m/s",
                };
                var warnings = new List<string>();
                var rules = _configuration.GetSection("WeatherConfig:WorkRules").Get<Dictionary<string, WeatherRule>>() ?? new();
                var extremeWeather = _configuration.GetSection("WeatherConfig:ExtremeWeatherConditions").Get<Dictionary<string, List<int>>>() ?? new();

                foreach (var (workType, rule) in rules)
                {
                    var conditionsViolated = new List<string>();
                    // troi qua lanh
                    if (rule.MinTemperature.HasValue && weatherProperty.CurrentTemp < rule.MinTemperature)
                        conditionsViolated.Add($"Temperature too low: {weatherProperty.CurrentTemp}°C");
                    // troi qua nong
                    if (rule.MaxTemperature.HasValue && weatherProperty.CurrentTemp > rule.MaxTemperature)
                        conditionsViolated.Add($"Temperature too high: {weatherProperty.CurrentTemp}°C");
                    // troi qua kho
                    if (rule.MinHumidity.HasValue && weatherProperty.Humidity < rule.MinHumidity)
                        conditionsViolated.Add($"Humidity too low: {weatherProperty.Humidity}%");
                    // qua am
                    if (rule.MaxHumidity.HasValue && weatherProperty.Humidity > rule.MaxHumidity)
                        conditionsViolated.Add($"Humidity too high: {weatherProperty.Humidity}%");
                    // nhieu gio
                    if (rule.MaxWindSpeed.HasValue && weatherData["wind"]["speed"].Value<double>() > rule.MaxWindSpeed)
                        conditionsViolated.Add($"Wind speed too high: {weatherData["wind"]["speed"].Value<double>()} m/s");

                    // cv nay neu ko can lam khi troi mua
                    if (rule.RainCondition == "NoRain" && weatherData["weather"][0].Contains("Rain"))
                        conditionsViolated.Add("Rain detected, avoid work.");
                    if (conditionsViolated.Any())
                        warnings.Add($"{workType} - Not recommended work because: {string.Join(", ", conditionsViolated)} ");
                }

                // Kiểm tra các hiện tượng thời tiết cực đoan
                foreach (var (condition, ids) in extremeWeather)
                {
                    if (weatherData["weather"][0].Any(w => ids.Contains(weatherData["weather"][0]["id"].Value<int>())))
                        warnings.Add($"Extreme Weather Warning: {condition}");
                }
                var getAllPlantOfFarm = await _unitOfWork.PlantRepository.GetAllPlantByFarmId(farmId);
                var total = getAllPlantOfFarm.Count;

                var statusOfPlant = new[] { "Minor Issues", "Healthy", "Serious Issues", "Dead" };

                var statusPercentage = statusOfPlant
                                      .Select(status => new
                                      {
                                          HealthStatus = status,
                                          Quantity = getAllPlantOfFarm.Count(x =>
                                              ((x.HealthStatus ?? "Unknown").ToLower()) == ((status ?? "Unknown").ToLower()))
                                      }).ToList();

                var getAllCrop = await _unitOfWork.CropRepository.GetAllCropByFarmId(farmId);
                var totalYield = getAllCrop.Sum(x => x.ActualYield);

                var workOverview = await _unitOfWork.WorkLogRepository.GetListWorkLogByFarmId(farmId);
                var allStatuses = new[] { "Not Started", "In Progress", "Done", "Overdue", "Redo", "Cancelled", "Reviewing" };
                var groupedByStatus = allStatuses
                                    .Select(status => new
                                    {
                                        Status = status,
                                        Count = workOverview.Count(w => w.Status == status)
                                    })
                                    .ToList();
                var result = new
                {
                    Warning = warnings,
                    FarmOverview = new
                    {
                        totalPlants = total,
                        totalYield = totalYield,
                        statusPercentage = statusPercentage,
                    },
                    WorkOverview = groupedByStatus
                };
                return new BusinessResult(200, "Get home mobile manager success", result);
            }
            catch (Exception ex)
            {

                return new BusinessResult(Const.ERROR_EXCEPTION, Const.ERROR_MESSAGE);
            }
        }
    }
}
