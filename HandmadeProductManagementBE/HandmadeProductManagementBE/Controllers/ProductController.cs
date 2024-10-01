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
using System.ComponentModel.DataAnnotations;

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


        //[httpget("search")]
        //public async task<iactionresult> searchproducts(productsearchmodel searchmodel)
        //{
        //    var response = new baseresponse<productsearchvm>
        //    {
        //        code = ,
        //        statuscode = statuscodehelper.ok,
        //        message = "success",
        //        data = _productservice.searchproductsasync(searchmodel)
        //    };
        //    return ok(response);
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
        }

        [HttpGet("GetProductDetails/{id}")]
        public async Task<IActionResult> GetProductDetails([Required] string id)
        {
            try
            {
                var response = await _productService.GetProductDetailsByIdAsync(id);

                // Check the status code in the response from the service
                if (response.StatusCode == StatusCodeHelper.OK)
                {
                    return Ok(new BaseResponse<ProductDetailResponseModel>
                    {
                        Code = "Success",
                        StatusCode = response.StatusCode,
                        Message = "Product details retrieved successfully.",
                        Data = response.Data
                    });
                }
                else
                {
                    return StatusCode((int)response.StatusCode, new BaseResponse<ProductDetailResponseModel>
                    {
                        Code = response.Code,
                        StatusCode = response.StatusCode,
                        Message = response.Message,
                        Data = null
                    });
                }
            }
            catch (Exception)
            {
                var errorResponse = new BaseResponse<ProductDetailResponseModel>
                {
                    Code = "ServerError",
                    StatusCode = StatusCodeHelper.ServerError,
                    Message = "An unexpected error occurred.",
                    Data = null
                };
                return StatusCode(500, errorResponse);
            }
        }
    }
}
