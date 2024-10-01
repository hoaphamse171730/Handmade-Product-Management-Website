using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.ModelViews.CartModelViews;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Constants;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using HandmadeProductManagement.Contract.Services;
using HandmadeProductManagement.Contract.Repositories.Entity;

namespace HandmadeProductManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly ICartItemService _cartItemService;

        public CartController(ICartService cartService, ICartItemService cartItemService)
        {
            _cartService = cartService;
            _cartItemService = cartItemService;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetCart([Required] Guid userId)
        {
            var cart = await _cartService.GetCartByUserId(userId);
            var response = new BaseResponse<CartModel>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Cart retrieved successfully.",
                Data = cart
            };
            return Ok(response);
        }

        [HttpPost("item/add/{cartId}")]
        public async Task<IActionResult> AddCartItem([Required] string cartId, [FromBody] CreateCartItemDto createCartItemDto)
        {
            var response = new BaseResponse<bool>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Add Cart Item successfully.",
                Data = await _cartItemService.AddCartItem(cartId, createCartItemDto);
            };
            return Ok(response);
        }

        [HttpPut("item/updateQuantity/{cartItemId}")]
        public async Task<IActionResult> UpdateCartItem([Required] string cartItemId, [Required] int productQuantity)
        {

            var response = new BaseResponse<bool>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Update Cart Item successfully.",
                Data = await _cartItemService.UpdateCartItem(cartItemId, productQuantity);
            };
            return Ok(response);
        }

        [HttpDelete("item/{cartItemId}")]
        public async Task<IActionResult> RemoveCartItem([Required] string cartItemId)
        {
            var response = new BaseResponse<bool>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Remove Cart Item successfully.",
                Data = await _cartItemService.RemoveCartItem(cartItemId);
            };
            return Ok(response);
        }
    }
}
