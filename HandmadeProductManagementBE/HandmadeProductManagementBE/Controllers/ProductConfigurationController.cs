using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HandmadeProductManagement.ModelViews.ProductConfigurationModelViews;
using HandmadeProductManagement.Core.Constants;
using Microsoft.EntityFrameworkCore;
using Firebase.Auth;

namespace HandmadeProductManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductConfigurationController : ControllerBase
    {
        private readonly IProductConfigurationService _productConfigurationService;

        public ProductConfigurationController(IProductConfigurationService productConfigurationService)
        {
            _productConfigurationService = productConfigurationService;
        }

        //[Authorize(Roles = "Admin")]
        //// POST: api/productconfiguration
        //[HttpPost]
        //public async Task<IActionResult> CreateProductConfiguration([FromBody] ProductConfigurationForCreationDto productConfigurationDto)
        //{
        //    var result = await _productConfigurationService.Create(productConfigurationDto);

        //    // Trả về phản hồi thành công
        //    var response = new BaseResponse<bool>
        //    {
        //        Code = "Success",
        //        StatusCode = StatusCodeHelper.OK,
        //        Message = "Created Product Configuration successfully!",
        //        Data = result
        //    };

        //    return Ok(response);
        //}

        //[Authorize(Roles = "Admin")]
        //// DELETE: api/productconfiguration/{productItemId}/{variationOptionId}
        //[HttpDelete("{productItemId}/{variationOptionId}")]
        //public async Task<IActionResult> DeleteProductConfiguration(string productItemId, string variationOptionId)
        //{
        //    var result = await _productConfigurationService.Delete(productItemId, variationOptionId);

        //    var response = new BaseResponse<string>
        //    {
        //        Code = "Success",
        //        StatusCode = StatusCodeHelper.OK,
        //        Message = $"Product Configuration has been successfully deleted.",
        //        Data = null
        //    };

        //    return Ok(response);
        //}

    }
}
