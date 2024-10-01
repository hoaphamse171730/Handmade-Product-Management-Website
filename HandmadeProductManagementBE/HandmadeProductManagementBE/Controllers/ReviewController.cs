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
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 10)
        {
            var reviews = await _reviewService.GetAllAsync(pageNumber, pageSize);
            var response = new BaseResponse<IList<ReviewModel>>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Reviews retrieved successfully.",
                Data = reviews
            };
            return Ok(response);
        }

        [HttpGet("{reviewId}")]
        public async Task<IActionResult> GetById([Required] string reviewId)
        {
            var review = await _reviewService.GetByIdAsync(reviewId);
            var response = new BaseResponse<ReviewModel>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Review retrieved successfully.",
                Data = review
            };
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> Create(string? content, [Required] int rating, [Required] string productId, [Required] Guid userId)
        {
            var reviewModel = new ReviewModel
            {
                Content = content,
                Rating = rating,
                ProductId = productId,
                UserId = userId
            };

            var createdReview = await _reviewService.CreateAsync(reviewModel);
            var response = new BaseResponse<ReviewModel>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Review created successfully.",
                Data = createdReview
            };
            return Ok(response);
        }

        [HttpPut("{reviewId}")]
        public async Task<IActionResult> Update([Required] string reviewId, string? content, int? rating)
        {
            var existingReview = await _reviewService.GetByIdAsync(reviewId);
            existingReview.Content = content;
            existingReview.Rating = rating;

            var updatedReview = await _reviewService.UpdateAsync(reviewId, existingReview);
            var response = new BaseResponse<ReviewModel>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Review updated successfully."
            };
            return Ok(response);
        }

        [HttpDelete("{reviewId}")]
        public async Task<IActionResult> Delete([Required] string reviewId)
        {
            var result = await _reviewService.DeleteAsync(reviewId);
            var response = new BaseResponse<bool>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Review deleted successfully."
            };
            return Ok(response);
        }

        [HttpDelete("{reviewId}/softdelete")]
        public async Task<IActionResult> SoftDelete([Required] string reviewId)
        {
            var result = await _reviewService.SoftDeleteAsync(reviewId);
            var response = new BaseResponse<bool>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Review soft deleted successfully."
            };
            return Ok(response);
        }
    }
}