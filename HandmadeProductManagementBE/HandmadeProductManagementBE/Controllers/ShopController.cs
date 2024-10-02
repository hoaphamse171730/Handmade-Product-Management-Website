using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.ShopModelViews;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        [HttpPost]
        public async Task<IActionResult> CreateShop([FromBody] CreateShopDto shop)
        {
            var createdShop = await _shopService.CreateShopAsync(shop);
            var response = new BaseResponse<bool>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Shop created successfully",
                Data = createdShop
            };
            return Ok(response);
        }

        [HttpDelete("{userId}/{id}")]
        public async Task<IActionResult> DeleteShop(Guid userId, string id)
        {
            var result = await _shopService.DeleteShopAsync(userId, id);
            var response = new BaseResponse<bool>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Shop deleted successfully",
                Data = result
            };
            return Ok(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllShops()
        {
            var shops = await _shopService.GetAllShopsAsync();
            var response = new BaseResponse<IList<ShopResponseModel>>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Shops retrieved successfully",
                Data = shops
            };
            return Ok(response);
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetShopByUserId(Guid userId)
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

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateShop(string id, [FromBody] CreateShopDto shop)
        {
            var updatedShop = await _shopService.UpdateShopAsync(id, shop);
            var response = new BaseResponse<bool>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Shop updated successfully",
                Data = updatedShop
            };
            return Ok(response);
        }
    }
}