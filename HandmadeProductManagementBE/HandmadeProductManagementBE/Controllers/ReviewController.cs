using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.ReviewModelViews;
using Microsoft.AspNetCore.Mvc;

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
            var reviews = await _reviewService.GetAllAsync(pageNumber, pageSize);
            return Ok(BaseResponse<IList<ReviewModel>>.OkResponse(reviews));
        }

        [HttpGet("{reviewId}")]
        public async Task<ActionResult<BaseResponse<ReviewModel>>> GetById(string reviewId)
        {
            var review = await _reviewService.GetByIdAsync(reviewId);
            if (review == null)
            {
                return NotFound(new BaseResponse<ReviewModel>(StatusCodeHelper.BadRequest, "Review not found.", string.Empty));
            }

            return Ok(new BaseResponse<ReviewModel>(StatusCodeHelper.OK, "Success", review));
        }

        [HttpPost]
        public async Task<ActionResult<BaseResponse<ReviewModel>>> Create(string content, int rating, string productId, Guid userId)
        {
            if (string.IsNullOrWhiteSpace(content) || rating < 1 || rating > 5)
            {
                return BadRequest(new BaseResponse<ReviewModel>(StatusCodeHelper.BadRequest, "Invalid review data.", string.Empty));
            }

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

        [HttpPut("{reviewId}")]
        public async Task<ActionResult<BaseResponse<ReviewModel>>> Update(string reviewId, string? content, int? rating)
        {
            var existingReview = await _reviewService.GetByIdAsync(reviewId);
            if (existingReview == null) return NotFound(new BaseResponse<ReviewModel>(StatusCodeHelper.BadRequest, "Review not found.", string.Empty));

            existingReview.Content = content;
            existingReview.Rating = rating;

            var updatedReview = await _reviewService.UpdateAsync(reviewId, existingReview);
            return Ok(new BaseResponse<ReviewModel>(StatusCodeHelper.OK, "Review updated successfully.", updatedReview));
        }

        [HttpDelete("{reviewId}")]
        public async Task<ActionResult<BaseResponse<bool>>> Delete(string reviewId)
        {
            var isDeleted = await _reviewService.DeleteAsync(reviewId);
            if (!isDeleted)
            {
                return NotFound(new BaseResponse<bool>(StatusCodeHelper.BadRequest, "Review not found.", string.Empty));
            }

            return Ok(new BaseResponse<bool>(StatusCodeHelper.OK, "Review deleted successfully.", true));
        }
    }
}
