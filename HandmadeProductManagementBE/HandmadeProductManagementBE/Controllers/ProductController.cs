using Azure;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Utils;
using HandmadeProductManagement.ModelViews.ProductDetailModelViews;
using HandmadeProductManagement.ModelViews.ProductModelViews;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace HandmadeProductManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService) => _productService = productService;

        [HttpGet("search")]
        [Authorize]
        public async Task<IActionResult> SearchProducts([FromQuery] ProductSearchFilter searchFilter)
        {
            var products = await _productService.SearchProductsAsync(searchFilter);
            var response = new BaseResponse<IEnumerable<ProductSearchVM>>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Search Product Successfully",
                Data = products
            };
            return Ok(response);
        }

        [HttpGet("sort")]
        [Authorize]
        public async Task<IActionResult> SortProducts([FromQuery] ProductSortFilter sortModel)
        {
            var products = await _productService.SortProductsAsync(sortModel);
            var response = new BaseResponse<IEnumerable<ProductSearchVM>>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Sort Product Successfully",
                Data = products
            };
            return Ok(response);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _productService.GetAll();
            var response = new BaseResponse<IList<ProductDto>>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Products retrieved successfully",
                Data = products
            };
            return Ok(response);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetProduct(string id)
        {
            var product = await _productService.GetById(id);
            var response = new BaseResponse<ProductDto>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Product retrieved successfully",
                Data = product
            };
            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateProduct(ProductForCreationDto productForCreation)
        {
            var createdProduct = await _productService.Create(productForCreation);
            var response = new BaseResponse<ProductDto>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Product created successfully",
                Data = createdProduct
            };
            return Ok(response); 
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProduct(string id, ProductForUpdateDto productForUpdate)
        {
            await _productService.Update(id, productForUpdate);
            var response = new BaseResponse<string>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Product updated successfully",
                Data = "Product updated successfully"
            };
            return Ok(response);
        }

        [HttpDelete("soft-delete/{id}")]
        [Authorize(Roles = "Admin")] 
        public async Task<IActionResult> SoftDeleteProduct(string id)
        {
            await _productService.SoftDelete(id);
            var response = new BaseResponse<string>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Product soft-deleted successfully",
                Data = "Product soft-deleted successfully"
            };
            return Ok(response);
        }

        [HttpGet("GetProductDetails/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetProductDetails([Required] string id)
        {
            var productDetails = await _productService.GetProductDetailsByIdAsync(id);
            var response = new BaseResponse<ProductDetailResponseModel>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Product details retrieved successfully.",
                Data = productDetails
            };
            return Ok(response);
        }

        [HttpGet("CalculateAverageRating/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CalculateAverageRating([Required] string id)
        {
            var averageRating = await _productService.CalculateAverageRatingAsync(id);
            var response = new BaseResponse<decimal>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Average rating calculated successfully.",
                Data = averageRating
            };
            return Ok(response);
        }
    }
}
