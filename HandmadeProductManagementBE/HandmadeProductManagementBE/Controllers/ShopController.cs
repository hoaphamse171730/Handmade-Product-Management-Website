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
            try
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
            catch (BaseException.ErrorException ex)
            {
                var response = new BaseResponse<object>
                {
                    Code = ex.ErrorDetail.ErrorCode,
                    StatusCode = (StatusCodeHelper)ex.StatusCode, 
                    Message = ex.ErrorDetail.ErrorMessage?.ToString(), 
                    Data = null
                };
                return StatusCode(ex.StatusCode, response);
            }
        }

        [HttpDelete("{userId}/{id}")]
        public async Task<IActionResult> DeleteShop(Guid userId, string id)
        {
            try
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
            catch (BaseException.ErrorException ex)
            {
                var response = new BaseResponse<object>
                {
                    Code = ex.ErrorDetail.ErrorCode,
                    StatusCode = (StatusCodeHelper)ex.StatusCode, 
                    Message = ex.ErrorDetail.ErrorMessage?.ToString(), 
                    Data = null
                };
                return StatusCode(ex.StatusCode, response);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllShops()
        {
            try
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
            catch (BaseException.ErrorException ex)
            {
                var response = new BaseResponse<object>
                {
                    Code = ex.ErrorDetail.ErrorCode,
                    StatusCode = (StatusCodeHelper)ex.StatusCode, 
                    Message = ex.ErrorDetail.ErrorMessage?.ToString(), 
                    Data = null
                };
                return StatusCode(ex.StatusCode, response);
            }
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetShopByUserId(Guid userId)
        {
            try
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
            catch (BaseException.ErrorException ex)
            {
                var response = new BaseResponse<object>
                {
                    Code = ex.ErrorDetail.ErrorCode,
                    StatusCode = (StatusCodeHelper)ex.StatusCode, 
                    Message = ex.ErrorDetail.ErrorMessage?.ToString(), 
                    Data = null
                };
                return StatusCode(ex.StatusCode, response);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateShop(string id, [FromBody] CreateShopDto shop)
        {
            try
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
            catch (BaseException.ErrorException ex)
            {
                var response = new BaseResponse<object>
                {
                    Code = ex.ErrorDetail.ErrorCode,
                    StatusCode = (StatusCodeHelper)ex.StatusCode, 
                    Message = ex.ErrorDetail.ErrorMessage?.ToString(),
                    Data = null
                };
                return StatusCode(ex.StatusCode, response);
            }
        }
    }
}