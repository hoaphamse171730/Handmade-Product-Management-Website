using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.ModelViews.VariationModelViews;

namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface IVariationService
    {
        Task<IList<VariationDto>> GetByCategoryId(string id);
        Task<bool> Create(VariationForCreationDto cancelReason, string userId);
        Task<bool> Update(string id, VariationForUpdateDto cancelReason, string username);
        Task<bool> Delete(string id, string userId);
        Task<IList<Variation>> GetDeleted();
        Task<bool> Recover(string id, string userId);
        Task<LatestVariationId> GetLatestVariationId(string categoryId, string userId);
    }
}
