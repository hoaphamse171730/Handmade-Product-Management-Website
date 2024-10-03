using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.ReplyModelViews;
using HandmadeProductManagement.ModelViews.ReviewModelViews;
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

        [HttpPost]
        public async Task<IActionResult> Create([Required] string content, [Required] string reviewId, [Required] string shopId)
        {
            var replyModel = new ReplyModel
            {
                Content = content,
                ReviewId = reviewId,
                ShopId = shopId
            };

            var createdReply = await _replyService.CreateAsync(replyModel);
            var response = new BaseResponse<bool>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Reply created successfully.",
                Data = createdReply
            };
            return Ok(response);
        }

        [HttpPut("{replyId}")]
        public async Task<IActionResult> Update([Required] string replyId, [Required] string content, [Required] string shopId)
        {
            var replyModel = new ReplyModel
            {
                Content = content,
                ShopId = shopId
            };

            var updatedReply = await _replyService.UpdateAsync(replyId, shopId, replyModel);
            var response = new BaseResponse<ReplyModel>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Reply updated successfully."
            };
            return Ok(response);
        }

        [HttpDelete("{replyId}")]
        public async Task<IActionResult> Delete([Required] string replyId, [Required] string shopId)
        {
            var result = await _replyService.DeleteAsync(replyId, shopId);
            var response = new BaseResponse<bool>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Reply deleted successfully."
            };
            return Ok(response);
        }

        [HttpDelete("{replyId}/soft-delete")]
        public async Task<IActionResult> SoftDelete(string replyId, [Required] string shopId)
        {
            var result = await _replyService.SoftDeleteAsync(replyId, shopId);
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