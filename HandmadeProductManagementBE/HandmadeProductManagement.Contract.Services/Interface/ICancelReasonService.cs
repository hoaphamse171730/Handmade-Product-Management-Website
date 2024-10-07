using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.ModelViews.CancelReasonModelViews;

namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface ICancelReasonService
    {
        Task<IList<CancelReasonDto>> GetAll();
        Task<bool> Create(CancelReasonForCreationDto cancelReason, string username);
        Task<bool> Update(string id, CancelReasonForUpdateDto cancelReason, string username);
        Task<bool> Delete(string id, string username);
    }
}
