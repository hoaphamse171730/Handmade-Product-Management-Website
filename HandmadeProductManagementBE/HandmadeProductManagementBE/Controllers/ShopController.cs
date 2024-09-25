using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
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
            try
            {
                var createdShop = await _shopService.CreateShopAsync(shop);
                return Ok(BaseResponse<ShopResponseModel>.OkResponse(createdShop));
            }
            catch (BaseException.ErrorException ex)
            {
                return StatusCode(ex.StatusCode, new { ex.ErrorDetail.ErrorCode, ex.ErrorDetail.ErrorMessage });
            }
        }

        [HttpDelete("{userId}/{id}")]
        public async Task<IActionResult> DeleteShop(Guid userId, string id)
        {
            try
            {
                var result = await _shopService.DeleteShopAsync(userId, id);
                return Ok(BaseResponse<bool>.OkResponse(result));
            }
            catch (BaseException.ErrorException ex)
            {
                return StatusCode(ex.StatusCode, new { ex.ErrorDetail.ErrorCode, ex.ErrorDetail.ErrorMessage });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllShops()
        {
            try
            {
                var shops = await _shopService.GetAllShopsAsync();
                return Ok(BaseResponse<IList<ShopResponseModel>>.OkResponse(shops));
            }
            catch (BaseException.ErrorException ex)
            {
                return StatusCode(ex.StatusCode, new { ex.ErrorDetail.ErrorCode, ex.ErrorDetail.ErrorMessage });
            }
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetShopByUserId(Guid userId)
        {
            try
            {
                var shop = await _shopService.GetShopByUserIdAsync(userId);
                return Ok(BaseResponse<ShopResponseModel>.OkResponse(shop));
            }
            catch (BaseException.ErrorException ex)
            {
                return StatusCode(ex.StatusCode, new { ex.ErrorDetail.ErrorCode, ex.ErrorDetail.ErrorMessage });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateShop(string id, [FromBody] CreateShopDto shop)
        {
            try
            {
                var updatedShop = await _shopService.UpdateShopAsync(id, shop);
                return Ok(BaseResponse<ShopResponseModel>.OkResponse(updatedShop));
            }
            catch (BaseException.ErrorException ex)
            {
                return StatusCode(ex.StatusCode, new { ex.ErrorDetail.ErrorCode, ex.ErrorDetail.ErrorMessage });
            }
        }
    }
}