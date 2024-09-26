using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.ModelViews.ProductModelViews;

namespace HandmadeProductManagement.Services.Service;

public class ProductService : IProductService
{
    public Task<BaseResponse<IEnumerable<ProductResponseModel>>> SearchProductsAsync(ProductSearchModel searchModel)
    {
        throw new NotImplementedException();
    }

    public Task<BaseResponse<IEnumerable<ProductResponseModel>>> SortProductsAsync(ProductSortModel sortModel)
    {
        throw new NotImplementedException();
    }
}