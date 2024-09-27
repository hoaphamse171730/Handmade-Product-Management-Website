using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.ModelViews.CartModelViews;
using HandmadeProductManagement.Core.Base;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using HandmadeProductManagement.Contract.Services;
using HandmadeProductManagement.Core.Constants;

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
            try
            {
                var cart = await _cartService.GetCartByUserId(userId);
                if (cart == null)
                {
                    return NotFound(BaseResponse<CartModel>.FailResponse("No cart found for user ID.",StatusCodeHelper.NotFound));
                }
                return Ok(BaseResponse<CartModel>.OkResponse(cart));
            }
            catch (BaseException.ErrorException ex)
            {
                return StatusCode(ex.StatusCode, new { ex.ErrorDetail.ErrorCode, ex.ErrorDetail.ErrorMessage });
            }
        }

        [HttpPost("item/add/{cartId}")]
        public async Task<IActionResult> AddCartItem(string cartId, [FromBody] CreateCartItemDto createCartItemDto)
        {
            var response = await _cartItemService.AddCartItem(cartId, createCartItemDto);

            // Assuming 'StatusCode' is of type 'StatusCodeHelper' and checking if the status code indicates success.
            if (response.StatusCode != StatusCodeHelper.OK)
            {
                return StatusCode((int)response.StatusCode, response);
            }
            return Ok(response);
        }



        [HttpPut("item/updateQuantity/{cartItemId}")]
        public async Task<IActionResult> UpdateCartItem(string cartItemId, [FromBody] int productQuantity)
        {
            var response = await _cartItemService.UpdateCartItem(cartItemId, productQuantity);
            if (response.StatusCode != StatusCodeHelper.OK)
            {
                return StatusCode((int)response.StatusCode, response);
            }
            return Ok(response);
        }



        [HttpDelete("item/{cartItemId}")]
        public async Task<IActionResult> RemoveCartItem(string cartItemId)
        {
            var response = await _cartItemService.RemoveCartItem(cartItemId);
            if (response.StatusCode != StatusCodeHelper.OK)
            {
                return StatusCode((int)response.StatusCode, response);
            }
            return Ok(response);
        }

    }
}
