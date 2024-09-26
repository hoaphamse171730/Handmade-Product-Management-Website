using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.ReplyModelViews;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<ActionResult<BaseResponse<IList<ReplyModel>>>> GetAll(int pageNumber = 1, int pageSize = 10)
        {
            var replies = await _replyService.GetAllAsync(pageNumber, pageSize);
            return Ok(BaseResponse<IList<ReplyModel>>.OkResponse(replies));
        }

        [HttpGet("{replyId}")]
        public async Task<ActionResult<BaseResponse<ReplyModel>>> GetById(string replyId)
        {
            var reply = await _replyService.GetByIdAsync(replyId);
            if (reply == null)
            {
                return NotFound(new BaseResponse<ReplyModel>(StatusCodeHelper.BadRequest, "Reply not found.", string.Empty));
            }

            return Ok(new BaseResponse<ReplyModel>(StatusCodeHelper.OK, "Success", reply));
        }

        [HttpPost]
        public async Task<ActionResult<BaseResponse<ReplyModel>>> Create(string content, string reviewId, string shopId)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return BadRequest(new BaseResponse<ReplyModel>(StatusCodeHelper.BadRequest, "Invalid reply data.", string.Empty));
            }

            var replyModel = new ReplyModel
            {
                Content = content,
                ReviewId = reviewId,
                ShopId = shopId
            };

            var createdReply = await _replyService.CreateAsync(replyModel);
            return Ok(new BaseResponse<ReplyModel>(StatusCodeHelper.OK, "Reply created successfully.", createdReply));
        }

        [HttpPut("{replyId}")]
        public async Task<ActionResult<BaseResponse<ReplyModel>>> Update(string replyId, string? content)
        {
            var existingReply = await _replyService.GetByIdAsync(replyId);
            if (existingReply == null) return NotFound(new BaseResponse<ReplyModel>(StatusCodeHelper.BadRequest, "Reply not found.", string.Empty));

            existingReply.Content = content;

            var updatedReply = await _replyService.UpdateAsync(replyId, existingReply);
            return Ok(new BaseResponse<ReplyModel>(StatusCodeHelper.OK, "Reply updated successfully.", updatedReply));
        }

        [HttpDelete("{replyId}")]
        public async Task<ActionResult<BaseResponse<bool>>> Delete(string replyId)
        {
            var isDeleted = await _replyService.DeleteAsync(replyId);
            if (!isDeleted)
            {
                return NotFound(new BaseResponse<bool>(StatusCodeHelper.BadRequest, "Reply not found.", string.Empty));
            }

            return Ok(new BaseResponse<bool>(StatusCodeHelper.OK, "Reply deleted successfully.", true));
        }
    }
}
