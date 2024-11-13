using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Store;
using HandmadeProductManagement.ModelViews.CartItemModelViews;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static HandmadeProductManagement.Core.Base.BaseException;
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
        public string? ErrorMessage { get; set; }
        public string? ErrorDetail { get; set; }
        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                await LoadCartItemsAsync();
            }
            catch (BaseException.ErrorException ex)
            {
                ErrorMessage = ex.ErrorDetail.ErrorCode;
                ErrorDetail = ex.ErrorDetail.ErrorMessage?.ToString();
                if (ErrorMessage == "unauthorized") return RedirectToPage("/Login");
            }
            catch (Exception ex)
            {
                ErrorMessage = "An unexpected error occurred.";
            }
            return Page();
        }
        // Phương thức để xóa mục khỏi giỏ hàng
        public async Task<IActionResult> OnPostDeleteAsync(string cartItemId)
        {
            var response = await _apiResponseHelper.DeleteAsync<bool>($"{Constants.ApiBaseUrl}/api/cartitem/{cartItemId}");
            if (response?.StatusCode == StatusCodeHelper.OK && response.Data)
            {
                await LoadCartItemsAsync();
            }
            else
            {
                ModelState.AddModelError(string.Empty, response?.Message ?? "Đã xảy ra lỗi khi xóa mục khỏi giỏ hàng.");
            }
            return Page();
        }
        // Phương thức để cập nhật số lượng sản phẩm trong giỏ hàng
        public async Task<IActionResult> OnPostUpdateQuantityAsync(string cartItemId, int newQuantity)
        {
            if (newQuantity < 1)
            {
                ModelState.AddModelError(string.Empty, "Quantity must be at least 1.");
                return Page();
            }
            var updateDto = new CartItemForUpdateDto { ProductQuantity = newQuantity };
            var response = await _apiResponseHelper.PutAsync<bool>($"{Constants.ApiBaseUrl}/api/cartitem/{cartItemId}", updateDto);
            if (response?.StatusCode == StatusCodeHelper.OK && response.Data)
            {
                // Tải lại danh sách giỏ hàng để cập nhật tổng tiền
                await LoadCartItemsAsync();
            }
            else
            {
                ModelState.AddModelError(string.Empty, response?.Message ?? "An error occurred while updating the quantity.");
            }
            return Page();
        }
        // Phương thức dùng để tải danh sách giỏ hàng và tính toán lại tổng
        private async Task LoadCartItemsAsync()
        {
            var response = await _apiResponseHelper.GetAsync<List<CartItemGroupDto>>($"{Constants.ApiBaseUrl}/api/cartitem");
            if (response?.StatusCode == StatusCodeHelper.OK && response.Data != null)
            {
                var cartItems = response.Data;
                Subtotal = cartItems.Sum(group => group.CartItems.Sum(item => item.DiscountPrice * item.ProductQuantity));
                Total = Subtotal;
                CartItems = cartItems;
            }
            else
            {
                ModelState.AddModelError(string.Empty, response?.Message ?? "Đã xảy ra lỗi khi lấy dữ liệu giỏ hàng.");
                CartItems = new List<CartItemGroupDto>();
            }
        }
    }
}
