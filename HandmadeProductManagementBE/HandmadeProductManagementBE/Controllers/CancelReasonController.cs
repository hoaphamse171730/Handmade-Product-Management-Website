using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using Microsoft.AspNetCore.Mvc;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.CancelReasonModelViews;
using Microsoft.AspNetCore.Authorization;
using HandmadeProductManagement.Contract.Repositories.Entity;

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

        [Authorize]
        // GET: api/cancelreason/all
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var response = new BaseResponse<IList<CancelReasonDto>>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Get all Cancel Reasons successfully!",
                Data = await _cancelReasonService.GetAll()
            };
            return Ok(response);
        }

        [Authorize(Roles = "Admin")]
        // POST: api/CancelReason
        [HttpPost]
        public async Task<IActionResult> CreateCancelReason([FromBody] CancelReasonForCreationDto reason)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var result = await _cancelReasonService.Create(reason, userId);
            var response = new BaseResponse<bool>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Created Cancel Reason successfully!",
                Data = result
            };
            return Ok(response);
        }

        [Authorize(Roles = "Admin")]
        // PUT: api/CancelReason/{id} (string id)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCancelReason(string id, CancelReasonForUpdateDto updatedReason)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var result = await _cancelReasonService.Update(id, updatedReason, userId);
            var response = new BaseResponse<bool>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Updated Cancel Reason successfully!",
                Data = result
            };
            return Ok(response);
        }

        [Authorize(Roles = "Admin")]
        // DELETE: api/CancelReason/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> SoftDeleteCancelReason(string id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            await _cancelReasonService.Delete(id, userId);

            var response = new BaseResponse<string>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = $"Cancel Reason with ID {id} has been successfully deleted.",
                Data = null
            };
            return Ok(response);
        }

        [Authorize(Roles = "Admin")]
        // GET: api/cancelreason/deleted
        [HttpGet("deleted")]
        public async Task<IActionResult> GetDeletedCancelReasons()
        {
            var response = new BaseResponse<IList<CancelReason>>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Get all deleted Cancel Reasons successfully!",
                Data = await _cancelReasonService.GetDeletedCancelReasons()
            };
            return Ok(response);
        }

        [Authorize(Roles = "Admin")]
        // PATCH: api/CancelReason/reverse-delete/{id}
        [HttpPatch("reverse-delete/{id}")]
        public async Task<IActionResult> PatchReverseDeleteCancelReason(string id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var result = await _cancelReasonService.PatchReverseDelete(id, userId);

            var response = new BaseResponse<bool>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = $"Cancel Reason with ID {id} has been successfully restored.",
                Data = result
            };
            return Ok(response);
        }
    }
}