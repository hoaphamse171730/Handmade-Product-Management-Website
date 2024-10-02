using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.ModelViews.CancelReasonModelViews;

namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface ICancelReasonService
    {
        Task<CancelReasonResponseModel> GetById(string id);
        Task<IList<CancelReasonResponseModel>> GetByPage(int page, int pageSize);
        Task<bool> Create(CancelReasonForCreationDto cancelReason);
        Task<bool> Update(string id, CancelReasonForUpdateDto cancelReason);
        Task<bool> Delete(string id);
    }
}
