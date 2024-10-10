using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.ModelViews.ProductDetailModelViews;
using HandmadeProductManagement.ModelViews.ProductModelViews;

namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface IProductService
    {
        Task<IEnumerable<ProductSearchVM>> SearchProductsAsync(ProductSearchFilter searchFilter, int pageNumber, int pageSize);
        //Task<IEnumerable<ProductSearchVM>> SortProductsAsync(ProductSortFilter sortFilter);
        Task<ProductDto> GetById(string id);
        Task<bool> Create(ProductForCreationDto product, string userId);
        Task<bool> Update(string id, ProductForUpdateDto product, string userId);
        Task<bool> SoftDelete(string id, string userId);
        Task<IList<Product>> GetAllDeletedProducts(int pageNumber, int pageSize);
        Task<bool> RecoverProduct(string id, string userId);
        Task<ProductDetailResponseModel> GetProductDetailsByIdAsync(string productId);
        Task<decimal> CalculateAverageRatingAsync(string productId);
        Task<IList<ProductOverviewDto>> GetProductsByUserByPage(string userId, int pageNumber, int pageSize);
        Task<bool> UpdateStatusProduct(string productId, string newStatus, string userId);
    }
}
