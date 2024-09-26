using Microsoft.AspNetCore.Mvc;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.ModelViews.CartModelViews;
using System;
using System.Threading.Tasks;
using HandmadeProductManagement.Contract.Services;

namespace HandmadeProductManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
        public async Task<IActionResult> GetCart(Guid userId)
        {
            var cart = await _cartService.GetCartByUserId(userId);
            return Ok(cart);
        }

        [HttpPost("{userId}")]
        public async Task<IActionResult> UpdateCart(Guid userId, [FromBody] CartModel cartModel)
        {
            var updatedCart = await _cartService.CreateOrUpdateCart(userId, cartModel);
            return Ok(updatedCart);
        }

        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteCart(Guid userId)
        {
            var result = await _cartService.DeleteCart(userId);
            return Ok(result);
        }

        [HttpPost("item/{cartId}")]
        public async Task<IActionResult> AddOrUpdateCartItem(Guid cartId, [FromBody] CartItemModel cartItemModel)
        {
            var result = await _cartItemService.AddOrUpdateCartItem(cartId, cartItemModel);
            return Ok(result);
        }

        [HttpDelete("item/{cartItemId}")]
        public async Task<IActionResult> RemoveCartItem(Guid cartItemId)
        {
            var result = await _cartItemService.RemoveCartItem(cartItemId);
            return Ok(result);
        }
    }
}
