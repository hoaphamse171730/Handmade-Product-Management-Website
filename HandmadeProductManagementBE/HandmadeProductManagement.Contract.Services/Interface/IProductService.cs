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
        Task<IEnumerable<ProductSearchVM>> SearchProductsAsync(ProductSearchFilter searchFilter);
        Task<IEnumerable<ProductSearchVM>> SortProductsAsync(ProductSortFilter sortFilter);
        Task<IList<ProductDto>> GetAll();
        Task<ProductDto> GetById(string id);
        Task<ProductDto> Create(ProductForCreationDto product);
        Task<ProductDto> Update(string id, ProductForUpdateDto product);
        Task<bool> SoftDelete(string id);
        Task<ProductDetailResponseModel> GetProductDetailsByIdAsync(string productId);
        Task<decimal> CalculateAverageRatingAsync(string productId);
    }
}
