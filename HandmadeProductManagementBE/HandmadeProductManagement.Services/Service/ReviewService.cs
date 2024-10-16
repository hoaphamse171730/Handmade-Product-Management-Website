using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.ReplyModelViews;
using HandmadeProductManagement.ModelViews.ReviewModelViews;
using HandmadeProductManagement.Repositories.Entity;
using Microsoft.EntityFrameworkCore;

namespace HandmadeProductManagement.Services.Service
{
    public class ReviewService : IReviewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProductService _productService;
        private readonly IShopService _shopService;
        DateTime vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

        public ReviewService(IUnitOfWork unitOfWork, IProductService productService, IShopService shopService)
        {
            _unitOfWork = unitOfWork;
            _productService = productService;
            _shopService = shopService;
        }

        public async Task<IList<ReviewModel>> GetByProductIdAsync(string productId, int pageNumber, int pageSize)
        {
            if (string.IsNullOrWhiteSpace(productId) || !Guid.TryParse(productId, out _))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidGuidFormat);
            }

            if (pageNumber <= 0)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidPageNumber);
            }

            if (pageSize <= 0)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidPageSize);
            }

            var productExists = await _unitOfWork.GetRepository<Product>().GetByIdAsync(productId)
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageProductNotFound);

            if (productExists.DeletedTime != null)
            {
                throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageProductSoftDeleted);
            }

            var reviews = await _unitOfWork.GetRepository<Review>()
                                           .Entities
                                           .Include(r => r.Reply)
                                           .Where(r => r.ProductId == productId && r.DeletedTime == null)
                                           .OrderByDescending(r => r.Date)
                                           .Skip((pageNumber - 1) * pageSize)
                                           .Take(pageSize)
                                           .ToListAsync();

            return reviews.Select(r => new ReviewModel
            {
                Id = r.Id,
                Content = r.Content,
                Rating = r.Rating,
                Date = r.Date,
                ProductId = r.ProductId,
                UserId = r.UserId,
                Reply = r.Reply == null ? null : new ReplyModel
                {
                    Id = r.Reply.Id,
                    Content = r.Reply.Content,
                    Date = r.Reply.Date,
                    ReviewId = r.Reply.ReviewId,
                    ShopId = r.Reply.ShopId
                }
            }).ToList();
        }

        public async Task<IList<ReviewModel>> GetByPageAsync(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidPageNumber);
            }

            if (pageSize <= 0)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidPageSize);
            }

            var reviews = await _unitOfWork.GetRepository<Review>()
                                           .Entities
                                           .Include(r => r.Reply)
                                           .Where(r => r.DeletedTime == null)
                                           .Skip((pageNumber - 1) * pageSize)
                                           .Take(pageSize)
                                           .ToListAsync();

            return reviews.Select(r => new ReviewModel
            {
                Id = r.Id,
                Content = r.Content,
                Rating = r.Rating,
                Date = r.Date,
                ProductId = r.ProductId,
                UserId = r.UserId,
                Reply = r.Reply == null ? null : new ReplyModel
                {
                    Id = r.Reply.Id,
                    Content = r.Reply.Content,
                    Date = r.Reply.Date,
                    ReviewId = r.Reply.ReviewId,
                    ShopId = r.Reply.ShopId
                }
            }).ToList();
        }

        public async Task<ReviewModel> GetByIdAsync(string reviewId)
        {
            if (string.IsNullOrWhiteSpace(reviewId) || !Guid.TryParse(reviewId, out _))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidReviewIdFormat);
            }

            var review = await _unitOfWork.GetRepository<Review>()
                                          .Entities
                                          .Include(r => r.Reply)
                                          .FirstOrDefaultAsync(r => r.Id == reviewId && r.DeletedTime == null)
                                          ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageReviewNotFound);

            return new ReviewModel
            {
                Id = review.Id,
                Content = review.Content,
                Rating = review.Rating,
                Date = review.Date,
                ProductId = review.ProductId,
                UserId = review.UserId,
                Reply = review.Reply == null ? null : new ReplyModel
                {
                    Id = review.Reply.Id,
                    Content = review.Reply.Content,
                    Date = review.Reply.Date,
                    ReviewId = review.Reply.ReviewId,
                    ShopId = review.Reply.ShopId
                }
            };
        }

        public async Task<bool> CreateAsync(ReviewModel reviewModel, string orderId)
        {
            if (string.IsNullOrWhiteSpace(reviewModel.ProductId) || !Guid.TryParse(reviewModel.ProductId, out _))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidGuidFormat);
            }

            // Check if the productId exists in the database
            var productExists = await _unitOfWork.GetRepository<Product>().GetByIdAsync(reviewModel.ProductId)
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageProductNotFound);

            if (string.IsNullOrWhiteSpace(orderId) || !Guid.TryParse(orderId, out _))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidGuidFormat);
            }

            // Check if the order exists
            var order = await _unitOfWork.GetRepository<Order>()
                                         .Entities
                                         .Include(o => o.OrderDetails)
                                         .FirstOrDefaultAsync(o => o.Id == orderId)
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageOrderNotFound);

            // Check if the user owns the order
            if (order.UserId != reviewModel.UserId)
            {
                throw new BaseException.ForbiddenException(StatusCodeHelper.Forbidden.ToString(), Constants.ErrorMessageForbidden);
            }

            // Check if the order status is "Shipped"
            if (order.Status != Constants.OrderStatusShipped)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageOrderNotShipped);
            }

            // Check if a review already exists for this product by this user
            var existingReview = await _unitOfWork.GetRepository<Review>()
                                                  .Entities
                                                  .FirstOrDefaultAsync(r => r.ProductId == reviewModel.ProductId && r.UserId == reviewModel.UserId);
            if (existingReview != null)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageReviewAlreadyExists);
            }

            if (reviewModel.Rating < 1 || reviewModel.Rating > 5)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidRating);
            }

            var user = await _unitOfWork.GetRepository<ApplicationUser>()
                                         .Entities
                                         .FirstOrDefaultAsync(u => u.Id == reviewModel.UserId)
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageUserNotFound);

            var review = new Review
            {
                Content = reviewModel.Content,
                Rating = reviewModel.Rating,
                Date = vietnamTime,
                ProductId = reviewModel.ProductId,
                UserId = reviewModel.UserId,
                CreatedBy = reviewModel.UserId.ToString(),
                LastUpdatedBy = reviewModel.UserId.ToString()
            };

            await _unitOfWork.GetRepository<Review>().InsertAsync(review);
            await _unitOfWork.SaveAsync();

            // Update product rating
            await _productService.CalculateAverageRatingAsync(reviewModel.ProductId);

            // Update shop rating
            var product = await _unitOfWork.GetRepository<Product>().GetByIdAsync(reviewModel.ProductId);
            await _shopService.CalculateShopAverageRatingAsync(product.ShopId);

            return true;
        }

        public async Task<bool> UpdateAsync(string reviewId, Guid userId, ReviewModel updatedReview)
        {
            if (string.IsNullOrWhiteSpace(reviewId) || !Guid.TryParse(reviewId, out _))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidReviewIdFormat);
            }

            // Check if the review exists
            var existingReview = await _unitOfWork.GetRepository<Review>()
                                                  .Entities
                                                  .Include(r => r.User)
                                                  .FirstOrDefaultAsync(r => r.Id == reviewId)
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageReviewNotFound);

            // Check if the user has permission to update the review
            if (existingReview.UserId != userId)
            {
                throw new BaseException.ForbiddenException(StatusCodeHelper.Forbidden.ToString(), Constants.ErrorMessageForbidden);
            }

            // Update Content only if it's provided
            if (!string.IsNullOrWhiteSpace(updatedReview.Content))
            {
                existingReview.Content = updatedReview.Content;
            }

            // Update Rating only if it's within valid range
            if (updatedReview.Rating.HasValue)
            {
                if (updatedReview.Rating < 1 || updatedReview.Rating > 5)
                {
                    throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidRating);
                }
                existingReview.Rating = updatedReview.Rating;
            }

            if (existingReview.DeletedTime != null)
            {
                throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageReviewNotFoundSoftDeleted);
            }

            existingReview.LastUpdatedBy = existingReview.UserId.ToString();
            existingReview.LastUpdatedTime = vietnamTime;

            await _unitOfWork.GetRepository<Review>().UpdateAsync(existingReview);
            await _unitOfWork.SaveAsync();

            // Update product rating
            await _productService.CalculateAverageRatingAsync(existingReview.ProductId);

            // Update shop rating
            var product = await _unitOfWork.GetRepository<Product>().GetByIdAsync(existingReview.ProductId);
            await _shopService.CalculateShopAverageRatingAsync(product.ShopId);

            return true;
        }

        public async Task<bool> DeleteAsync(string reviewId, Guid userId)
        {
            if (string.IsNullOrWhiteSpace(reviewId) || !Guid.TryParse(reviewId, out _))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidReviewIdFormat);
            }

            // Check if the review exists
            var existingReview = await _unitOfWork.GetRepository<Review>()
                                                  .Entities
                                                  .FirstOrDefaultAsync(r => r.Id == reviewId)
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageReviewNotFound);

            // Check if the user has permission to delete the review
            if (existingReview.UserId != userId)
            {
                throw new BaseException.ForbiddenException(StatusCodeHelper.Forbidden.ToString(), Constants.ErrorMessageForbidden);
            }

            if (existingReview.DeletedTime != null)
            {
                throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageReviewNotFoundSoftDeleted);
            }

            existingReview.DeletedBy = userId.ToString();

            await _unitOfWork.GetRepository<Review>().DeleteAsync(existingReview.Id);
            await _unitOfWork.SaveAsync();

            // Update product rating
            await _productService.CalculateAverageRatingAsync(existingReview.ProductId);

            // Update shop rating
            var product = await _unitOfWork.GetRepository<Product>().GetByIdAsync(existingReview.ProductId);
            await _shopService.CalculateShopAverageRatingAsync(product.ShopId);

            return true;
        }

        public async Task<bool> SoftDeleteAsync(string reviewId, Guid userId)
        {
            if (string.IsNullOrWhiteSpace(reviewId) || !Guid.TryParse(reviewId, out _))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidReviewIdFormat);
            }

            // Check if the review exists
            var existingReview = await _unitOfWork.GetRepository<Review>()
                                                  .Entities
                                                  .FirstOrDefaultAsync(r => r.Id == reviewId)
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageReviewNotFound);

            // Check if the user has permission to delete the review
            if (existingReview.UserId != userId)
            {
                throw new BaseException.ForbiddenException(StatusCodeHelper.Forbidden.ToString(), Constants.ErrorMessageForbidden);
            }

            existingReview.DeletedTime = vietnamTime;
            existingReview.DeletedBy = userId.ToString();

            await _unitOfWork.GetRepository<Review>().UpdateAsync(existingReview);
            await _unitOfWork.SaveAsync();

            // Update product rating
            await _productService.CalculateAverageRatingAsync(existingReview.ProductId);

            // Update shop rating
            var product = await _unitOfWork.GetRepository<Product>().GetByIdAsync(existingReview.ProductId);
            await _shopService.CalculateShopAverageRatingAsync(product.ShopId);

            return true;
        }
    }
}