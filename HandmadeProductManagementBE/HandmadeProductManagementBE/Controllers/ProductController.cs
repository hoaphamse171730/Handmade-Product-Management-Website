using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Utils;
using HandmadeProductManagement.ModelViews.ProductModelViews;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace HandmadeProductManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet("search")]
        public async Task<IActionResult> searchProducts([FromQuery] ProductSearchModel searchModel){
            
            var response = await _productService.SearchProductsAsync(searchModel);
            if (response.Data.IsNullOrEmpty())
            {
                return StatusCode(404,new BaseResponse<Product>(StatusCodeHelper.NotFound, StatusCodeHelper.NotFound.Name(), "Product Not Found!"));
            }
            return StatusCode((int)response.StatusCode, response);

        }

        [HttpGet("sort")]
        public async Task<IActionResult> SortProducts([FromQuery] ProductSortModel sortModel)
        {
            var response = await _productService.SortProductsAsync(sortModel);
            if (response.Data.IsNullOrEmpty())
            {
                return StatusCode(404, new BaseResponse<Product>(StatusCodeHelper.NotFound, StatusCodeHelper.NotFound.Name(), "Product Not Found!"));
            }
            return StatusCode((int)response.StatusCode, response);
        }


    }
}
