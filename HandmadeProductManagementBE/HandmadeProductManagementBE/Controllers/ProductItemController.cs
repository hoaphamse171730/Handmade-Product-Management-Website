using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.ProductItemModelViews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        // GET: api/ProductItem/{productId}
        //[HttpGet("{productId}")]
        //public async Task<IActionResult> GetByProductId(string productId)
        //{
        //    var productItem = await _productItemService.GetByProductId(productId);

        //    var response = new BaseResponse<ProductItemDto>
        //    {
        //        Code = "Success",
        //        StatusCode = StatusCodeHelper.OK,
        //        Message = "Get Product Item successfully!",
        //        Data = productItem
        //    };
        //    return Ok(response);
        //}

        //[Authorize(Roles = "Seller")]
        //// POST: api/ProductItem
        //[HttpPost]
        //public async Task<IActionResult> CreateProductItem([FromBody] ProductItemForCreationDto productItemDto)
        //{
        //    var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        //    var result = await _productItemService.Create(productItemDto, userId);

        //    var response = new BaseResponse<bool>
        //    {
        //        Code = "Success",
        //        StatusCode = StatusCodeHelper.OK,
        //        Message = "Created Product Item successfully!",
        //        Data = result
        //    };
        //    return Ok(response);
        //}

        //[Authorize(Roles = "Seller")]
        //// PUT: api/ProductItem/{id}
        //[HttpPut("{id}")]
        //public async Task<IActionResult> UpdateProductItem(string id, [FromBody] ProductItemForUpdateDto updatedProductItemDto)
        //{
        //    var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        //    var result = await _productItemService.Update(id, updatedProductItemDto, userId);

        //    var response = new BaseResponse<bool>
        //    {
        //        Code = "Success",
        //        StatusCode = StatusCodeHelper.OK,
        //        Message = "Updated Product Item successfully!",
        //        Data = result
        //    };
        //    return Ok(response);
        //}

        //[Authorize(Roles = "Seller, Admin")]
        //// DELETE: api/ProductItem/{id}
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> SoftDeleteProductItem(string id)
        //{
        //    var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        //    var result = await _productItemService.Delete(id, userId);

        //    var response = new BaseResponse<string>
        //    {
        //        Code = "Success",
        //        StatusCode = StatusCodeHelper.OK,
        //        Message = $"Product Item with ID {id} has been successfully deleted.",
        //        Data = null
        //    };
        //    return Ok(response);
        //}
    }
}
