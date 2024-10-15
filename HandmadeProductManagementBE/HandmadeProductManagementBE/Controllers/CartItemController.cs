using Microsoft.AspNetCore.Mvc;
using HandmadeProductManagement.ModelViews.CartItemModelViews;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace HandmadeProductManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartItemController : ControllerBase
    {
        private readonly ICartItemService _cartItemService;

        public CartItemController(ICartItemService cartItemService)
        {
            _cartItemService = cartItemService;
        }

        [Authorize]
        // GET: api/cartitem
        [HttpGet]
        public async Task<IActionResult> GetCartItems()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var response = new BaseResponse<List<CartItemGroupDto>>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Retrieved cart items successfully!",
                Data = await _cartItemService.GetCartItemsByUserIdAsync(userId)
            };
            return Ok(response);
        }

        [Authorize]
        // POST: api/cartitem
        [HttpPost]
        public async Task<IActionResult> AddCartItem(CartItemForCreationDto createCartItemDto)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            try
            {
                var response = new BaseResponse<bool>
                {
                    Code = "Success",
                    StatusCode = StatusCodeHelper.OK,
                    Message = "Cart item added successfully!",
                    Data = await _cartItemService.AddCartItem(createCartItemDto, userId)
                };
                return Ok(response);
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse<string>
                {
                    Code = "DbUpdateException",
                    StatusCode = StatusCodeHelper.ServerError,
                    Message = $"An error occurred while updating the database. {userId}",
                    Data = dbEx.InnerException?.Message
                });
            }
        }

        [Authorize]
        // PUT: api/cartitem/{cartItemId}
        [HttpPut("{cartItemId}")]
        public async Task<IActionResult> UpdateCartItem(string cartItemId, [FromBody] CartItemForUpdateDto updateCartItemDto)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var response = new BaseResponse<bool>
                {
                    Code = "Success",
                    StatusCode = StatusCodeHelper.OK,
                    Message = "Cart item updated successfully!",
                    Data = await _cartItemService.UpdateCartItem(cartItemId, updateCartItemDto, userId)
                };
                return Ok(response);
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse<string>
                {
                    Code = "DbUpdateException",
                    StatusCode = StatusCodeHelper.ServerError,
                    Message = "An error occurred while updating the database.",
                    Data = dbEx.InnerException?.Message
                });
            }
        }

        [Authorize]
        // DELETE: api/cartitem/{cartItemId}
        [HttpDelete("{cartItemId}")]
        public async Task<IActionResult> DeleteCartItem(string cartItemId)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var response = new BaseResponse<bool>
                {
                    Code = "Success",
                    StatusCode = StatusCodeHelper.OK,
                    Message = "Cart item deleted successfully!",
                    Data = await _cartItemService.DeleteCartItemByIdAsync(cartItemId, userId)
                };
                return Ok(response);
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse<string>
                {
                    Code = "DbUpdateException",
                    StatusCode = StatusCodeHelper.ServerError,
                    Message = "An error occurred while updating the database.",
                    Data = dbEx.InnerException?.Message
                });
            }
            
        }

        [Authorize]
        // GET: api/cartitem/total
        [HttpGet("total")]
        public async Task<IActionResult> GetTotalCartPrice()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var totalPrice = await _cartItemService.GetTotalCartPrice(userId);
            var response = new BaseResponse<decimal>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Total cart price retrieved successfully!",
                Data = totalPrice
            };
            return Ok(response);
        }
    }
}
