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
        public async Task<ActionResult<BaseResponse<IList<ReplyModel>>>> GetAll(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var replies = await _replyService.GetAllAsync(pageNumber, pageSize);
                return Ok(BaseResponse<IList<ReplyModel>>.OkResponse(replies));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new BaseResponse<IList<ReviewModel>>(StatusCodeHelper.BadRequest, ex.Message, string.Empty));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<IList<ReviewModel>>(StatusCodeHelper.ServerError, "An unexpected error occurred.", string.Empty));
            }
        }

        [HttpGet("{replyId}")]
        public async Task<ActionResult<BaseResponse<ReplyModel>>> GetById([Required] string replyId)
        {
            var reply = await _replyService.GetByIdAsync(replyId);
            if (reply == null)
            {
                return NotFound(new BaseResponse<ReplyModel>(StatusCodeHelper.BadRequest, "Reply not found.", string.Empty));
            }

            return Ok(new BaseResponse<ReplyModel>(StatusCodeHelper.OK, "Success", reply));
        }

        [HttpPost]
        public async Task<ActionResult<BaseResponse<ReplyModel>>> Create([Required] string content, [Required] string reviewId, [Required] string shopId)
        {
            try
            {
                var replyModel = new ReplyModel
                {
                    Content = content,
                    ReviewId = reviewId,
                    ShopId = shopId
                };

                var createdReply = await _replyService.CreateAsync(replyModel);
                return Ok(new BaseResponse<ReplyModel>(StatusCodeHelper.OK, "Reply created successfully.", createdReply));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new BaseResponse<ReviewModel>(StatusCodeHelper.BadRequest, ex.Message, string.Empty));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<ReviewModel>(StatusCodeHelper.ServerError, "An unexpected error occurred.", "Invalid value. Please try again."));
            }

        }

        [HttpPut("{replyId}")]
        public async Task<ActionResult<BaseResponse<ReplyModel>>> Update([Required] string replyId, string? content)
        {
            try
            {
                var existingReply = await _replyService.GetByIdAsync(replyId);
                if (existingReply == null) return NotFound(new BaseResponse<ReplyModel>(StatusCodeHelper.BadRequest, "Reply not found.", string.Empty));

                existingReply.Content = content;

                var updatedReply = await _replyService.UpdateAsync(replyId, existingReply);
                return Ok(new BaseResponse<ReplyModel>(StatusCodeHelper.OK, "Reply updated successfully.", updatedReply));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new BaseResponse<ReplyModel>(StatusCodeHelper.BadRequest, ex.Message, string.Empty));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<ReplyModel>(StatusCodeHelper.ServerError, "An unexpected error occurred.", string.Empty));
            }
        }

        [HttpDelete("{replyId}")]
        public async Task<ActionResult<BaseResponse<bool>>> Delete([Required] string replyId)
        {
            var isDeleted = await _replyService.DeleteAsync(replyId);
            if (!isDeleted)
            {
                return NotFound(new BaseResponse<bool>(StatusCodeHelper.BadRequest, "Reply not found.", string.Empty));
            }

            return Ok(new BaseResponse<bool>(StatusCodeHelper.OK, "Reply deleted successfully.", true));
        }

        [HttpDelete("{replyId}/softdelete")]
        public async Task<ActionResult<BaseResponse<bool>>> SoftDelete([Required] string replyId)
        {
            try
            {
                var isSoftDeleted = await _replyService.SoftDeleteAsync(replyId);
                if (!isSoftDeleted)
                {
                    return NotFound(new BaseResponse<bool>(StatusCodeHelper.BadRequest, "Reply not found.", "Reply is empty."));
                }

                return Ok(new BaseResponse<bool>(StatusCodeHelper.OK, "Reply soft deleted successfully.", true));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new BaseResponse<bool>(StatusCodeHelper.BadRequest, ex.Message, "Please input a correct value."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<bool>(StatusCodeHelper.ServerError, "An unexpected error occurred.", string.Empty));
            }
        }

    }
}
