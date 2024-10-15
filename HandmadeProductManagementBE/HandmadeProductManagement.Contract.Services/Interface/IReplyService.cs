using HandmadeProductManagement.ModelViews.ReplyModelViews;

namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface IReplyService
    {
        Task<IList<ReplyModel>> GetByPageAsync(int pageNumber, int pageSize);
        Task<ReplyModel?> GetByIdAsync(string replyId);
        Task<bool> CreateAsync(ReplyModel reply, Guid userId);
        Task<bool> UpdateAsync(string replyId, Guid userId, ReplyModel updatedReply);
        Task<bool> DeleteAsync(string replyId, Guid userId);
        Task<bool> SoftDeleteAsync(string reviewId, Guid userId);
    }
}
