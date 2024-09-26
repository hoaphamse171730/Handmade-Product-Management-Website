using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.ModelViews.ProductModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HandmadeProductManagement.Contract.Repositories.Entity;

namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface IProductService
    {
        Task<BaseResponse<IEnumerable<ProductResponseModel>>> SearchProductsAsync(ProductSearchModel searchModel);
        Task<BaseResponse<IEnumerable<ProductResponseModel>>> SortProductsAsync(ProductSortModel sortModel);
        Task<IList<Product>> GetAll();
        Task<Product> GetById(string id);
        Task<Product> Create(Product product);
        Task<Product> Update(string id, Product product);
        Task<bool> Delete(string id);
    }
}