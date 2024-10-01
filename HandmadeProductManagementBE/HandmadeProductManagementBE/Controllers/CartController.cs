using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.ModelViews.CartModelViews;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Constants;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using HandmadeProductManagement.Contract.Services;

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
            try
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
            catch (ArgumentException ex)
            {
                var response = new BaseResponse<CartModel>
                {
                    Code = "NotFound",
                    StatusCode = StatusCodeHelper.NotFound,
                    Message = ex.Message,
                    Data = null
                };
                return NotFound(response);
            }
            catch (Exception)
            {
                var response = new BaseResponse<CartModel>
                {
                    Code = "ServerError",
                    StatusCode = StatusCodeHelper.ServerError,
                    Message = "An unexpected error occurred.",
                    Data = null
                };
                return StatusCode(500, response);
            }
        }

        [HttpPost("item/add/{cartId}")]
        public async Task<IActionResult> AddCartItem([Required] string cartId, [FromBody] CreateCartItemDto createCartItemDto)
        {
            try
            {
                var response = await _cartItemService.AddCartItem(cartId, createCartItemDto);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                var response = new BaseResponse<bool>
                {
                    Code = "BadRequest",
                    StatusCode = StatusCodeHelper.BadRequest,
                    Message = ex.Message,
                    Data = false
                };
                return BadRequest(response);
            }
            catch (Exception)
            {
                var response = new BaseResponse<bool>
                {
                    Code = "ServerError",
                    StatusCode = StatusCodeHelper.ServerError,
                    Message = "An unexpected error occurred.",
                    Data = false
                };
                return StatusCode(500, response);
            }
        }

        [HttpPut("item/updateQuantity/{cartItemId}")]
        public async Task<IActionResult> UpdateCartItem([Required] string cartItemId, [Required] int productQuantity)
        {
            try
            {
                var response = await _cartItemService.UpdateCartItem(cartItemId, productQuantity);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                var response = new BaseResponse<bool>
                {
                    Code = "BadRequest",
                    StatusCode = StatusCodeHelper.BadRequest,
                    Message = ex.Message,
                    Data = false
                };
                return BadRequest(response);
            }
            catch (Exception)
            {
                var response = new BaseResponse<bool>
                {
                    Code = "ServerError",
                    StatusCode = StatusCodeHelper.ServerError,
                    Message = "An unexpected error occurred.",
                    Data = false
                };
                return StatusCode(500, response);
            }
        }

        [HttpDelete("item/{cartItemId}")]
        public async Task<IActionResult> RemoveCartItem([Required] string cartItemId)
        {
            try
            {
                var response = await _cartItemService.RemoveCartItem(cartItemId);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                var response = new BaseResponse<bool>
                {
                    Code = "BadRequest",
                    StatusCode = StatusCodeHelper.BadRequest,
                    Message = ex.Message,
                    Data = false
                };
                return BadRequest(response);
            }
            catch (Exception)
            {
                var response = new BaseResponse<bool>
                {
                    Code = "ServerError",
                    StatusCode = StatusCodeHelper.ServerError,
                    Message = "An unexpected error occurred.",
                    Data = false
                };
                return StatusCode(500, response);
            }
        }
    }
}
