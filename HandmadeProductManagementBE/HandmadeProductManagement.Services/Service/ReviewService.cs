using Firebase.Auth;
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
                Reply = r.Reply != null && r.Reply.DeletedTime == null ? new ReplyModel
                {
                    Id = r.Reply.Id,
                    Content = r.Reply.Content,
                    Date = r.Reply.Date,
                    ReviewId = r.Reply.ReviewId,
                    ShopId = r.Reply.ShopId
                } : null
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
                Reply = r.Reply != null && r.Reply.DeletedTime == null ? new ReplyModel
                {
                    Id = r.Reply!.Id,
                    Content = r.Reply.Content,
                    Date = r.Reply.Date,
                    ReviewId = r.Reply.ReviewId,
                    ShopId = r.Reply.ShopId
                } : null
            }).ToList();
        }

        public async Task<IList<ReviewModel>> GetBySellerIdAsync(string sellerId, int pageNumber, int pageSize)
        {
            if (string.IsNullOrWhiteSpace(sellerId) || !Guid.TryParse(sellerId, out _))
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

            // Lấy các sản phẩm được tạo bởi người bán
            var productsBySeller = await _unitOfWork.GetRepository<Product>()
                                                     .Entities
                                                     .Where(p => p.CreatedBy == sellerId && (!p.DeletedTime.HasValue || p.DeletedBy == null))
                                                     .Select(p => p.Id)
                                                     .ToListAsync();

            // Lấy các đánh giá của sản phẩm của người bán với phân trang
            var reviews = await _unitOfWork.GetRepository<Review>()
                                           .Entities
                                           .Include(r => r.Reply)
                                           .Where(r => productsBySeller.Contains(r.ProductId) && (!r.DeletedTime.HasValue || r.DeletedBy == null))
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
                Reply = r.Reply != null && r.Reply.DeletedTime == null ? new ReplyModel
                {
                    Id = r.Reply.Id,
                    Content = r.Reply.Content,
                    Date = r.Reply.Date,
                    ReviewId = r.Reply.ReviewId,
                    ShopId = r.Reply.ShopId
                } : null
            }).ToList();
        }

        public async Task<IList<ReviewModel>> GetByUserIdAsync(string userId, int pageNumber, int pageSize)
        {
            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out _))
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

            // Get the reviews for the specific user
            var reviews = await _unitOfWork.GetRepository<Review>()
                                           .Entities
                                           .Include(r => r.Reply)
                                           .Where(r => r.UserId.ToString() == userId && (!r.DeletedTime.HasValue || r.DeletedBy == null)) // Filter by UserId
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
                Reply = r.Reply != null && r.Reply.DeletedTime == null ? new ReplyModel
                {
                    Id = r.Reply.Id,
                    Content = r.Reply.Content,
                    Date = r.Reply.Date,
                    ReviewId = r.Reply.ReviewId,
                    ShopId = r.Reply.ShopId
                } : null
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
                Reply = review.Reply != null && review.Reply.DeletedTime == null ? new ReplyModel
                {
                    Id = review.Reply!.Id,
                    Content = review.Reply.Content,
                    Date = review.Reply.Date,
                    ReviewId = review.Reply.ReviewId,
                    ShopId = review.Reply.ShopId
                } : null
            };
        }

        public async Task<bool> CreateAsync(ReviewForCreationDto reviewCreate, string userId)
        {
            var reviewModel = reviewCreate;
            if (string.IsNullOrWhiteSpace(reviewModel.ProductId) || !Guid.TryParse(reviewModel.ProductId, out _))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidGuidFormat);
            }

            // Check if the productId exists in the database
            var productExists = await _unitOfWork.GetRepository<Product>().GetByIdAsync(reviewModel.ProductId)
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageProductNotFound);

            if (string.IsNullOrWhiteSpace(reviewModel.OrderId) || !Guid.TryParse(reviewModel.OrderId, out _))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidGuidFormat);
            }

            // Check if the order exists
            var order = await _unitOfWork.GetRepository<Order>()
                                         .Entities
                                         .Include(o => o.OrderDetails)
                                         .FirstOrDefaultAsync(o => o.Id == reviewModel.OrderId)
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageOrderNotFound);

            // Check if the user owns the order
            if (order.UserId != Guid.Parse(userId))
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
                                                  .FirstOrDefaultAsync(r => r.ProductId == reviewModel.ProductId && r.UserId == Guid.Parse(userId));
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
                                         .FirstOrDefaultAsync(u => u.Id == Guid.Parse(userId))
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageUserNotFound);

            var review = new Review
            {
                Content = reviewModel.Content,
                Rating = reviewModel.Rating,
                Date = vietnamTime,
                ProductId = reviewModel.ProductId,
                UserId = Guid.Parse(userId),
                CreatedBy = userId,
                LastUpdatedBy = userId
            };

            await _unitOfWork.GetRepository<Review>().InsertAsync(review);
            await _unitOfWork.SaveAsync();

            // Update product rating
            await _productService.CalculateAverageRatingAsync(reviewModel.ProductId);

            // Update shop rating
            var product = await _unitOfWork.GetRepository<Product>().GetByIdAsync(reviewModel.ProductId);
            if (product != null)
            {
                await _shopService.CalculateShopAverageRatingAsync(product.ShopId);
            }

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
            if (updatedReview.Rating < 1 || updatedReview.Rating > 5)
            {
                    throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidRating);
            }
            existingReview.Rating = updatedReview.Rating;

            if (existingReview.DeletedTime != null)
            {
                throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageReviewNotFoundSoftDeleted);
            }

            existingReview.LastUpdatedBy = existingReview.UserId.ToString();
            existingReview.LastUpdatedTime = vietnamTime;

            try
            {
                await _unitOfWork.GetRepository<Review>()
                                 .UpdateAsync(existingReview);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                // Log the exception here
                throw new BaseException.BadRequestException(StatusCodeHelper.ServerError.ToString(), $"Failed to save review: {ex.Message}");
            }

            // Update product rating
            await _productService.CalculateAverageRatingAsync(existingReview.ProductId);

            // Update shop rating
            var product = await _unitOfWork.GetRepository<Product>().GetByIdAsync(existingReview.ProductId);
            if (product != null)
            {
                await _shopService.CalculateShopAverageRatingAsync(product.ShopId);
            }
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

            // Get all associated replies
            var associatedReplies = await _unitOfWork.GetRepository<Reply>()
                .Entities
                .Where(r => r.ReviewId == reviewId)
                .ToListAsync();

            // Delete all associated replies
            foreach (var reply in associatedReplies)
            {
                await _unitOfWork.GetRepository<Reply>().DeleteAsync(reply.Id);
            }

            existingReview.DeletedBy = userId.ToString();

            await _unitOfWork.GetRepository<Review>().DeleteAsync(existingReview.Id);
            await _unitOfWork.SaveAsync();

            // Update product rating
            await _productService.CalculateAverageRatingAsync(existingReview.ProductId);

            // Update shop rating
            var product = await _unitOfWork.GetRepository<Product>().GetByIdAsync(existingReview.ProductId);
            if(product!=null)
            {
                await _shopService.CalculateShopAverageRatingAsync(product.ShopId);
            }

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

            // Get all associated replies that aren't already soft-deleted
            var associatedReplies = await _unitOfWork.GetRepository<Reply>()
                .Entities
                .Where(r => r.ReviewId == reviewId && r.DeletedTime == null)
                .ToListAsync();

            // Soft delete all associated replies
            foreach (var reply in associatedReplies)
            {
                reply.DeletedTime = vietnamTime;
                reply.DeletedBy = reply.ShopId;
                await _unitOfWork.GetRepository<Reply>().UpdateAsync(reply);
            }

            existingReview.DeletedTime = vietnamTime;
            existingReview.DeletedBy = userId.ToString();

            await _unitOfWork.GetRepository<Review>().UpdateAsync(existingReview);
            await _unitOfWork.SaveAsync();

            // Update product rating
            await _productService.CalculateAverageRatingAsync(existingReview.ProductId);

            // Update shop rating
            var product = await _unitOfWork.GetRepository<Product>().GetByIdAsync(existingReview.ProductId);
            if (product != null)
            {
                await _shopService.CalculateShopAverageRatingAsync(product.ShopId);
            }

            return true;
        }

        public async Task<bool> RecoverDeletedReviewAsync(string reviewId, Guid userId)
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

            // Check if the user has permission to recover the review
            if (existingReview.UserId != userId)
            {
                throw new BaseException.ForbiddenException(StatusCodeHelper.Forbidden.ToString(), Constants.ErrorMessageForbidden);
            }

            // Check if the review is actually soft-deleted
            if (existingReview.DeletedTime == null)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageDeletedReviews);
            }

            // Check if the associated product still exists and is not deleted
            var product = await _unitOfWork.GetRepository<Product>().GetByIdAsync(existingReview.ProductId)
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageProductNotFound);

            if (product.DeletedTime != null)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageProductSoftDeleted);
            }

            // Recover the review
            existingReview.DeletedTime = null;
            existingReview.DeletedBy = null;
            existingReview.LastUpdatedTime = vietnamTime;
            existingReview.LastUpdatedBy = userId.ToString();

            await _unitOfWork.GetRepository<Review>().UpdateAsync(existingReview);

            // Check if there's an associated reply to recover
            var reply = await _unitOfWork.GetRepository<Reply>()
                                         .Entities
                                         .FirstOrDefaultAsync(r => r.ReviewId == reviewId && r.DeletedTime != null);

            if (reply != null)
            {
                // Recover the reply
                reply.DeletedTime = null;
                reply.DeletedBy = null;
                reply.LastUpdatedTime = vietnamTime;
                reply.LastUpdatedBy = reply.ShopId;

                await _unitOfWork.GetRepository<Reply>().UpdateAsync(reply);
            }

            await _unitOfWork.SaveAsync();

            // Update product rating
            await _productService.CalculateAverageRatingAsync(existingReview.ProductId);

            // Update shop rating
            if (product != null)
            {
                await _shopService.CalculateShopAverageRatingAsync(product.ShopId);
            }

            return true;
        }

        public async Task<IList<DeletedReviewModel>> GetAllDeletedReviewsAsync(Guid userId)
        {
            var deletedReviews = await _unitOfWork.GetRepository<Review>()
                                            .Entities
                                            .Where(r => r.DeletedTime != null)
                                            .Select(r => new DeletedReviewModel
                                            {
                                                Id = r.Id,
                                                DeletedTime = r.DeletedTime!.Value,
                                                Content = r.Content,
                                                Rating = r.Rating,
                                                ProductId = r.ProductId,
                                                UserId = r.UserId
                                            })
                                            .ToListAsync();
            return deletedReviews;
        }

        public async Task<int> GetTotalPagesAsync(int pageSize)
        {
            // Validate page size
            if (pageSize <= 0)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidPageSize);
            }

            // Get the total count of reviews
            var totalReviewsCount = await _unitOfWork.GetRepository<Review>()
                                                     .Entities
                                                     .CountAsync();

            // Calculate total pages
            var totalPages = (int)Math.Ceiling(totalReviewsCount / (double)pageSize);

            return totalPages;
        }
    }
}