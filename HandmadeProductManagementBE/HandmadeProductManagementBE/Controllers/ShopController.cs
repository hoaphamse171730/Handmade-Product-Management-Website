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
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
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
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
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

        [HttpGet("{shopId}")]
        public async Task<IActionResult> GetShopById(string shopId)
        {
            var shop = await _shopService.GetShopByIdAsync(shopId);

            var response = new BaseResponse<ShopResponseModel>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Shop retrieved successfully",
                Data = shop
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
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
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
        [HttpGet("hasShop")]
        public async Task<IActionResult> HasShopByUserId()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var hasShop = await _shopService.HaveShopAsync(Guid.Parse(userId));
            var response = new BaseResponse<bool>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Shop retrieved successfully",
                Data = hasShop
            };
            return Ok(response);
        }

        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateShop([FromBody] CreateShopDto shop)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
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


        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllShops()
        {
            var shops = await _shopService.GetAllShopsAsync();
            var response = new BaseResponse<IList<ShopResponseModel>>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "All shops retrieved successfully",
                Data = shops
            };
            return Ok(response);
        }
        [HttpGet("admin_get/{shopId}")]
        public async Task<IActionResult> AdminGetShopById(string shopId)
        {
            var shop = await _shopService.AdminGetShopByIdAsync(shopId);

            var response = new BaseResponse<ShopDto>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Shop retrieved successfully",
                Data = shop
            };

            return Ok(response);
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetShopListByAdmin(int pageNumber, int pageSize)
        {
            var shops = await _shopService.GetShopListByAdmin(pageNumber, pageSize);
            var response = new BaseResponse<IList<ShopDto>>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "All shops retrieved successfully",
                Data = shops
            };
            return Ok(response);
        }
        [HttpGet("deleted")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetDeleteShops(int pageNumber, int pageSize)
        {
            var shops = await _shopService.GetDeletedShops(pageNumber, pageSize);
            var response = new BaseResponse<IList<ShopDto>>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "All shops retrieved successfully",
                Data = shops
            };
            return Ok(response);
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("deleteById/{shopId}")]
        public async Task<IActionResult> DeleteShopById(string shopId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var result = await _shopService.DeleteShopByIdAsync(shopId, userId);
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
        [HttpPut("{shopId}/recover")]
        public async Task<IActionResult> RecoverDeletedShop(string shopId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var result = await _shopService.RecoverDeletedShopAsync(shopId, userId);
            var response = new BaseResponse<bool>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Shop recover successfully",
                Data = result
            };
            return Ok(response);
        }


    }
}