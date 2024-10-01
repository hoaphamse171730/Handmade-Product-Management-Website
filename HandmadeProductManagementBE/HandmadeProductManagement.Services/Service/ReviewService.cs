using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Utils;
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
                throw new ArgumentException("Page Number must be greater than zero.");
            }
            if (pageSize <= 0)
            {
                throw new ArgumentException("Page Size must be greater than zero.");
            }

            var reviews = await _unitOfWork.GetRepository<Review>().GetAllAsync();
            return reviews.Skip((pageNumber - 1) * pageSize)
                          .Take(pageSize)
                          .Select(r => new ReviewModel
                          {
                              Id = r.Id,
                              Content = r.Content,
                              Rating = r.Rating,
                              Date = r.Date,
                              ProductId = r.ProductId,
                              UserId = r.UserId
                          }).ToList();
        }

        public async Task<ReviewModel?> GetByIdAsync(string reviewId)
        {
            if (string.IsNullOrWhiteSpace(reviewId) || !Guid.TryParse(reviewId, out _))
            {
                throw new ArgumentException("Invalid reviewId format.");
            }

            var review = await _unitOfWork.GetRepository<Review>().GetByIdAsync(reviewId);
            if (review == null)
            {
                throw new ArgumentException("Review not found.");
            }

            return new ReviewModel
            {
                Id = review.Id,
                Content = review.Content,
                Rating = review.Rating,
                Date = review.Date,
                ProductId = review.ProductId,
                UserId = review.UserId
            };
        }

        public async Task<ReviewModel> CreateAsync(ReviewModel reviewModel)
        {
            if (string.IsNullOrWhiteSpace(reviewModel.ProductId) || !Guid.TryParse(reviewModel.ProductId, out _))
            {
                throw new ArgumentException("Invalid productId format.");
            }

            // Check if the productId exists in the database
            var productExists = await _unitOfWork.GetRepository<Product>().GetByIdAsync(reviewModel.ProductId);
            if (productExists == null)
            {
                throw new ArgumentException("Product not found.");
            }

            if (reviewModel.Rating < 1 || reviewModel.Rating > 5)
            {
                throw new ArgumentException("Invalid rating. It must be between 1 and 5.");
            }

            var user = await _unitOfWork.GetRepository<ApplicationUser>()
                                         .Entities
                                         .Include(u => u.UserInfo)
                                         .FirstOrDefaultAsync(u => u.Id == reviewModel.UserId);

            if (user == null)
            {
                throw new ArgumentException("User not found.");
            }

            var userFullName = user.UserInfo.FullName;

            var review = new Review
            {
                Content = reviewModel.Content,
                Rating = reviewModel.Rating,
                Date = DateTime.UtcNow,
                ProductId = reviewModel.ProductId,
                UserId = reviewModel.UserId,
            };

            review.CreatedBy = userFullName;
            review.LastUpdatedBy = userFullName;

            await _unitOfWork.GetRepository<Review>().InsertAsync(review);
            await _unitOfWork.SaveAsync();

            // Populate the reviewModel with the created review's properties
            reviewModel.Id = review.Id;
            reviewModel.Date = review.Date;

            return reviewModel;
        }

        public async Task<ReviewModel> UpdateAsync(string reviewId, ReviewModel updatedReview)
        {
            if (string.IsNullOrWhiteSpace(reviewId) || !Guid.TryParse(reviewId, out _))
            {
                throw new ArgumentException("Invalid reviewId format.");
            }

            var existingReview = await _unitOfWork.GetRepository<Review>().GetByIdAsync(reviewId);
            if (existingReview == null)
            {
                throw new ArgumentException("Review not found.");
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
                    throw new ArgumentException("Rating must be between 1 and 5.");
                }
                existingReview.Rating = updatedReview.Rating;
            }

            var user = await _unitOfWork.GetRepository<ApplicationUser>()
                                .Entities
                                .Include(u => u.UserInfo)
                                .FirstOrDefaultAsync(u => u.Id == updatedReview.UserId);

            if (user == null)
            {
                throw new ArgumentException("User not found.");
            }

            var userFullName = user.UserInfo.FullName;

            existingReview.LastUpdatedBy = userFullName;
            existingReview.LastUpdatedTime = DateTimeOffset.UtcNow;

            _unitOfWork.GetRepository<Review>().Update(existingReview);
            await _unitOfWork.SaveAsync();

            updatedReview.Content = existingReview.Content;
            updatedReview.Rating = existingReview.Rating;

            return updatedReview;
        }

        public async Task<bool> DeleteAsync(string reviewId)
        {
            if (string.IsNullOrWhiteSpace(reviewId) || !Guid.TryParse(reviewId, out _))
            {
                throw new ArgumentException("Invalid reviewId format.");
            }

            var existingReview = await _unitOfWork.GetRepository<Review>().GetByIdAsync(reviewId);
            if (existingReview == null)
            {
                throw new ArgumentException("Review not found.");
            }

            existingReview.DeletedTime = DateTime.UtcNow;

            _unitOfWork.GetRepository<Review>().Delete(reviewId);
            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<bool> SoftDeleteAsync(string reviewId)
        {
            if (string.IsNullOrWhiteSpace(reviewId) || !Guid.TryParse(reviewId, out _))
            {
                throw new ArgumentException("Invalid reviewId format.");
            }

            var existingReview = await _unitOfWork.GetRepository<Review>().GetByIdAsync(reviewId);
            if (existingReview == null)
            {
                throw new ArgumentException("Review not found.");
            }

            var user = await _unitOfWork.GetRepository<ApplicationUser>()
                                .Entities
                                .Include(u => u.UserInfo)
                                .FirstOrDefaultAsync(u => u.Id == existingReview.UserId);

            if (user == null)
            {
                throw new ArgumentException("User not found.");
            }

            var userFullName = user.UserInfo.FullName;

            existingReview.DeletedTime = CoreHelper.SystemTimeNow;
            existingReview.DeletedBy = userFullName;

            _unitOfWork.GetRepository<Review>().Update(existingReview);
            await _unitOfWork.SaveAsync();

            return true;
        }
    }
}
