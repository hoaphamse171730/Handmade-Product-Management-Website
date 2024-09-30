using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.ReviewModelViews;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace HandmadeProductManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpGet]
        public async Task<ActionResult<BaseResponse<IList<ReviewModel>>>> GetAll(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var reviews = await _reviewService.GetAllAsync(pageNumber, pageSize);
                return Ok(BaseResponse<IList<ReviewModel>>.OkResponse(reviews));
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

        [HttpGet("{reviewId}")]
        public async Task<ActionResult<BaseResponse<ReviewModel>>> GetById([Required] string reviewId)
        {
            var review = await _reviewService.GetByIdAsync(reviewId);
            if (review == null)
            {
                return NotFound(new BaseResponse<ReviewModel>(StatusCodeHelper.BadRequest, "Review not found.", string.Empty));
            }

            return Ok(new BaseResponse<ReviewModel>(StatusCodeHelper.OK, "Success", review));
        }

        [HttpPost]
        public async Task<ActionResult<BaseResponse<ReviewModel>>> Create(string? content, [Required] int rating, [Required] string productId, [Required] Guid userId)
        {
            try
            {
                var reviewModel = new ReviewModel
                {
                    Content = content,
                    Rating = rating,
                    ProductId = productId,
                    UserId = userId
                };

                var createdReview = await _reviewService.CreateAsync(reviewModel);
                return Ok(new BaseResponse<ReviewModel>(StatusCodeHelper.OK, "Review created successfully.", createdReview));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new BaseResponse<ReviewModel>(StatusCodeHelper.BadRequest, ex.Message, "Please input again a correct value."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<ReviewModel>(StatusCodeHelper.ServerError, "An unexpected error occurred.", "Invalid value. Please try again."));
            }
        }

        [HttpPut("{reviewId}")]
        public async Task<ActionResult<BaseResponse<ReviewModel>>> Update([Required] string reviewId, string? content, int? rating)
        {
            try
            {
                var existingReview = await _reviewService.GetByIdAsync(reviewId);
                if (existingReview == null)
                {
                    return NotFound(new BaseResponse<ReviewModel>(StatusCodeHelper.BadRequest, "Review not found.", "Review is empty."));
                }

                existingReview.Content = content;
                existingReview.Rating = rating;

                var updatedReview = await _reviewService.UpdateAsync(reviewId, existingReview);
                return Ok(new BaseResponse<ReviewModel>(StatusCodeHelper.OK, "Review updated successfully.", updatedReview));

            }
            catch (ArgumentException ex)
            {
                return BadRequest(new BaseResponse<ReviewModel>(StatusCodeHelper.BadRequest, ex.Message, "Please input again a correct value."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<ReviewModel>(StatusCodeHelper.ServerError, "An unexpected error occurred.", string.Empty));
            }
        }

        [HttpDelete("{reviewId}")]
        public async Task<ActionResult<BaseResponse<bool>>> Delete([Required] string reviewId)
        {
            try
            {
                var isDeleted = await _reviewService.DeleteAsync(reviewId);
                if (!isDeleted)
                {
                    return NotFound(new BaseResponse<bool>(StatusCodeHelper.BadRequest, "Review not found.", "Review is empty."));
                }

                return Ok(new BaseResponse<bool>(StatusCodeHelper.OK, "Review deleted successfully.", true));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<bool>(StatusCodeHelper.ServerError, "An unexpected error occurred.", string.Empty));
            }
        }

        [HttpDelete("{reviewId}/softdelete")]
        public async Task<ActionResult<BaseResponse<bool>>> SoftDelete([Required] string reviewId)
        {
            try
            {
                var isSoftDeleted = await _reviewService.SoftDeleteAsync(reviewId);
                if (!isSoftDeleted)
                {
                    return NotFound(new BaseResponse<bool>(StatusCodeHelper.BadRequest, "Review not found.", "Review is empty."));
                }

                return Ok(new BaseResponse<bool>(StatusCodeHelper.OK, "Review soft deleted successfully.", true));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new BaseResponse<bool>(StatusCodeHelper.BadRequest, ex.Message, "Please input again a correct value."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<bool>(StatusCodeHelper.ServerError, "An unexpected error occurred.", string.Empty));
            }
        }
    }
}
