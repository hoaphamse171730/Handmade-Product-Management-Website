using Microsoft.AspNetCore.Mvc;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.ModelViews.CartModelViews;
using System;
using System.Threading.Tasks;
using HandmadeProductManagement.Contract.Services;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Contract.Repositories.Entity;

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
        public async Task<ActionResult<CartModel>> GetCart(Guid userId)
        {
            var cart = await _cartService.GetCartByUserId(userId);
            return Ok(BaseResponse<CartModel>.OkResponse(cart));
        }

        [HttpPost("item/add/{cartId}")]
        public async Task<ActionResult<bool>> AddCartItem(string cartId, [FromBody] CreateCartItemDto createCartItemDto)
        {
            var result = await _cartItemService.AddCartItem(cartId, createCartItemDto);
            return Ok(BaseResponse<bool>.OkResponse(result));
        }

        [HttpPut("item/update/{cartItemId}")]
        public async Task<ActionResult<bool>> UpdateCartItem(string cartItemId, [FromBody] CartItemModel cartItemModel)
        {
            var result = await _cartItemService.UpdateCartItem(cartItemId, cartItemModel);
            return Ok(BaseResponse<bool>.OkResponse(result));
        }


        [HttpDelete("item/{cartItemId}")]
        public async Task<ActionResult<bool>> RemoveCartItem(string cartItemId)
        {
            var result = await _cartItemService.RemoveCartItem(cartItemId);
            return Ok(BaseResponse<bool>.OkResponse(result));
        }
    }
}
