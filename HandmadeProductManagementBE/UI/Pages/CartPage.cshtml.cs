using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Store;
using HandmadeProductManagement.ModelViews.CartItemModelViews;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UI.Pages
{
    public class CartPageModel : PageModel
    {
        private readonly ApiResponseHelper _apiResponseHelper;

        public CartPageModel(ApiResponseHelper apiResponseHelper)
        {
            _apiResponseHelper = apiResponseHelper ?? throw new ArgumentNullException(nameof(apiResponseHelper));
        }

        public List<CartItemDto>? CartItems { get; set; }

        public decimal TotalAmount { get; set; }

        public async Task OnGetAsync()
        {
            // Lấy danh sách sản phẩm trong giỏ hàng từ API
            var response = await _apiResponseHelper.GetAsync<List<CartItemDto>>(Constants.ApiBaseUrl + "/api/cartitem");

            // Kiểm tra kết quả trả về từ API
            if (response.StatusCode == StatusCodeHelper.OK && response.Data != null)
            {
                CartItems = response.Data;
                // Tính tổng giá trị giỏ hàng
                TotalAmount = CartItems.Sum(item => item.ProductQuantity * item.TotalPriceEachProduct);
            }
            else
            {
                // Hiển thị lỗi nếu có
                ModelState.AddModelError(string.Empty, response.Message ?? "An error occurred while fetching cart items.");
            }
        }

        //public async Task<IActionResult> OnPostEditItemAsync(int productId, int newQuantity)
        //{
        //    // Gửi yêu cầu chỉnh sửa số lượng sản phẩm trong giỏ hàng
        //    var response = await _apiResponseHelper.PostAsync<object>($"{Constants.ApiBaseUrl}/api/cartitem/{productId}", new { quantity = newQuantity });

        //    // Kiểm tra kết quả trả về từ API
        //    if (response.StatusCode == StatusCodeHelper.OK)
        //    {
        //        return RedirectToPage("/Cart");
        //    }
        //    else
        //    {
        //        // Hiển thị lỗi nếu có
        //        ModelState.AddModelError(string.Empty, response.Message ?? "An error occurred while updating the cart item.");
        //        return Page();
        //    }
        //}

        //public async Task<IActionResult> OnPostCheckoutAsync()
        //{
        //    // Gửi yêu cầu thanh toán giỏ hàng
        //    var response = await _apiResponseHelper.PostAsync<object>($"{Constants.ApiBaseUrl}/api/cart/checkout", null);

        //    // Kiểm tra kết quả trả về từ API
        //    if (response.StatusCode == StatusCodeHelper.OK)
        //    {
        //        return RedirectToPage("/CheckoutConfirmation");
        //    }
        //    else
        //    {
        //        // Hiển thị lỗi nếu có
        //        ModelState.AddModelError(string.Empty, response.Message ?? "An error occurred during checkout.");
        //        return Page();
        //    }
        //}
    }
}
