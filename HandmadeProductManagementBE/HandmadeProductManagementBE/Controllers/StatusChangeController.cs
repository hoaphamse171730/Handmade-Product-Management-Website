using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Contract.Repositories.Entity;
using Microsoft.AspNetCore.Mvc;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Services.Service;
using HandmadeProductManagement.ModelViews.StatusChangeModelViews;

namespace HandmadeProductManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatusChangeController : ControllerBase
    {
        private readonly IStatusChangeService _statusChangeService;

        public StatusChangeController(IStatusChangeService statusChangeService)
        {
            _statusChangeService = statusChangeService;
        }

        // GET: api/statuschange/page?page=1&pageSize=10
        [HttpGet("page")]
        public async Task<IActionResult> GetByPage([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var paginatedResult = await _statusChangeService.GetByPage(page, pageSize);
                return Ok(BaseResponse<IList<StatusChangeResponseModel>>.OkResponse(paginatedResult));
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponse<string>.FailResponse(ex.Message, StatusCodeHelper.ServerError));
            }
        }

        // GET: api/StatusChange/Order/{orderId}
        [HttpGet("Order/{orderId}")]
        public async Task<IActionResult> GetStatusChangesByOrderId(string orderId)
        {
            try
            {
                IList<StatusChangeResponseModel> statusChanges = await _statusChangeService.GetByOrderId(orderId);
                return Ok(BaseResponse<IList<StatusChangeResponseModel>>.OkResponse(statusChanges));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(BaseResponse<string>.FailResponse("No status changes found for the given OrderId.", StatusCodeHelper.NotFound));
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponse<string>.FailResponse(ex.Message, StatusCodeHelper.ServerError));
            }
        }

        // POST: api/statuschange
        [HttpPost]
        public async Task<IActionResult> CreateStatusChange([FromBody] CreateStatusChangeDto statusChange)
        {
            try
            {
                var createdStatusChange = await _statusChangeService.Create(statusChange);
                return Ok(BaseResponse<StatusChangeResponseModel>.OkResponse("Created Status Change successfully!"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(BaseResponse<string>.FailResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponse<string>.FailResponse(ex.Message, StatusCodeHelper.ServerError));
            }
        }

        // PUT: api/statuschange/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStatusChange(string id, [FromBody] CreateStatusChangeDto updatedStatusChange)
        {
            try
            {
                var statusChange = await _statusChangeService.Update(id, updatedStatusChange);
                return Ok(BaseResponse<StatusChangeResponseModel>.OkResponse("Updated Status Change successfully!"));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(BaseResponse<string>.FailResponse("Status Change not found", StatusCodeHelper.NotFound));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(BaseResponse<string>.FailResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponse<string>.FailResponse(ex.Message, StatusCodeHelper.ServerError));
            }
        }

        // DELETE: api/statuschange/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStatusChange(string id)
        {
            try
            {
                bool success = await _statusChangeService.Delete(id);
                if (!success)
                {
                    return NotFound(BaseResponse<string>.FailResponse($"Status Change with ID {id} not found.", StatusCodeHelper.NotFound));
                }
                return Ok(BaseResponse<string>.OkResponse($"Status Change with ID {id} has been successfully deleted."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponse<string>.FailResponse(ex.Message, StatusCodeHelper.ServerError));
            }
        }
    }
}
