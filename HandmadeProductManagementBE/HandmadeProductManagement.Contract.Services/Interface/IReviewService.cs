using HandmadeProductManagement.ModelViews.ReviewModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface IReviewService
    {
        Task<IList<ReviewModel>> GetAllAsync(int pageNumber, int pageSize);
        Task<ReviewModel?> GetByIdAsync(string reviewId);
        Task<ReviewModel> CreateAsync(ReviewModel review, string order);
        Task<ReviewModel> UpdateAsync(string reviewId, Guid userId, ReviewModel updatedReview);
        Task<bool> DeleteAsync(string reviewId, Guid userId);
        Task<bool> SoftDeleteAsync(string reviewId, Guid userId);
    }
}
