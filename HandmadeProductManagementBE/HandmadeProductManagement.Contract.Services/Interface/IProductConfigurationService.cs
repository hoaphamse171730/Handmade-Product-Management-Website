using HandmadeProductManagement.ModelViews.ProductConfigurationModelViews;

namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface IProductConfigurationService
    {
        Task<bool> Create(ProductConfigurationForCreationDto productConfigurationDto, string userId);
        Task<bool> Delete(string productItemId, string variationOptionId);
    }
}
