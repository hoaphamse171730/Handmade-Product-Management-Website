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
            try
            {
                var replies = await _replyService.GetAllAsync(pageNumber, pageSize);
                var response = new BaseResponse<IList<ReplyModel>>
                {
                    Code = "Success",
                    StatusCode = StatusCodeHelper.OK,
                    Message = "Replies retrieved successfully.",
                    Data = replies
                };
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                var response = new BaseResponse<IList<ReplyModel>>
                {
                    Code = "BadRequest",
                    StatusCode = StatusCodeHelper.BadRequest,
                    Message = ex.Message,
                    Data = null
                };
                return BadRequest(response);
            }
            catch (Exception)
            {
                var response = new BaseResponse<IList<ReplyModel>>
                {
                    Code = "ServerError",
                    StatusCode = StatusCodeHelper.ServerError,
                    Message = "An unexpected error occurred.",
                    Data = null
                };
                return StatusCode(500, response);
            }
        }

        [HttpGet("{replyId}")]
        public async Task<IActionResult> GetById([Required] string replyId)
        {
            try
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
            catch (ArgumentException ex)
            {
                var response = new BaseResponse<ReplyModel>
                {
                    Code = "NotFound",
                    StatusCode = StatusCodeHelper.NotFound,
                    Message = ex.Message,
                    Data = null
                };
                return NotFound(response);
            }
            catch (Exception)
            {
                var response = new BaseResponse<ReplyModel>
                {
                    Code = "ServerError",
                    StatusCode = StatusCodeHelper.ServerError,
                    Message = "An unexpected error occurred.",
                    Data = null
                };
                return StatusCode(500, response);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([Required] string content, [Required] string reviewId, [Required] string shopId)
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
                var response = new BaseResponse<ReplyModel>
                {
                    Code = "Success",
                    StatusCode = StatusCodeHelper.OK,
                    Message = "Reply created successfully.",
                    Data = createdReply
                };
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                var response = new BaseResponse<ReplyModel>
                {
                    Code = "BadRequest",
                    StatusCode = StatusCodeHelper.BadRequest,
                    Message = ex.Message,
                    Data = null
                };
                return BadRequest(response);
            }
            catch (Exception)
            {
                var response = new BaseResponse<ReplyModel>
                {
                    Code = "ServerError",
                    StatusCode = StatusCodeHelper.ServerError,
                    Message = "An unexpected error occurred.",
                    Data = null
                };
                return StatusCode(500, response);
            }
        }

        [HttpPut("{replyId}")]
        public async Task<IActionResult> Update([Required] string replyId, [Required] string content, [Required] string shopId)
        {
            try
            {
                var replyModel = new ReplyModel
                {
                    Content = content,
                    ShopId = shopId
                };

                var updatedReply = await _replyService.UpdateAsync(replyId, replyModel);
                var response = new BaseResponse<ReplyModel>
                {
                    Code = "Success",
                    StatusCode = StatusCodeHelper.OK,
                    Message = "Reply updated successfully."
                };
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                var response = new BaseResponse<ReplyModel>
                {
                    Code = "BadRequest",
                    StatusCode = StatusCodeHelper.BadRequest,
                    Message = ex.Message,
                    Data = null
                };
                return BadRequest(response);
            }
            catch (Exception)
            {
                var response = new BaseResponse<ReplyModel>
                {
                    Code = "ServerError",
                    StatusCode = StatusCodeHelper.ServerError,
                    Message = "An unexpected error occurred.",
                    Data = null
                };
                return StatusCode(500, response);
            }
        }

        [HttpDelete("{replyId}")]
        public async Task<IActionResult> Delete([Required] string replyId)
        {
            try
            {
                var result = await _replyService.DeleteAsync(replyId);
                var response = new BaseResponse<bool>
                {
                    Code = "Success",
                    StatusCode = StatusCodeHelper.OK,
                    Message = "Reply deleted successfully."
                };
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                var response = new BaseResponse<bool>
                {
                    Code = "BadRequest",
                    StatusCode = StatusCodeHelper.BadRequest,
                    Message = ex.Message,
                    Data = false
                };
                return BadRequest(response);
            }
            catch (Exception)
            {
                var response = new BaseResponse<bool>
                {
                    Code = "ServerError",
                    StatusCode = StatusCodeHelper.ServerError,
                    Message = "An unexpected error occurred.",
                    Data = false
                };
                return StatusCode(500, response);
            }
        }

        [HttpDelete("{replyId}/soft-delete")]
        public async Task<IActionResult> SoftDelete(string replyId)
        {
            try
            {
                var result = await _replyService.SoftDeleteAsync(replyId);
                var response = new BaseResponse<bool>
                {
                    Code = "Success",
                    StatusCode = StatusCodeHelper.OK,
                    Message = "Reply soft-deleted successfully."
                };
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                var response = new BaseResponse<bool>
                {
                    Code = "BadRequest",
                    StatusCode = StatusCodeHelper.BadRequest,
                    Message = ex.Message,
                    Data = false
                };
                return BadRequest(response);
            }
            catch (Exception)
            {
                var response = new BaseResponse<bool>
                {
                    Code = "ServerError",
                    StatusCode = StatusCodeHelper.ServerError,
                    Message = "An unexpected error occurred.",
                    Data = false
                };
                return StatusCode(500, response);
            }
        }
    }
}