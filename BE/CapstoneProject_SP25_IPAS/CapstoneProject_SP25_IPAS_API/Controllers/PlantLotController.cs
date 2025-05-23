﻿using CapstoneProject_SP25_IPAS_API.Payload;
using CapstoneProject_SP25_IPAS_BussinessObject.Payloads.Request;
using CapstoneProject_SP25_IPAS_Common.Utils;
using CapstoneProject_SP25_IPAS_Service.IService;
using CapstoneProject_SP25_IPAS_BussinessObject.Payloads.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CapstoneProject_SP25_IPAS_API.ProgramConfig.AuthorizeConfig;
using CapstoneProject_SP25_IPAS_Common.Enum;
using CapstoneProject_SP25_IPAS_BussinessObject.RequestModel.LandPlotRequest;
using CapstoneProject_SP25_IPAS_BussinessObject.RequestModel.PlantLotRequest;
using CapstoneProject_SP25_IPAS_API.Middleware;
using CapstoneProject_SP25_IPAS_BussinessObject.RequestModel.CriteriaRequest.CriteriaTagerRequest;
using CapstoneProject_SP25_IPAS_Service.Service;

namespace CapstoneProject_SP25_IPAS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlantLotController : ControllerBase
    {
        private readonly IPlantLotService _plantLotService;
        private readonly IJwtTokenService _jwtTokenService;
        public PlantLotController(IPlantLotService plantLotService, IJwtTokenService jwtTokenService)
        {
            _plantLotService = plantLotService;
            _jwtTokenService = jwtTokenService;
        }
        //[HybridAuthorize($"{nameof(RoleEnum.ADMIN)},{nameof(RoleEnum.OWNER)},{nameof(RoleEnum.MANAGER)},{nameof(RoleEnum.EMPLOYEE)}")]
        [HttpGet(APIRoutes.PlantLot.getPlantLotWithPagination, Name = "getPlantLotWithPagination")]
        [HybridAuthorize($"{nameof(RoleEnum.ADMIN)},{nameof(RoleEnum.OWNER)},{nameof(RoleEnum.MANAGER)},{nameof(RoleEnum.EMPLOYEE)}")]
        [CheckUserFarmAccess]
        [FarmExpired]
        public async Task<IActionResult> GetAllPlantLot([FromQuery] GetPlantLotRequest filterRequest, [FromQuery] PaginationParameter paginationParameter)
        {
            try
            {
                if (!filterRequest.FarmId.HasValue)
                    filterRequest.FarmId = _jwtTokenService.GetFarmIdFromToken();
                if (!filterRequest.FarmId.HasValue)
                    return BadRequest(new BaseResponse()
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "FarmId cannot be null"
                    });
                var result = await _plantLotService.GetAllPlantLots(filterRequest, paginationParameter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                var response = new BaseResponse()
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = ex.Message
                };
                return BadRequest(response);
            }
        }
        [HttpGet(APIRoutes.PlantLot.getPlantLotById, Name = "getPlantLotById")]
        [HybridAuthorize($"{nameof(RoleEnum.ADMIN)},{nameof(RoleEnum.OWNER)},{nameof(RoleEnum.MANAGER)},{nameof(RoleEnum.EMPLOYEE)}")]
        [CheckUserFarmAccess]
        [FarmExpired]
        public async Task<IActionResult> GetPlantLotById([FromRoute] int id)
        {
            try
            {
                var result = await _plantLotService.GetPlantLotById(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                var response = new BaseResponse()
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = ex.Message
                };
                return BadRequest(response);
            }
        }
        [HttpPost(APIRoutes.PlantLot.createPlantLot, Name = "createPlantLot")]
        //[ServiceFilter(typeof(FarmExpiredFilter))]
        [HybridAuthorize($"{nameof(RoleEnum.ADMIN)},{nameof(RoleEnum.OWNER)},{nameof(RoleEnum.MANAGER)}")]
        [CheckUserFarmAccess]
        [FarmExpired]
        public async Task<IActionResult> CreatePlantLot([FromBody] CreatePlantLotModel createPlantLotModel)
        {
            try
            {
                if (!createPlantLotModel.FarmId.HasValue)
                    createPlantLotModel.FarmId = _jwtTokenService.GetFarmIdFromToken();
                if (!createPlantLotModel.FarmId.HasValue)
                    return BadRequest(new BaseResponse()
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "FarmId cannot be null"
                    });
                var result = await _plantLotService.CreatePlantLot(createPlantLotModel);
                return Ok(result);
            }
            catch (Exception ex)
            {

                var response = new BaseResponse()
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = ex.Message
                };
                return BadRequest(response);
            }
        }
        [HttpPost(APIRoutes.PlantLot.createPlantLotAdditional, Name = "createPlantLotAdditional")]
        [HybridAuthorize($"{nameof(RoleEnum.ADMIN)},{nameof(RoleEnum.OWNER)},{nameof(RoleEnum.MANAGER)}")]
        [CheckUserFarmAccess]
        [FarmExpired]
        public async Task<IActionResult> createPlantLotAdditional([FromBody] CreateAdditionalPlantLotModel createPlantLotModel)
        {
            try
            {
                var result = await _plantLotService.CreateAdditionalPlantLot(createPlantLotModel);
                return Ok(result);
            }
            catch (Exception ex)
            {

                var response = new BaseResponse()
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = ex.Message
                };
                return BadRequest(response);
            }
        }
        [HttpPut(APIRoutes.PlantLot.updatePlantLotInfo, Name = "updatePlantLotInfo")]
        [HybridAuthorize($"{nameof(RoleEnum.ADMIN)},{nameof(RoleEnum.OWNER)},{nameof(RoleEnum.MANAGER)}")]
        [CheckUserFarmAccess]
        [FarmExpired]
        public async Task<IActionResult> UpdatePlantLot([FromBody] UpdatePlantLotModel updatePlantLotModel)
        {
            try
            {
                var result = await _plantLotService.UpdatePlantLot(updatePlantLotModel);
                return Ok(result);
            }
            catch (Exception ex)
            {

                var response = new BaseResponse()
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = ex.Message
                };
                return BadRequest(response);
            }
        }

        [HttpDelete(APIRoutes.PlantLot.permanenlyDelete, Name = "permanentlyDeletePlantLot")]
        [HybridAuthorize($"{nameof(RoleEnum.ADMIN)},{nameof(RoleEnum.OWNER)},{nameof(RoleEnum.MANAGER)}")]
        [CheckUserFarmAccess]
        [FarmExpired]
        public async Task<IActionResult> DeletePlantLot([FromRoute] int id)
        {
            try
            {
                var result = await _plantLotService.DeletePlantLot(id);
                return Ok(result);
            }
            catch (Exception ex)
            {

                var response = new BaseResponse()
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = ex.Message
                };
                return BadRequest(response);
            }
        }

        //[HttpPost(APIRoutes.PlantLot.createManyPlantFromPlantLot, Name = "createmanyPlantFromPlantLot")]
        //public async Task<IActionResult> CreateManyPlant([FromBody] List<CriteriaForPlantLotRequestModel> criteriaForPlantLotRequestModels, [FromQuery] int quantity)
        //{
        //    try
        //    {
        //        var result = await _plantLotService.CreateManyPlant(criteriaForPlantLotRequestModels, quantity);
        //        return Ok(result);
        //    }
        //    catch (Exception ex)
        //    {

        //        var response = new BaseResponse()
        //        {
        //            StatusCode = StatusCodes.Status400BadRequest,
        //            Message = ex.Message
        //        };
        //        return BadRequest(response);
        //    }
        //}

        [HttpPost(APIRoutes.PlantLot.FillPlantToPlot, Name = "FillPlantToPlotAsync")]
        [HybridAuthorize($"{nameof(RoleEnum.ADMIN)},{nameof(RoleEnum.OWNER)},{nameof(RoleEnum.MANAGER)}")]
        [CheckUserFarmAccess]
        [FarmExpired]
        public async Task<IActionResult> FillPlantToPlotAsync([FromBody] FillPlanToPlotRequest fillRequest)
        {
            try
            {
                var result = await _plantLotService.FillPlantToPlot(fillRequest);
                return Ok(result);
            }
            catch (Exception ex)
            {

                var response = new BaseResponse()
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = ex.Message
                };
                return BadRequest(response);
            }
        }

        //[HybridAuthorize($"{nameof(RoleEnum.ADMIN)},{nameof(RoleEnum.OWNER)},{nameof(RoleEnum.MANAGER)},{nameof(RoleEnum.EMPLOYEE)}")]
        [HttpGet(APIRoutes.PlantLot.GetPlantPlotForSelected, Name = "GetPlantPlotForSelected")]
        [HybridAuthorize($"{nameof(RoleEnum.ADMIN)},{nameof(RoleEnum.OWNER)},{nameof(RoleEnum.MANAGER)},{nameof(RoleEnum.EMPLOYEE)}")]
        [CheckUserFarmAccess]
        //[FarmExpired]
        public async Task<IActionResult> GetPlantPlotForSelected([FromQuery] int? farmId, bool? isFromGrafted)
        {
            if (!farmId.HasValue)
                farmId = _jwtTokenService.GetFarmIdFromToken();
            if (!farmId.HasValue)
            {
                var response = new BaseResponse()
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Farm Id is required"
                };
                return BadRequest(response);
            }
            var result = await _plantLotService.GetForSelectedByFarmId(farmId.Value, isFromGrafted);
            return Ok(result);
        }

        //[HybridAuthorize($"{nameof(RoleEnum.ADMIN)},{nameof(RoleEnum.OWNER)},{nameof(RoleEnum.MANAGER)},{nameof(RoleEnum.EMPLOYEE)}")]
        [HttpGet(APIRoutes.PlantLot.GetAllPlantPlotForSelected, Name = "GetAllPlantPlotForSelected")]
        [HybridAuthorize($"{nameof(RoleEnum.ADMIN)},{nameof(RoleEnum.OWNER)},{nameof(RoleEnum.MANAGER)},{nameof(RoleEnum.EMPLOYEE)}")]
        [CheckUserFarmAccess]
        //[FarmExpired]
        public async Task<IActionResult> GetAllPlantPlotForSelected([FromQuery] int? farmId, bool? isFromGrafted)
        {
            if (!farmId.HasValue)
                farmId = _jwtTokenService.GetFarmIdFromToken();
            if (!farmId.HasValue)
            {
                var response = new BaseResponse()
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Farm Id is required"
                };
                return BadRequest(response);
            }
            var result = await _plantLotService.GetAllForSelectedByFarmId(farmId.Value, isFromGrafted);
            return Ok(result);
        }

        [HttpPatch(APIRoutes.PlantLot.SoftedDeletePlantLot, Name = "SoftedDeletePlantLot")]
        [HybridAuthorize($"{nameof(RoleEnum.ADMIN)},{nameof(RoleEnum.OWNER)},{nameof(RoleEnum.MANAGER)}")]
        [CheckUserFarmAccess]
        [FarmExpired]
        public async Task<IActionResult> SoftedDeletePlantLot([FromBody] List<int> plantIds)
        {
            try
            {
                var result = await _plantLotService.softedMultipleDelete(plantIds);
                return Ok(result);
            }
            catch (Exception ex)
            {
                var response = new BaseResponse()
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = ex.Message
                };
                return BadRequest(response);
            }
        }

        [HttpPut(APIRoutes.PlantLot.checkCriteriaForLot, Name = "checkCriteriaForLot")]
        [HybridAuthorize($"{nameof(RoleEnum.ADMIN)},{nameof(RoleEnum.OWNER)},{nameof(RoleEnum.MANAGER)}")]
        [CheckUserFarmAccess]
        [FarmExpired]
        public async Task<IActionResult> checkCriteriaForLot([FromBody] CheckPlantLotCriteriaRequest request)
        {
            if (!ModelState.IsValid)
            {
                var response = new BaseResponse()
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Data = ModelState.ValidationState.ToString()
                };
                return BadRequest(response);
            }
            var result = await _plantLotService.CheckingCriteriaForLot(request);
            return Ok(result);
        }

        [HttpPatch(APIRoutes.PlantLot.MarkUsedPlantLot, Name = "MarkUsedPlantLot")]
        [HybridAuthorize($"{nameof(RoleEnum.ADMIN)},{nameof(RoleEnum.OWNER)},{nameof(RoleEnum.MANAGER)}")]
        [CheckUserFarmAccess]
        [FarmExpired]
        public async Task<IActionResult> MarkUsedPlantLot([FromQuery] int plantLotIds)
        {
            try
            {
                var result = await _plantLotService.MarkStatusUsed(plantLotIds);
                return Ok(result);
            }
            catch (Exception ex)
            {
                var response = new BaseResponse()
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = ex.Message
                };
                return BadRequest(response);
            }
        }
    }
}
