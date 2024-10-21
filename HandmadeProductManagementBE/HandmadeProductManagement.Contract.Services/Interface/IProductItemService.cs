using HandmadeProductManagement.ModelViews.ProductItemModelViews;
using HandmadeProductManagement.ModelViews.VariationCombinationModelViews;

namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface IProductItemService
    {
        Task<ProductItemDto> GetByProductId(string productId);
        Task<bool> AddVariationOptionsToProduct(string productId, List<VariationCombinationDto> variationCombinations, string userId);
        Task<bool> Update(string id, ProductItemForUpdateDto productItemDto, string userId);
        Task<bool> Delete(string id, string userId);
        Task<bool> Restore(string id, string userId);
        Task<List<ProductItemDto>> GetAllDeletedAsync();
    }
}
