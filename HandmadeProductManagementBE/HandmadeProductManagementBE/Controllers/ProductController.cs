using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.ProductDetailModelViews;
using HandmadeProductManagement.ModelViews.ProductModelViews;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using HandmadeProductManagement.Contract.Repositories.Entity;

namespace HandmadeProductManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService) => _productService = productService;

        [HttpGet("user")]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> GetProductsByUser([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

            var response = new BaseResponse<IList<ProductOverviewDto>>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Products retrieved successfully",
                Data = await _productService.GetProductsByUserByPage(userId, pageNumber, pageSize)
            };
            return Ok(response);
        }

        [HttpPut("update-status/{id}")]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> UpdateStatusProduct(string id, [FromQuery] bool isAvailable = false)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var response = new BaseResponse<bool>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Product status updated successfully.",
                Data = await _productService.UpdateStatusProduct(id, isAvailable, userId)
            };

            return Ok(response);
        }



        [HttpGet("search")]
        public async Task<IActionResult> SearchProducts([FromQuery] ProductSearchFilter searchFilter, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var response = new BaseResponse<IEnumerable<ProductSearchVM>>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Search Product Successfully",
                Data = await _productService.SearchProductsAsync(searchFilter, pageNumber, pageSize)
            };
            return Ok(response);
        }

        [HttpGet("search-seller")]
        [Authorize]
        public async Task<IActionResult> SearchProductsBySeller([FromQuery] ProductSearchFilter searchFilter, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

            var response = new BaseResponse<IEnumerable<ProductSearchVM>>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Search Product Successfully",
                Data = await _productService.SearchProductsBySellerAsync(searchFilter, userId, pageNumber, pageSize)
            };
            return Ok(response);
        }

        [HttpGet("shop/{shopId}")]
        public async Task<IActionResult> GetProductById(string shopId, [FromQuery] ProductSearchFilter searchFilter, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var response = new BaseResponse<IEnumerable<ProductSearchVM>>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Search Product Successfully",
                Data = await _productService.GetProductByShopId(searchFilter, shopId, pageNumber, pageSize)
            };
            return Ok(response);
        }

        //[HttpGet("sort")]
        //[Authorize]
        //public async Task<IActionResult> SortProducts([FromQuery] ProductSortFilter sortModel)
        //{
        //    var products = await _productService.SortProductsAsync(sortModel);
        //    var response = new BaseResponse<IEnumerable<ProductSearchVM>>
        //    {
        //        Code = "200",
        //        StatusCode = StatusCodeHelper.OK,
        //        Message = "Sort Product Successfully",
        //        Data = products
        //    };
        //    return Ok(response);
        //}

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(string id)
        {
            var response = new BaseResponse<ProductDto>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Product retrieved successfully",
                Data = await _productService.GetById(id)
            };
            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> CreateProduct(ProductForCreationDto productForCreation)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

            var response = new BaseResponse<bool>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Product created successfully",
                Data = await _productService.Create(productForCreation, userId)
            };

            return Ok(response);
        }

        [HttpPatch("{id}")]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> UpdateProduct(string id, [FromBody] ProductForUpdateDto product)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var response = new BaseResponse<bool>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Product updated successfully.",
                Data = await _productService.Update(id, product, userId)
            };

            return Ok(response);
        }


        [HttpDelete("soft-delete/{id}")]
        [Authorize(Roles = "Admin, Seller")] 
        public async Task<IActionResult> SoftDeleteProduct(string id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var response = new BaseResponse<bool>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Product soft-deleted successfully",
                Data = await _productService.SoftDelete(id, userId)
            };
            return Ok(response);
        }

        [HttpGet("all-deleted-products")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllDeletedProducts([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var response = new BaseResponse<IList<Product>>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "All deleted products retrieved successfully",
                Data = await _productService.GetAllDeletedProducts(pageNumber, pageSize)
            };

            return Ok(response);
        }

        [HttpPut("{id}/recover")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RecoverProduct(string id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

            var response = new BaseResponse<bool>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Product recovered successfully",
                Data = await _productService.RecoverProduct(id, userId)
            };

            return Ok(response);
        }

        [HttpGet("detail/{id}")]
        public async Task<IActionResult> GetProductDetails([Required] string id)
        {
            var response = new BaseResponse<ProductDetailResponseModel>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Product details retrieved successfully.",
                Data = await _productService.GetProductDetailsByIdAsync(id)
            };
            return Ok(response);
        }

        [HttpGet("CalculateAverageRating/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CalculateAverageRating([Required] string id)
        {
            var response = new BaseResponse<decimal>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Average rating calculated successfully.",
                Data = await _productService.CalculateAverageRatingAsync(id)
            };
            return Ok(response);
        }
    }
}
