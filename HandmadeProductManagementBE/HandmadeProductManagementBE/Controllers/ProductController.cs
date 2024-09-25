using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.ProductModelViews;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
            if (response.StatusCode == StatusCodeHelper.OK)
            {
                return Ok(response);
            }

            return StatusCode((int)response.StatusCode, response);

        }


    }
}
