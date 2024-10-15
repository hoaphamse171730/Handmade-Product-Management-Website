using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Utils;
using HandmadeProductManagement.ModelViews.ShopModelViews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HandmadeProductManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShopController : ControllerBase
    {
        private readonly IShopService _shopService;

        public ShopController(IShopService shopService)
        {
            _shopService = shopService;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateShop([FromBody] CreateShopDto shop)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var createdShop = await _shopService.CreateShopAsync(userId, shop);
            var response = new BaseResponse<bool>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Shop created successfully",
                Data = createdShop
            };
            return Ok(response);
        }

        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> DeleteShop()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var result = await _shopService.DeleteShopAsync(userId);
            var response = new BaseResponse<bool>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Shop deleted successfully",
                Data = result
            };
            return Ok(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetShopsByPage([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var paginatedShops = await _shopService.GetShopsByPageAsync(pageNumber, pageSize);
            var response = new BaseResponse<PaginatedList<ShopResponseModel>>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Shops retrieved successfully",
                Data = paginatedShops
            };
            return Ok(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin/{userId}")]
        public async Task<IActionResult> GetShopByUserIdForAdmin(Guid userId)
        {
            var shop = await _shopService.GetShopByUserIdAsync(userId);
            var response = new BaseResponse<ShopResponseModel>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Shop retrieved successfully",
                Data = shop
            };
            return Ok(response);
        }

        [Authorize]
        [HttpGet("user")]
        public async Task<IActionResult> GetShopByUserId()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var shop = await _shopService.GetShopByUserIdAsync(Guid.Parse(userId));
            var response = new BaseResponse<ShopResponseModel>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Shop retrieved successfully",
                Data = shop
            };
            return Ok(response);
        }

        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateShop([FromBody] CreateShopDto shop)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var updatedShop = await _shopService.UpdateShopAsync(userId, shop);
            var response = new BaseResponse<bool>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Shop updated successfully",
                Data = updatedShop
            };
            return Ok(response);
        }

        [HttpGet("CalculateAverageRating/{shopId}")]
        public async Task<IActionResult> CalculateAverageRating(string shopId)
        {
            var averageRating = await _shopService.CalculateShopAverageRatingAsync(shopId);
            var response = new BaseResponse<decimal>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Shop average rating calculated successfully.",
                Data = averageRating
            };
            return Ok(response);
        }
    }
}