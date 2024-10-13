using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Utils;
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

        public ReviewService(IUnitOfWork unitOfWork, IProductService productService)
        {
            _unitOfWork = unitOfWork;
            _productService = productService;
        }
        public async Task<IList<ReviewModel>> GetByProductIdAsync(string productId, int pageNumber, int pageSize)
        {
            if (string.IsNullOrWhiteSpace(productId) || !Guid.TryParse(productId, out _))
            {
                throw new BaseException.BadRequestException("invalid_product_id_format", "Invalid productId format.");
            }
            if (pageNumber <= 0)
            {
                throw new BaseException.BadRequestException("invalid_page_number", "Page Number must be greater than zero.");
            }
            if (pageSize <= 0)
            {
                throw new BaseException.BadRequestException("invalid_page_size", "Page Size must be greater than zero.");
            }

            var productExists = await _unitOfWork.GetRepository<Product>().GetByIdAsync(productId);
            if (productExists == null)
            {
                throw new BaseException.NotFoundException("product_not_found", "Product not found.");
            }

            if (productExists.DeletedTime != null)
            {
                throw new BaseException.NotFoundException("product_not_found", "Product not found as it has been soft-delete.");
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
                throw new BaseException.BadRequestException("invalid_page_number", "Page Number must be greater than zero.");
            }
            if (pageSize <= 0)
            {
                throw new BaseException.BadRequestException("invalid_page_size", "Page Size must be greater than zero.");
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
                throw new BaseException.BadRequestException("invalid_review_id_format", "Invalid reviewId format.");
            }

            var review = await _unitOfWork.GetRepository<Review>()
                                          .Entities
                                          .Include(r => r.Reply)
                                          .FirstOrDefaultAsync(r => r.Id == reviewId && r.DeletedTime == null);

            if (review == null)
            {
                throw new BaseException.NotFoundException("review_not_found", "Review not found.");
            }

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
                throw new BaseException.BadRequestException("invalid_product_id_format", "Invalid productId format.");
            }

            // Check if the productId exists in the database
            var productExists = await _unitOfWork.GetRepository<Product>().GetByIdAsync(reviewModel.ProductId);
            if (productExists == null)
            {
                throw new BaseException.NotFoundException("product_not_found", "Product not found.");
            }


            if (string.IsNullOrWhiteSpace(orderId) || !Guid.TryParse(orderId, out _))
            {
                throw new BaseException.BadRequestException("invalid_order_id_format", "Invalid orderId format.");
            }

            // Check if the order exists
            var order = await _unitOfWork.GetRepository<Order>()
                                         .Entities
                                         .Include(o => o.OrderDetails)
                                         .FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
            {
                throw new BaseException.NotFoundException("order_not_found", "Order not found.");
            }

            // Check if the user owns the order
            if (order.UserId != reviewModel.UserId)
            {
                throw new BaseException.BadRequestException("unauthorized_order_access", "User does not own this order.");
            }

            // Check if the order status is "Shipped"
            if (order.Status != "Shipped")
            {
                throw new BaseException.BadRequestException("order_not_shipped", "Review can only be created if the order is 'Shipped'.");
            }

            // Check if the order contains the specific product
            //if (!order.OrderDetails.Any(od => od.ProductItemId == reviewModel.ProductId))
            //{
            //    throw new BaseException.BadRequestException("product_not_in_order", "User can only review products that have been purchased in a 'Shipped' order.");
            //}

            // Check if a review already exists for this product by this user
            var existingReview = await _unitOfWork.GetRepository<Review>()
                                                  .Entities
                                                  .FirstOrDefaultAsync(r => r.ProductId == reviewModel.ProductId && r.UserId == reviewModel.UserId);
            if (existingReview != null)
            {
                throw new BaseException.BadRequestException("review_already_exists", "User has already reviewed this product.");
            }

            if (reviewModel.Rating < 1 || reviewModel.Rating > 5)
            {
                throw new BaseException.BadRequestException("invalid_rating", "Invalid rating. It must be between 1 and 5.");
            }

            var user = await _unitOfWork.GetRepository<ApplicationUser>()
                                         .Entities
                                         //.Include(u => u.UserInfo)
                                         .FirstOrDefaultAsync(u => u.Id == reviewModel.UserId);

            if (user == null)
            {
                throw new BaseException.NotFoundException("user_not_found", "User not found.");
            }

            //var userFullName = user.UserInfo.FullName;

            var review = new Review
            {
                Content = reviewModel.Content,
                Rating = reviewModel.Rating,
                Date = DateTime.UtcNow,
                ProductId = reviewModel.ProductId,
                UserId = reviewModel.UserId,
                CreatedBy = reviewModel.UserId.ToString(),
                LastUpdatedBy = reviewModel.UserId.ToString()
            };

            await _unitOfWork.GetRepository<Review>().InsertAsync(review);
            await _unitOfWork.SaveAsync();

            await _productService.CalculateAverageRatingAsync(reviewModel.ProductId);

            return true;
        }

        public async Task<bool> UpdateAsync(string reviewId, Guid userId, ReviewModel updatedReview)
        {
            if (string.IsNullOrWhiteSpace(reviewId) || !Guid.TryParse(reviewId, out _))
            {
                throw new BaseException.BadRequestException("invalid_review_id_format", "Invalid reviewId format.");
            }

            // Check if the review exists
            var existingReview = await _unitOfWork.GetRepository<Review>()
                                                  .Entities
                                                  .Include(r => r.User)
                                                  .FirstOrDefaultAsync(r => r.Id == reviewId);
            if (existingReview == null)
            {
                throw new BaseException.NotFoundException("review_not_found", "Review not found.");
            }

            // Check if the user has permission to update the review
            if (existingReview.UserId != userId)
            {
                throw new BaseException.BadRequestException("unauthorized_review_update", "User does not have permission to update this review.");
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
                    throw new BaseException.BadRequestException("invalid_rating", "Rating must be between 1 and 5.");
                }
                existingReview.Rating = updatedReview.Rating;
            }

            if (existingReview.DeletedTime != null)
            {
                throw new BaseException.NotFoundException("review_not_found", "Cannot update the review which has been soft-delete.");
            }

            existingReview.LastUpdatedBy = existingReview.UserId.ToString();
            existingReview.LastUpdatedTime = DateTime.UtcNow;

            _unitOfWork.GetRepository<Review>().UpdateAsync(existingReview);
            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(string reviewId, Guid userId)
        {
            if (string.IsNullOrWhiteSpace(reviewId) || !Guid.TryParse(reviewId, out _))
            {
                throw new BaseException.BadRequestException("invalid_review_id_format", "Invalid reviewId format.");
            }

            // Check if the review exists
            var existingReview = await _unitOfWork.GetRepository<Review>()
                                                  .Entities
                                                  .FirstOrDefaultAsync(r => r.Id == reviewId);
            if (existingReview == null)
            {
                throw new BaseException.NotFoundException("review_not_found", "Review not found.");
            }

            // Check if the user has permission to delete the review
            if (existingReview.UserId != userId)
            {
                throw new BaseException.BadRequestException("unauthorized_review_delete", "User does not have permission to delete this review.");
            }

            if(existingReview.DeletedTime != null)
            {
                throw new BaseException.NotFoundException("review_not_found", "Review not found as it has been soft-delete.");
            }

            existingReview.DeletedBy = userId.ToString();

            await _unitOfWork.GetRepository<Review>().DeleteAsync(existingReview.Id);
            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<bool> SoftDeleteAsync(string reviewId, Guid userId)
        {
            if (string.IsNullOrWhiteSpace(reviewId) || !Guid.TryParse(reviewId, out _))
            {
                throw new BaseException.BadRequestException("invalid_review_id_format", "Invalid reviewId format.");
            }

            // Check if the review exists
            var existingReview = await _unitOfWork.GetRepository<Review>()
                                                  .Entities
                                                  .FirstOrDefaultAsync(r => r.Id == reviewId);
            if (existingReview == null)
            {
                throw new BaseException.NotFoundException("review_not_found", "Review not found.");
            }

            // Check if the user has permission to delete the review
            if (existingReview.UserId != userId)
            {
                throw new BaseException.BadRequestException("unauthorized_review_delete", "User does not have permission to delete this review.");
            }

            existingReview.DeletedTime = DateTime.UtcNow;
            existingReview.DeletedBy = userId.ToString();

            await _unitOfWork.GetRepository<Review>().UpdateAsync(existingReview);
            await _unitOfWork.SaveAsync();

            return true;
        }
    }
}