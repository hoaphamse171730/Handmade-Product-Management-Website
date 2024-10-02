using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using Microsoft.AspNetCore.Mvc;
using HandmadeProductManagement.Core.Constants;
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
            var response = new BaseResponse<IList<CancelReasonResponseModel>>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Get Cancel Reason sucessfully.",
                Data = await _cancelReasonService.GetByPage(page, pageSize)
            };
            return Ok(response);
        }

        // POST: api/CancelReason
        [HttpPost]
        public async Task<IActionResult> CreateCancelReason([FromBody] CreateCancelReasonDto reason)
        {
            await _cancelReasonService.Create(reason);
            var response = new BaseResponse<CancelReasonResponseModel>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Created Cancel Reason successfully.",
                Data = null
            };
            return Ok(response);
        }

        // PUT: api/CancelReason/{id} (string id)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCancelReason(string id, CreateCancelReasonDto updatedReason)
        {
            await _cancelReasonService.Update(id, updatedReason);
            var response = new BaseResponse<CancelReasonResponseModel>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Updated Cancel Reason successfully.",
                Data = null
            };
            return Ok(response);
        }

        // DELETE: api/CancelReason/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> SoftDeleteCancelReason(string id)
        {
            await _cancelReasonService.Delete(id);

            var response = new BaseResponse<string>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = $"Cancel Reason with ID {id} has been successfully deleted.",
                Data = null
            };
            return Ok(response);
        }
    }
}
