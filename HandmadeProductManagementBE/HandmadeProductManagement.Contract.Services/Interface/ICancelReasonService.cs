using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.ModelViews.CancelReasonModelViews;

namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface ICancelReasonService
    {
        Task<IList<CancelReasonDto>> GetAll();
        Task<bool> Create(CancelReasonForCreationDto cancelReason, string userId);
        Task<bool> Update(string id, CancelReasonForUpdateDto cancelReason, string userId);
        Task<bool> Delete(string id, string userId);
        Task<IList<CancelReason>> GetDeletedCancelReasons();
        Task<bool> PatchReverseDelete(string id, string userId);
    }
}
