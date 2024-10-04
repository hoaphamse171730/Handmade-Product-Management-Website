using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.ModelViews.CartModelViews;
using HandmadeProductManagement.Core.Base;
using Microsoft.AspNetCore.Mvc;
using HandmadeProductManagement.Contract.Services;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.UserModelViews;
using HandmadeProductManagement.Services.Service;

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
        public async Task<IActionResult> GetCart(Guid userId)
        {
                var cart = await _cartService.GetCartByUserId(userId);
            return Ok(BaseResponse<CartModel>.OkResponse(cart));
        }

        [HttpPost("item/add/{cartId}")]
        public async Task<IActionResult> AddCartItem(string cartId, [FromBody] CreateCartItemDto createCartItemDto)
        {
            var response = await _cartItemService.AddCartItem(cartId, createCartItemDto);
            return Ok(response);
        }



        [HttpPut("item/updateQuantity/{cartItemId}")]
        public async Task<IActionResult> UpdateCartItem(string cartItemId, [FromBody] int productQuantity)
        {
            var response = await _cartItemService.UpdateCartItem(cartItemId, productQuantity);
            return Ok(response);
        }



        [HttpDelete("item/{cartItemId}")]
        public async Task<IActionResult> RemoveCartItem(string cartItemId)
        {
            var response = await _cartItemService.RemoveCartItem(cartItemId);
            return Ok(response);
        }

        [HttpGet("getTotalPrice")]
        public async Task<IActionResult> GetTotalCartPrice(string cartId)
        {
            var response = new BaseResponse<Decimal>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Success",
                Data = await _cartService.GetTotalCartPrice(cartId),
            };
            return Ok(response);
        }

    }
}
