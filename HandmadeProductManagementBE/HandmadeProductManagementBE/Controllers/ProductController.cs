using Azure;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Utils;
using HandmadeProductManagement.ModelViews.ProductDetailModelViews;
using HandmadeProductManagement.ModelViews.ProductModelViews;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Runtime.InteropServices;

namespace HandmadeProductManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService) => _productService = productService;

        [HttpGet("search")]
        public async Task<IActionResult> SearchProducts([FromQuery] ProductSearchFilter searchFilter)
        {
            var products = await _productService.SearchProductsAsync(searchFilter);
            var response = new BaseResponse<IEnumerable<ProductSearchVM>>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Search Product Successfully",
                Data = products
            };
            return Ok(response);
        }


        //[HttpGet("search")]
        //public async Task<IActionResult> SearchProducts(ProductSearchModel searchModel)
        //{
        //    var response = new BaseResponse<ProductSearchVM>
        //    {
        //        Code = ,
        //        StatusCode = StatusCodeHelper.OK,
        //        Message = "Success",
        //        Data = _productService.SearchProductsAsync(searchModel)
        //    };
        //    return Ok(response);
        //}

        [HttpGet("sort")]
        public async Task<IActionResult> SortProducts([FromQuery] ProductSortFilter sortModel)
        {
            var products = await _productService.SortProductsAsync(sortModel);
            var response = new BaseResponse<IEnumerable<ProductSearchVM>>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Sort Product Successfully",
                Data = products
            };
            return Ok(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _productService.GetAll();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(string id)
        {
            var product = await _productService.GetById(id);
            return Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct(ProductForCreationDto productForCreation)
        {
            var result = await _productService.Create(productForCreation);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(string id, ProductForUpdateDto productForUpdate)
        {
            var result = await _productService.Update(id, productForUpdate);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            var result = await _productService.Delete(id);
            return Ok(result);
        }

        [HttpDelete("soft-delete/{id}")]
        public async Task<IActionResult> SoftDeleteProduct(string id)
        {
            var result = await _productService.SoftDelete(id);
            return Ok(result);
        }

        [HttpGet("GetProductDetaills/{id}")]
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
