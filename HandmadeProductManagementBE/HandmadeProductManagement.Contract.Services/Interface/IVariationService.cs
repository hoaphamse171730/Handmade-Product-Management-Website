using HandmadeProductManagement.ModelViews.VariationModelViews;

namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface IVariationService
    {
        Task<IList<VariationDto>> GetByCategoryId(string id);
        Task<IList<VariationDto>> GetAll();
        Task<bool> Create(VariationForCreationDto cancelReason, string userId);
        Task<bool> Update(string id, VariationForUpdateDto cancelReason, string username);
        Task<bool> Delete(string id, string userId);
    }
}
