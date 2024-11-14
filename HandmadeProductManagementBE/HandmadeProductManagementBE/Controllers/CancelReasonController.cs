﻿using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using Microsoft.AspNetCore.Mvc;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.CancelReasonModelViews;
using Microsoft.AspNetCore.Authorization;
using HandmadeProductManagement.Contract.Repositories.Entity;
using Microsoft.EntityFrameworkCore;

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
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
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
        // PATCH: api/CancelReason/{id} (string id)
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchCancelReason(string id, [FromBody] CancelReasonForUpdateDto updatedReason)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
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
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            

            var response = new BaseResponse<bool>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = $"Cancel Reason with ID {id} has been successfully deleted.",
                Data = await _cancelReasonService.Delete(id, userId)
            };
            return Ok(response);
        }

        [Authorize(Roles = "Admin")]
        // GET: api/cancelreason/deleted
        [HttpGet("deleted")]
        public async Task<IActionResult> GetDeletedCancelReasons()
        {
            var response = new BaseResponse<IList<CancelReasonDeletedDto>>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Get all deleted Cancel Reasons successfully!",
                Data = await _cancelReasonService.GetDeletedCancelReasons()
            };
            return Ok(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/recover")]
        public async Task<IActionResult> PatchReverseDeleteCancelReason(string id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var result = await _cancelReasonService.PatchReverseDelete(id, userId);

            var response = new BaseResponse<bool>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = $"Cancel Reason has been successfully recovered.",
                Data = result
            };
            return Ok(response);
        }

        [Authorize]
        [HttpGet("description/{description}")]
        public async Task<IActionResult> GetByDescription(string description)
        {
            var cancelReason = await _cancelReasonService.GetByDescription(description);
            var response = new BaseResponse<CancelReasonDto>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Get Cancel Reason by description successfully!",
                Data = cancelReason
            };
            return Ok(response);
        }
    }
}