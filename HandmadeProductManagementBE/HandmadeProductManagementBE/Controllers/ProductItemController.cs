using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.ProductItemModelViews;
using HandmadeProductManagement.ModelViews.VariationCombinationModelViews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HandmadeProductManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductItemController : ControllerBase
    {
        private readonly IProductItemService _productItemService;

        public ProductItemController(IProductItemService productItemService)
        {
            _productItemService = productItemService;
        }

        [Authorize(Roles = "Seller")]
        [HttpPost]
        [Route("{productId}/items")]
        public async Task<IActionResult> CreateProductItem(string productId, [FromBody] List<VariationCombinationDto> variationCombinations)
        {

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var response = new BaseResponse<bool>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Updated Product Item successfully!",
                Data = await _productItemService.AddVariationOptionsToProduct(productId, variationCombinations, userId)
            };
            return Ok(response);
        }

        [Authorize(Roles = "Seller")]
        // PATCH: api/ProductItem/{id}
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateProductItem(string id, [FromBody] ProductItemForUpdateDto updatedProductItemDto)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var response = new BaseResponse<bool>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Updated Product Item successfully!",
                Data = await _productItemService.Update(id, updatedProductItemDto, userId)
            };
            return Ok(response);
        }

        [Authorize(Roles = "Seller, Admin")]
        // DELETE: api/ProductItem/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDeleteProductItem(string id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

            var response = new BaseResponse<bool>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = $"Product Item with ID {id} has been successfully deleted.",
                Data = await _productItemService.Delete(id, userId)
            };
            return Ok(response);
        }

        [Authorize]
        // POST: api/ProductItem/restore/{id}
        [HttpPatch("restore/{id}")]
        public async Task<IActionResult> RestoreProductItem(string id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

            var response = new BaseResponse<bool>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = $"Product Item with ID {id} has been successfully restored.",
                Data = await _productItemService.Restore(id, userId)
            };
            return Ok(response);
        }

        [Authorize(Roles = "Seller, Admin")]
        // GET: api/ProductItem/deleted
        [HttpGet("deleted")]
        public async Task<IActionResult> GetAllDeletedProductItems()
        {
            var response = new BaseResponse<List<ProductItemDto>>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Retrieved all deleted product items successfully.",
                Data = await _productItemService.GetAllDeletedAsync()
            };
            return Ok(response);
        }
    }
}
