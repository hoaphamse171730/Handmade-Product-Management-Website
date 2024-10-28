using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Store;
using HandmadeProductManagement.ModelViews.CartItemModelViews;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UI.Pages.Cart
{
    public class CartViewModel : PageModel
    {
        private readonly ApiResponseHelper _apiResponseHelper;

        public CartViewModel(ApiResponseHelper apiResponseHelper)
        {
            _apiResponseHelper = apiResponseHelper ?? throw new ArgumentNullException(nameof(apiResponseHelper));
        }

        public List<CartItemGroupDto> CartItems { get; set; } = new List<CartItemGroupDto>();
        public decimal Subtotal { get; set; }
        public decimal Total { get; set; }

        public async Task OnGetAsync()
        {
            CartItems = await GetCartItemsAsync();
        }

        private async Task<List<CartItemGroupDto>> GetCartItemsAsync()
        {
            try
            {
                var response = await _apiResponseHelper.GetAsync<List<CartItemGroupDto>>($"{Constants.ApiBaseUrl}/api/cartitem");

                if (response?.StatusCode == StatusCodeHelper.OK && response.Data != null)
                {
                    var cartItems = response.Data;

                    // Tính toán subtotal cho từng nhóm và tổng
                    Subtotal = cartItems.Sum(group => group.CartItems.Sum(item => item.DiscountPrice * item.ProductQuantity));
                    Total = Subtotal; // Cần thêm logic tính toán nếu có phí vận chuyển hoặc giảm giá

                    return cartItems;
                }
                else
                {
                    ModelState.AddModelError(string.Empty, response?.Message ?? "Đã xảy ra lỗi khi lấy dữ liệu giỏ hàng.");
                    return new List<CartItemGroupDto>();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Lỗi: {ex.Message}");
                return new List<CartItemGroupDto>();
            }
        }
        public async Task<IActionResult> OnPostDeleteItemAsync(string cartItemId)
        {
            var response = await _apiResponseHelper.DeleteAsync<bool>($"{Constants.ApiBaseUrl}/api/cartitem/{cartItemId}");

            if (response.StatusCode == StatusCodeHelper.OK)
            {
                // Cập nhật lại giỏ hàng sau khi xóa
                CartItems = await GetCartItemsAsync();
                return RedirectToPage();
            }

            ModelState.AddModelError(string.Empty, response.Message ?? "Đã xảy ra lỗi khi xóa sản phẩm.");
            return Page();
        }

    }
}
