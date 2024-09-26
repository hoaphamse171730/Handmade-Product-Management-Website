using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.ModelViews.ReviewModelViews;
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
            var review = await _unitOfWork.GetRepository<Review>().GetByIdAsync(reviewId);
            if (review == null) return null;

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
            if (string.IsNullOrWhiteSpace(reviewModel.Content) || reviewModel.Rating < 1 || reviewModel.Rating > 5)
                throw new ArgumentException("Invalid review data.");

            var review = new Review
            {
                Content = reviewModel.Content,
                Rating = reviewModel.Rating,
                ProductId = reviewModel.ProductId,
                UserId = reviewModel.UserId
            };

            await _unitOfWork.GetRepository<Review>().InsertAsync(review);
            await _unitOfWork.SaveAsync();

            reviewModel.Id = review.Id; // set the generated ID
            return reviewModel;
        }

        public async Task<ReviewModel> UpdateAsync(string reviewId, ReviewModel updatedReview)
        {
            var existingReview = await _unitOfWork.GetRepository<Review>().GetByIdAsync(reviewId);
            if (existingReview == null) throw new ArgumentException("Review not found.");

            // Update Content only if it's provided
            if (!string.IsNullOrWhiteSpace(updatedReview.Content))
            {
                existingReview.Content = updatedReview.Content;
            }

            // Update Rating only if it's within valid range
            if (updatedReview.Rating >= 1 && updatedReview.Rating <= 5)
            {
                existingReview.Rating = updatedReview.Rating;
            }

            _unitOfWork.GetRepository<Review>().Update(existingReview);
            await _unitOfWork.SaveAsync();

            return updatedReview;
        }

        public async Task<bool> DeleteAsync(string reviewId)
        {
            var existingReview = await _unitOfWork.GetRepository<Review>().GetByIdAsync(reviewId);
            if (existingReview == null) return false;

            _unitOfWork.GetRepository<Review>().Delete(reviewId);
            await _unitOfWork.SaveAsync();
            return true;
        }
    }
}
