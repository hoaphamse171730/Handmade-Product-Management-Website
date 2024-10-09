using HandmadeProductManagement.ModelViews.VariationOptionModelViews;

namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface IVariationOptionService
    {
        Task<IList<VariationOptionDto>> GetByPage(int page, int pageSize);
        Task<IList<VariationOptionDto>> GetByVariationId(string variationId);
        Task<bool> Create(VariationOptionForCreationDto option, string userId);
        Task<bool> Update(string id, VariationOptionForUpdateDto option, string userId);
        Task<bool> Delete(string id, string userId);
    }
}
