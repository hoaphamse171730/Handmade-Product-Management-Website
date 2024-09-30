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
        Task<ReviewModel> CreateAsync(ReviewModel review);
        Task<ReviewModel> UpdateAsync(string reviewId, ReviewModel updatedReview);
        Task<bool> DeleteAsync(string reviewId);
    }
}
