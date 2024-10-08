using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HandmadeProductManagement.ModelViews.ProductConfigurationModelViews;
using HandmadeProductManagement.Core.Constants;
using Microsoft.EntityFrameworkCore;

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

        [Authorize(Roles = "Admin")]
        // POST: api/productconfiguration
        [HttpPost]
        public async Task<IActionResult> CreateProductConfiguration([FromBody] ProductConfigurationForCreationDto productConfigurationDto)
        {
            try
            {
                // Lấy userId từ claim
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                // Tạo ProductConfiguration
                var result = await _productConfigurationService.Create(productConfigurationDto, userId);

                // Trả về phản hồi thành công
                var response = new BaseResponse<bool>
                {
                    Code = "Success",
                    StatusCode = StatusCodeHelper.OK,
                    Message = "Created Product Configuration successfully!",
                    Data = result
                };

                return Ok(response);
            }
            catch (DbUpdateException dbEx)
            {
                // Xử lý lỗi khi xảy ra DbUpdateException
                return StatusCode(500, new BaseResponse<string>
                {
                    Code = "Error",
                    StatusCode = StatusCodeHelper.ServerError,
                    Message = "An error occurred while saving the entity changes.",
                    Data = dbEx.InnerException?.Message ?? dbEx.Message // Cung cấp thông tin chi tiết về lỗi nếu có
                });
            }
        }

        [Authorize(Roles = "Admin")]
        // DELETE: api/productconfiguration/{productItemId}/{variationOptionId}
        [HttpDelete("{productItemId}/{variationOptionId}")]
        public async Task<IActionResult> DeleteProductConfiguration(string productItemId, string variationOptionId)
        {
            var result = await _productConfigurationService.Delete(productItemId, variationOptionId);

            var response = new BaseResponse<string>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = $"Product Configuration with ProductItemId: {productItemId} and VariationOptionId: {variationOptionId} has been successfully deleted.",
                Data = null
            };

            return Ok(response);
        }

    }
}
