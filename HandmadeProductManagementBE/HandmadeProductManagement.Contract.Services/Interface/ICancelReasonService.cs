using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.ModelViews.CancelReasonModelViews;

namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface ICancelReasonService
    {
        Task<CancelReasonResponseModel> Create(CreateCancelReasonDto cancelReason);
        Task<CancelReasonResponseModel> Update(string id, CreateCancelReasonDto cancelReason);
        Task<bool> Delete(string id);
        Task<IList<CancelReasonResponseModel>> GetByPage(int page, int pageSize);
    }
}
