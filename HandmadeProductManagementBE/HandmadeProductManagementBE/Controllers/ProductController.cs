using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.ProductDetailModelViews;
using HandmadeProductManagement.ModelViews.ProductModelViews;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace HandmadeProductManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService) => _productService = productService;

        [HttpGet("page")]
        public async Task<IActionResult> GetProducts([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var response = new BaseResponse<IList<ProductOverviewDto>>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Products retrieved successfully",
                Data = await _productService.GetByPage(pageNumber, pageSize)
            };
            return Ok(response);
        }

        [HttpGet("user")]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> GetProductsByUser([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var response = new BaseResponse<IList<ProductOverviewDto>>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Products retrieved successfully",
                Data = await _productService.GetProductsByUserByPage(userId, pageNumber, pageSize)
            };
            return Ok(response);
        }

        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetProductsByCategoryId(string categoryId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var response = new BaseResponse<IList<ProductOverviewDto>>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Products retrieved successfully.",
                Data = await _productService.GetProductByCategoryId(categoryId, pageNumber, pageSize)
            };

            return Ok(response);
        }

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

        [HttpGet("{id}")]
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
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> CreateProduct(ProductForCreationDto productForCreation)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var createdProduct = await _productService.Create(productForCreation, userId);

                var response = new BaseResponse<bool>
                {
                    Code = "200",
                    StatusCode = StatusCodeHelper.OK,
                    Message = "Product created successfully",
                    Data = createdProduct
                };

                return Ok(response);
            }
            catch (DbUpdateException ex)
            {
                // Log the error for further investigation (you can use any logging framework)
                // _logger.LogError(ex, "An error occurred while saving the product.");

                var response = new BaseResponse<string>
                {
                    Code = "500",
                    StatusCode = StatusCodeHelper.ServerError,
                    Message = "An error occurred while saving the product. Please try again later.",
                    Data = ex.InnerException?.Message ?? ex.Message // Include detailed error if available
                };

                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProduct(string id, ProductForUpdateDto productForUpdate)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var response = new BaseResponse<bool>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Product updated successfully",
                Data = await _productService.Update(id, productForUpdate, userId)
            };
            return Ok(response);
        }

        [HttpDelete("soft-delete/{id}")]
        [Authorize(Roles = "Admin")] 
        public async Task<IActionResult> SoftDeleteProduct(string id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            await _productService.SoftDelete(id, userId);
            var response = new BaseResponse<string>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Product soft-deleted successfully",
                Data = "Product soft-deleted successfully"
            };
            return Ok(response);
        }

        [HttpGet("detail/{id}")]
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
