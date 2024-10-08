using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.ReplyModelViews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace HandmadeProductManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReplyController : ControllerBase
    {
        private readonly IReplyService _replyService;

        public ReplyController(IReplyService replyService)
        {
            _replyService = replyService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 10)
        {
            var replies = await _replyService.GetByPageAsync(pageNumber, pageSize);
            var response = new BaseResponse<IList<ReplyModel>>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Replies retrieved successfully.",
                Data = replies
            };
            return Ok(response);
        }

        [HttpGet("{replyId}")]
        public async Task<IActionResult> GetById([Required] string replyId)
        {
            var reply = await _replyService.GetByIdAsync(replyId);
            var response = new BaseResponse<ReplyModel>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Reply retrieved successfully.",
                Data = reply
            };
            return Ok(response);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([Required] string content, [Required] string reviewId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var replyModel = new ReplyModel
            {
                Content = content,
                ReviewId = reviewId,
            };

            var createdReply = await _replyService.CreateAsync(replyModel, Guid.Parse(userId));
            var response = new BaseResponse<bool>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Reply created successfully.",
                Data = createdReply
            };
            return Ok(response);
        }

        [Authorize]
        [HttpPut("{replyId}")]
        public async Task<IActionResult> Update([Required] string replyId, [Required] string content)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var replyModel = new ReplyModel
            {
                Content = content
            };

            var updatedReply = await _replyService.UpdateAsync(replyId, Guid.Parse(userId), replyModel);
            var response = new BaseResponse<ReplyModel>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Reply updated successfully."
            };
            return Ok(response);
        }

        [Authorize]
        [HttpDelete("{replyId}")]
        public async Task<IActionResult> Delete([Required] string replyId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var result = await _replyService.DeleteAsync(replyId, Guid.Parse(userId));
            var response = new BaseResponse<bool>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Reply deleted successfully."
            };
            return Ok(response);
        }

        [Authorize]
        [HttpDelete("{replyId}/soft-delete")]
        public async Task<IActionResult> SoftDelete([Required] string replyId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var result = await _replyService.SoftDeleteAsync(replyId, Guid.Parse(userId));
            var response = new BaseResponse<bool>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Reply soft-deleted successfully."
            };
            return Ok(response);
        }
    }
}