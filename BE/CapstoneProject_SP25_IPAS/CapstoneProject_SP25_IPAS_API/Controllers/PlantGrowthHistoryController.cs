﻿using CapstoneProject_SP25_IPAS_API.Payload;
using CapstoneProject_SP25_IPAS_Service.IService;
using CapstoneProject_SP25_IPAS_BussinessObject.Payloads.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CapstoneProject_SP25_IPAS_API.ProgramConfig.AuthorizeConfig;
using CapstoneProject_SP25_IPAS_Common.Enum;
using CapstoneProject_SP25_IPAS_BussinessObject.RequestModel.PlantGrowthHistoryRequest;
using CapstoneProject_SP25_IPAS_Common.Utils;
using CapstoneProject_SP25_IPAS_Service.Base;
using CapstoneProject_SP25_IPAS_Service.Service;
using CapstoneProject_SP25_IPAS_BussinessObject.RequestModel.PlantRequest;
using CapstoneProject_SP25_IPAS_API.Middleware;

namespace CapstoneProject_SP25_IPAS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlantGrowthHistoryController : ControllerBase
    {
        private readonly IPlantGrowthHistoryService _plantGrowthHistoryService;
        private readonly IJwtTokenService _jwtTokenService;
        public PlantGrowthHistoryController(IPlantGrowthHistoryService plantGrowthHistoryService, IJwtTokenService jwtTokenService)
        {
            _plantGrowthHistoryService = plantGrowthHistoryService;
            _jwtTokenService = jwtTokenService;
        }

        [HttpPost(APIRoutes.PlantGrowthHistory.createPlantGrowthHistory, Name = "createPlantGrowthHistoryAsync")]
        //[HybridAuthorize($"{nameof(RoleEnum.ADMIN)},{nameof(RoleEnum.OWNER)},{nameof(RoleEnum.MANAGER)},{nameof(RoleEnum.EMPLOYEE)}")]
        //[CheckUserFarmAccess]
        //[FarmExpired]
        public async Task<IActionResult> CreatePlantGrowthHistoryAsync([FromForm] CreatePlantGrowthHistoryRequest request)
        {
            try
            {
                //if (!ModelState.IsValid)
                //{
                //    return BadRequest(ModelState);
                //}
                if (!request.UserId.HasValue)
                    request.UserId = _jwtTokenService.GetUserIdFromToken();
                if (!request.UserId.HasValue)
                {
                    return BadRequest(new BusinessResult
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "User Id is required"
                    });
                }
                var result = await _plantGrowthHistoryService.createPlantGrowthHistory(request);
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

        [HttpPut(APIRoutes.PlantGrowthHistory.updatePlantGrowthHistoryInfo, Name = "updatePlantGrowthHistoryAsync")]
        [HybridAuthorize($"{nameof(RoleEnum.ADMIN)},{nameof(RoleEnum.OWNER)},{nameof(RoleEnum.MANAGER)},{nameof(RoleEnum.EMPLOYEE)}")]
        [CheckUserFarmAccess]
        //[FarmExpired]
        public async Task<IActionResult> UpdatePlantGrowthHistoryAsync([FromForm] UpdatePlantGrowthHistoryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                if (!request.UserId.HasValue)
                    request.UserId = _jwtTokenService.GetUserIdFromToken();
                if (!request.UserId.HasValue)
                {
                    return BadRequest(new BusinessResult
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "User Id is required"
                    });
                }
                var result = await _plantGrowthHistoryService.updatePlantGrowthHistory(request);
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

        [HttpGet(APIRoutes.PlantGrowthHistory.getPlantGrowthHistoryById + "/{plant-growth-history-id}", Name = "getPlantGrowthByIdAsync")]
        [HybridAuthorize($"{nameof(RoleEnum.ADMIN)},{nameof(RoleEnum.OWNER)},{nameof(RoleEnum.MANAGER)},{nameof(RoleEnum.EMPLOYEE)}")]
        [CheckUserFarmAccess]
        //[FarmExpired]
        public async Task<IActionResult> GetPlantGrowthByIdAsync([FromRoute(Name = "plant-growth-history-id")] int plantGrowthHistoryId)
        {
            try
            {
                var result = await _plantGrowthHistoryService.getPlantGrowthById(plantGrowthHistoryId);
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
        [HttpGet(APIRoutes.PlantGrowthHistory.getAllHistoryOfPlantById + "/{plant-id}", Name = "getAllHistoryOfPlantByIdAsync")]
        [HybridAuthorize($"{nameof(RoleEnum.ADMIN)},{nameof(RoleEnum.OWNER)},{nameof(RoleEnum.MANAGER)},{nameof(RoleEnum.EMPLOYEE)}")]
        [CheckUserFarmAccess]
        //[FarmExpired]
        public async Task<IActionResult> GetAllHistoryOfPlantByIdAsync([FromRoute(Name = "plant-id")] int plantId)
        {
            try
            {
                var result = await _plantGrowthHistoryService.getAllHistoryOfPlantById(plantId);
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
        [HttpGet(APIRoutes.PlantGrowthHistory.getAllHistoryOfPlantPagin , Name = "getAllHistoryOfPlantPagin")]
        [HybridAuthorize($"{nameof(RoleEnum.ADMIN)},{nameof(RoleEnum.OWNER)},{nameof(RoleEnum.MANAGER)},{nameof(RoleEnum.EMPLOYEE)}")]
        [CheckUserFarmAccess]
        //[FarmExpired]
        public async Task<IActionResult> getAllHistoryOfPlantPagin([FromQuery] GetPlantGrowtRequest getRequest, PaginationParameter paginationParameter)
        {
            try
            {
                var result = await _plantGrowthHistoryService.getAllHistoryOfPlantPagin(getRequest, paginationParameter);
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

        [HttpDelete(APIRoutes.PlantGrowthHistory.deletePlantGrowthHistory + "/{plant-growth-history-id}", Name = "deleteGrowthHistoryAsync")]
        [HybridAuthorize($"{nameof(RoleEnum.ADMIN)},{nameof(RoleEnum.OWNER)},{nameof(RoleEnum.MANAGER)},{nameof(RoleEnum.EMPLOYEE)}")]
        [CheckUserFarmAccess]
        //[FarmExpired]
        public async Task<IActionResult> DeleteGrowthHistoryAsync([FromRoute(Name = "plant-growth-history-id")] int plantGrowthHistoryId, [FromQuery]int? userId)
        {
            try
            {
                if (!userId.HasValue)
                    userId = _jwtTokenService.GetUserIdFromToken();
                if (!userId.HasValue)
                {
                    return BadRequest(new BusinessResult
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "User Id is required"
                    });
                }
                var result = await _plantGrowthHistoryService.deleteGrowthHistory(plantGrowthHistoryId, userId.Value);
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

        [HttpGet(APIRoutes.PlantGrowthHistory.exportCSV)]
        [HybridAuthorize($"{nameof(RoleEnum.ADMIN)},{nameof(RoleEnum.OWNER)},{nameof(RoleEnum.MANAGER)},{nameof(RoleEnum.EMPLOYEE)}")]
        [CheckUserFarmAccess]
        //[FarmExpired]
        public async Task<IActionResult> ExportNotes([FromQuery] int plantId)
        {
            var result = await _plantGrowthHistoryService.ExportNotesByPlantId(plantId);

            if (result.Data is ExportFileResult file && file.FileBytes?.Length > 0)
            {
                return File(file.FileBytes, file.ContentType, file.FileName);
            }

            return NotFound(result.Message);
        }

    }
}
