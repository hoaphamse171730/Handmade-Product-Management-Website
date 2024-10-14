using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.ModelViews.CartModelViews;
using HandmadeProductManagement.Core.Base;
using Microsoft.AspNetCore.Mvc;
using HandmadeProductManagement.Contract.Services;
using HandmadeProductManagement.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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

        [HttpGet("cart")]
        [Authorize]
        public async Task<IActionResult> GetCart()
        {
            // Lấy thông tin userId từ token
            var userIdFromToken = User.FindFirstValue(ClaimTypes.NameIdentifier); // Giả sử NameIdentifier là claim cho userId

            if (!Guid.TryParse(userIdFromToken, out Guid userId))
            {
                return BadRequest(new { Message = "Invalid User ID" });
            }

            // Lấy giỏ hàng dựa trên userId
            var cart = await _cartService.GetCartByUserId(userId);

            return Ok(BaseResponse<CartModel>.OkResponse(cart));
        }

        [HttpPost("item/add/{cartId}")]
        public async Task<IActionResult> AddCartItem(string cartId, [FromBody] CreateCartItemDto createCartItemDto)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var response = new BaseResponse<bool>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Success",
                Data = await _cartItemService.AddCartItem(cartId, createCartItemDto, userId)
            };
            return Ok(response);
        }



        [HttpPut("item/updateQuantity/{cartItemId}")]
        public async Task<IActionResult> UpdateCartItem(string cartItemId, [FromBody] int productQuantity)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var response = new BaseResponse<bool>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Success",
                Data = await _cartItemService.UpdateCartItem(cartItemId, productQuantity, userId)
            };
            return Ok(response);
        }



        [HttpDelete("item/{cartItemId}")]
        public async Task<IActionResult> RemoveCartItem(string cartItemId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var response = new BaseResponse<bool>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Success",
                Data = await _cartItemService.DeleteCartItemByIdAsync(cartItemId, userId)
            };
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
