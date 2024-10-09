using HandmadeProductManagement.ModelViews.ProductConfigurationModelViews;

namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface IProductConfigurationService
    {
        Task<bool> Create(ProductConfigurationForCreationDto productConfigurationDto);
        Task<bool> Delete(string productItemId, string variationOptionId);
    }
}
