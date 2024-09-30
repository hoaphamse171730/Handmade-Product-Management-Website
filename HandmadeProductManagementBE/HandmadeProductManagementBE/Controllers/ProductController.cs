using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Utils;
using HandmadeProductManagement.ModelViews.ProductDetailModelViews;
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

        public ProductController(IProductService productService) => _productService = productService;

        [HttpGet("search")]
        public async Task<IActionResult> SearchProducts([FromQuery] ProductSearchModel searchModel)
        {
            var response = await _productService.SearchProductsAsync(searchModel);
            if (response.Data.IsNullOrEmpty())
            {
                return StatusCode(404, new BaseResponse<Product>(StatusCodeHelper.NotFound, StatusCodeHelper.NotFound.Name(), "Product Not Found!"));
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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
        {
            try
            {
                IList<ProductDto> products = await _productService.GetAll();
                return Ok(BaseResponse<IList<ProductDto>>.OkResponse(products));
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, BaseResponse<string>.FailResponse(ex.Message));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProduct(string id)
        {
            try
            {
                ProductDto product = await _productService.GetById(id);
                return Ok(BaseResponse<ProductDto>.OkResponse(product));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(BaseResponse<string>.FailResponse("Product not found"));
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, BaseResponse<string>.FailResponse(ex.Message));
            }
        }

        [HttpPost]
        public async Task<ActionResult<ProductDto>> CreateProduct(ProductForCreationDto productForCreation)
        {
            try
            {
                ProductDto createdProduct = await _productService.Create(productForCreation);
                return Ok(BaseResponse<ProductDto>.OkResponse(createdProduct));
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, BaseResponse<string>.FailResponse(ex.Message));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateProduct(string id, ProductForUpdateDto productForUpdate)
        {
            try
            {
                await _productService.Update(id, productForUpdate);
                return Ok(BaseResponse<string>.OkResponse("Product updated successfully"));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(BaseResponse<string>.FailResponse("Product not found"));
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, BaseResponse<string>.FailResponse(ex.Message));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProduct(string id)
        {
            try
            {
                await _productService.Delete(id);
                return Ok(new BaseResponse<bool>(StatusCodeHelper.OK, "Product deleted successfully.", true));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(BaseResponse<string>.FailResponse("Product not found"));
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, BaseResponse<string>.FailResponse(ex.Message));
            }
        }

        [HttpDelete("soft-delete/{id}")]
        public async Task<ActionResult> SoftDeleteProduct(string id)
        {
            try
            {
                await _productService.SoftDelete(id);
                return Ok(BaseResponse<string>.OkResponse("Product soft-deleted successfully"));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(BaseResponse<string>.FailResponse("Product not found"));
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, BaseResponse<string>.FailResponse(ex.Message));
            }
        [HttpGet("{GetProductDetaills/id}")]
        public async Task<IActionResult> GetProductDetails(string id)
        {
            var response = await _productService.GetProductDetailsByIdAsync(id);
            if (response.Data == null)
            {
                return StatusCode(404, new BaseResponse<ProductDetailResponseModel>(StatusCodeHelper.NotFound, StatusCodeHelper.NotFound.Name(), "Product Not Found!"));
            }
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
