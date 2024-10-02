using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Utils;
using HandmadeProductManagement.ModelViews.ReplyModelViews;
using HandmadeProductManagement.ModelViews.ReviewModelViews;
using HandmadeProductManagement.Repositories.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandmadeProductManagement.Services.Service
{
    public class ReviewService : IReviewService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReviewService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IList<ReviewModel>> GetAllAsync(int pageNumber, int pageSize)
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
                                          .FirstOrDefaultAsync(r => r.Id == reviewId);

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

        public async Task<ReviewModel> CreateAsync(ReviewModel reviewModel, string orderId)
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

            // Check if the order exists, is "Shipped", and contains the product
            var order = await _unitOfWork.GetRepository<Order>()
                                          .Entities
                                          .Include(o => o.OrderDetails)
                                          .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == reviewModel.UserId);

            if (order == null || order.Status != "Shipped")
            {
                throw new BaseException.BadRequestException("order_not_found", "Review can only be created if the order is 'Shipped'.");
            }

            // Check if the order contains the specific product
            if (!order.OrderDetails.Any(od => od.ProductId == reviewModel.ProductId))
            {
                throw new BaseException.BadRequestException("product_not_in_order", "User can only review products that have been purchased in a 'Shipped' order.");
            }

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
                                         .Include(u => u.UserInfo)
                                         .FirstOrDefaultAsync(u => u.Id == reviewModel.UserId);

            if (user == null)
            {
                throw new BaseException.NotFoundException("user_not_found", "User not found.");
            }

            var userFullName = user.UserInfo.FullName;

            var review = new Review
            {
                Content = reviewModel.Content,
                Rating = reviewModel.Rating,
                Date = DateTime.UtcNow,
                ProductId = reviewModel.ProductId,
                UserId = reviewModel.UserId,
                CreatedBy = userFullName,
                LastUpdatedBy = userFullName
            };

            await _unitOfWork.GetRepository<Review>().InsertAsync(review);
            await _unitOfWork.SaveAsync();

            // Populate the reviewModel with the created review's properties
            reviewModel.Id = review.Id;
            reviewModel.Date = review.Date;

            return reviewModel;
        }

        public async Task<ReviewModel> UpdateAsync(string reviewId, Guid userId, ReviewModel updatedReview)
        {
            if (string.IsNullOrWhiteSpace(reviewId) || !Guid.TryParse(reviewId, out _))
            {
                throw new BaseException.BadRequestException("invalid_review_id_format", "Invalid reviewId format.");
            }

            var existingReview = await _unitOfWork.GetRepository<Review>()
                                                  .Entities
                                                  .FirstOrDefaultAsync(r => r.Id == reviewId && r.UserId == userId);

            if (existingReview == null)
            {
                throw new BaseException.NotFoundException("review_not_found", "Review not found or user does not have permission to update.");
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

            existingReview.LastUpdatedBy = existingReview.User.UserInfo.FullName;
            existingReview.LastUpdatedTime = DateTime.UtcNow;

            _unitOfWork.GetRepository<Review>().Update(existingReview);
            await _unitOfWork.SaveAsync();

            updatedReview.Content = existingReview.Content;
            updatedReview.Rating = existingReview.Rating;

            return updatedReview;
        }

        public async Task<bool> DeleteAsync(string reviewId, Guid userId)
        {
            if (string.IsNullOrWhiteSpace(reviewId) || !Guid.TryParse(reviewId, out _))
            {
                throw new BaseException.BadRequestException("invalid_review_id_format", "Invalid reviewId format.");
            }

            var existingReview = await _unitOfWork.GetRepository<Review>()
                                                  .Entities
                                                  .FirstOrDefaultAsync(r => r.Id == reviewId && r.UserId == userId);

            if (existingReview == null)
            {
                return false; // Return false if the review is not found or user is unauthorized to delete it.
            }

            existingReview.DeletedTime = DateTime.UtcNow;

            _unitOfWork.GetRepository<Review>().Delete(existingReview);
            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<bool> SoftDeleteAsync(string reviewId, Guid userId)
        {
            if (string.IsNullOrWhiteSpace(reviewId) || !Guid.TryParse(reviewId, out _))
            {
                throw new BaseException.BadRequestException("invalid_review_id_format", "Invalid reviewId format.");
            }

            var existingReview = await _unitOfWork.GetRepository<Review>()
                                                  .Entities
                                                  .FirstOrDefaultAsync(r => r.Id == reviewId && r.UserId == userId);

            if (existingReview == null)
            {
                return false; // Return false if the review is not found or user is unauthorized to delete it.
            }

            existingReview.DeletedTime = CoreHelper.SystemTimeNow;
            existingReview.DeletedBy = existingReview.User.UserInfo.FullName;

            _unitOfWork.GetRepository<Review>().Update(existingReview);
            await _unitOfWork.SaveAsync();

            return true;
        }
    }
}