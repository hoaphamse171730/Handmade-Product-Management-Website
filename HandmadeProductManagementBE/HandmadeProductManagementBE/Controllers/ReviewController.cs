using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.ReviewModelViews;
using Microsoft.AspNetCore.Authorization;
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

        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetByProductId([Required] string productId, int pageNumber = 1, int pageSize = 10)
        {
            var reviews = await _reviewService.GetByProductIdAsync(productId, pageNumber, pageSize);
            var response = new BaseResponse<IList<ReviewModel>>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Reviews retrieved successfully.",
                Data = reviews
            };
            return Ok(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 10)
        {
            var reviews = await _reviewService.GetByPageAsync(pageNumber, pageSize);
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


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(string? content, [Required] int rating, [Required] string productId, [Required] string orderId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var userName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

            var reviewModel = new ReviewModel
            {
                Content = content,
                Rating = rating,
                ProductId = productId,
                UserId = Guid.Parse(userId)
            };

            var createdReview = await _reviewService.CreateAsync(reviewModel,orderId);

            var response = new BaseResponse<bool>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Review created successfully.",
                Data = createdReview
            };
            return Ok(response);
        }

        [Authorize]
        [HttpPut("{reviewId}")]
        public async Task<IActionResult> Update([Required] string reviewId, string? content, int? rating)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

            var existingReview = await _reviewService.GetByIdAsync(reviewId);
            existingReview.Content = content;
            existingReview.Rating = rating ?? existingReview.Rating;

            var updatedReview = await _reviewService.UpdateAsync(reviewId, Guid.Parse(userId), existingReview);
            var response = new BaseResponse<ReviewModel>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Review updated successfully."
            };
            return Ok(response);
        }

        [Authorize]
        [HttpDelete("{reviewId}")]
        public async Task<IActionResult> Delete([Required] string reviewId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

            var result = await _reviewService.DeleteAsync(reviewId, Guid.Parse(userId));
            var response = new BaseResponse<bool>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Review deleted successfully."
            };
            return Ok(response);
        }

        [Authorize]
        [HttpDelete("{reviewId}/softdelete")]
        public async Task<IActionResult> SoftDelete([Required] string reviewId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

            var result = await _reviewService.SoftDeleteAsync(reviewId, Guid.Parse(userId));
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