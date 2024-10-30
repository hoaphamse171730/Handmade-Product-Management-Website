using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.ModelViews.ProductDetailModelViews;
using HandmadeProductManagement.ModelViews.ProductModelViews;
using HandmadeProductManagement.ModelViews.VariationCombinationModelViews;

namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface IProductService
    {
        Task<IEnumerable<ProductSearchVM>> SearchProductsAsync(ProductSearchFilter searchFilter, int pageNumber, int pageSize);
        Task<IEnumerable<ProductSearchVM>> GetProductByShopId(ProductSearchFilter searchFilter,string userId, int pageNumber, int pageSize);
        //Task<IEnumerable<ProductSearchVM>> SortProductsAsync(ProductSortFilter sortFilter);
        Task AddVariationOptionsToProduct(Product product, List<VariationCombinationDto> variationCombinations, string userId);
        Task<ProductDto> GetById(string id);
        Task<bool> Create(ProductForCreationDto product, string userId);
        Task<bool> Update(string id, ProductForUpdateDto product, string userId);
        Task<bool> SoftDelete(string id, string userId);
        Task<IList<Product>> GetAllDeletedProducts(int pageNumber, int pageSize);
        Task<IEnumerable<ProductSearchVM>> SearchProductsBySellerAsync(ProductSearchFilter searchFilter, string userId, int pageNumber, int pageSize);
        Task<bool> RecoverProduct(string id, string userId);
        Task<ProductDetailResponseModel> GetProductDetailsByIdAsync(string productId);
        Task<decimal> CalculateAverageRatingAsync(string productId);
        Task<IList<ProductOverviewDto>> GetProductsByUserByPage(string userId, int pageNumber, int pageSize);
        Task<bool> UpdateStatusProduct(string productId, bool isAvailable, string userId);

        public Task UpdateProductSoldCountAsync(string orderId);
    }
}
