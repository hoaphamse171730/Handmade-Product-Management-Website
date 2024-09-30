using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Contract.Repositories.Entity;
using Microsoft.AspNetCore.Mvc;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Utils;
using HandmadeProductManagement.ModelViews.CancelReasonModelViews;

namespace HandmadeProductManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CancelReasonController : ControllerBase
    {
        private readonly ICancelReasonService _cancelReasonService;

        public CancelReasonController(ICancelReasonService cancelReasonService)
        {
            _cancelReasonService = cancelReasonService;
        }

        // GET: api/cancelreason/page?page=1&pageSize=10
        [HttpGet("page")]
        public async Task<IActionResult> GetByPage([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var paginatedResult = await _cancelReasonService.GetByPage(page, pageSize);
                // Wrap result in BaseResponse
                return Ok(BaseResponse<IList<CancelReasonResponseModel>>.OkResponse(paginatedResult));
            }
            catch (Exception ex)
            {
                // Handle exceptions and return appropriate response
                return StatusCode(500, BaseResponse<string>.FailResponse(ex.Message));
            }
        }

        // POST: api/CancelReason
        [HttpPost]
        public async Task<IActionResult> CreateCancelReason([FromBody] CreateCancelReasonDto reason)
        {
            try
            {
                var cancelReasonModel = await _cancelReasonService.Create(reason);
                return Ok(BaseResponse<CancelReasonResponseModel>.OkResponse("Created Cancel Reason sucessfully!"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(BaseResponse<string>.FailResponse(ex.Message));
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, BaseResponse<string>.FailResponse(ex.Message));
            }
        }

        // PUT: api/CancelReason/{id} (string id)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCancelReason(string id, CreateCancelReasonDto updatedReason)
        {
            try
            {
                var reason = await _cancelReasonService.Update(id, updatedReason);
                return Ok(BaseResponse<CancelReasonResponseModel>.OkResponse("Updated Cancel Reason sucessfully!"));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(BaseResponse<string>.FailResponse("Cancel Reason not found", StatusCodeHelper.NotFound));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(BaseResponse<string>.FailResponse(ex.Message));
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, BaseResponse<string>.FailResponse(ex.Message, StatusCodeHelper.ServerError));
            }
        }

        // PUT: api/CancelReason/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> SoftDeleteCancelReason(string id)
        {
            try
            {
                bool success = await _cancelReasonService.Delete(id);
                if (!success)
                {
                    return NotFound(BaseResponse<string>.FailResponse($"Cancel Reason with ID {id} not found.", StatusCodeHelper.NotFound));
                }
                return Ok(BaseResponse<string>.OkResponse($"Cancel Reason with ID {id} has been successfully deleted."));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(BaseResponse<string>.FailResponse("Cancel Reason not found", StatusCodeHelper.NotFound));
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, BaseResponse<string>.FailResponse(ex.Message, StatusCodeHelper.ServerError));
            }
            
        }

    }
}
