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
        Task<IList<ReviewModel>> GetByPageAsync(int pageNumber, int pageSize);
        Task<ReviewModel?> GetByIdAsync(string reviewId);
        Task<bool> CreateAsync(ReviewModel review, string order);
        Task<bool> UpdateAsync(string reviewId, Guid userId, ReviewModel updatedReview);
        Task<bool> DeleteAsync(string reviewId, Guid userId);
        Task<bool> SoftDeleteAsync(string reviewId, Guid userId);
    }
}
