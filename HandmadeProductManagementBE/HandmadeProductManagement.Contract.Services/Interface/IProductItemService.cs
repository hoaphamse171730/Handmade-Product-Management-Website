using HandmadeProductManagement.ModelViews.ProductItemModelViews;

namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface IProductItemService
    {
        Task<ProductItemDto> GetByProductId(string productId);
        Task<bool> Create(ProductItemForCreationDto productItemDto, string userId);
        Task<bool> Update(string id, ProductItemForUpdateDto productItemDto, string userId);
        Task<bool> Delete(string id, string userId);
    }
}
