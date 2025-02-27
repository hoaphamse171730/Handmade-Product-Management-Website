﻿using HandmadeProductManagement.ModelViews.ReviewModelViews;

namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface IReviewService
    {
        Task<IList<ReviewModel>> GetBySellerIdAsync(string sellerId, int pageNumber, int pageSize);
        Task<IList<ReviewModel>> GetByUserIdAsync(string userId, int pageNumber, int pageSize);
        Task<IList<ReviewModel>> GetByProductIdAsync(string productId, int pageNumber, int pageSize);
        Task<IList<ReviewModel>> GetByPageAsync(int pageNumber, int pageSize);
        Task<ReviewModel> GetByIdAsync(string reviewId);
        Task<bool> CreateAsync(ReviewForCreationDto review, string userId);
        Task<bool> UpdateAsync(string reviewId, Guid userId, ReviewModel updatedReview);
        Task<bool> DeleteAsync(string reviewId, Guid userId);
        Task<bool> SoftDeleteAsync(string reviewId, Guid userId);
        Task<bool> RecoverDeletedReviewAsync(string reviewId, Guid userId);
        Task<IList<DeletedReviewModel>> GetAllDeletedReviewsAsync(Guid userId);
        Task<int> GetTotalPagesAsync(int pageSize);
    }
}
