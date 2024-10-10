using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.ModelViews.ProductDetailModelViews;
using HandmadeProductManagement.ModelViews.ProductModelViews;
using HandmadeProductManagement.ModelViews.PromotionModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        Task<ProductDetailResponseModel> GetProductDetailsByIdAsync(string productId);
        Task<decimal> CalculateAverageRatingAsync(string productId);
        Task<IList<ProductOverviewDto>> GetProductsByUserByPage(string userId, int pageNumber, int pageSize);
    }
}
