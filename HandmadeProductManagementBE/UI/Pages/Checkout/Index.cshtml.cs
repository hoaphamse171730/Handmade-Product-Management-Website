using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Store;
using HandmadeProductManagement.ModelViews.CartItemModelViews;
using HandmadeProductManagement.ModelViews.UserInfoModelViews;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.RegularExpressions;

namespace UI.Pages.Checkout
{
    public class IndexModel : PageModel
    {
        private readonly ApiResponseHelper _apiResponseHelper;

        public IndexModel(ApiResponseHelper apiResponseHelper)
        {
            _apiResponseHelper = apiResponseHelper ?? throw new ArgumentNullException(nameof(apiResponseHelper));
        }
        public string? ErrorMessage { get; set; }
        public string? ErrorDetail { get; set; }

        public List<CartItemGroupDto> CartItems { get; set; } = new List<CartItemGroupDto>();
        public UserInfoDto UserInfo { get; set; } = new UserInfoDto();
        public decimal Subtotal { get; set; }
        public decimal Total { get; set; }

        public string Token { get; set; }

        public async Task OnGetAsync()
        {
            try
            {

                CartItems = await GetCartItemsAsync();
                UserInfo = await GetUserInfoAsync();
                Token = HttpContext.Session.GetString("Token");
            }
            catch (BaseException.ErrorException ex)
            {
                ErrorMessage = ex.ErrorDetail.ErrorCode;
                ErrorDetail = ex.ErrorDetail.ErrorMessage?.ToString();
            }
            catch (Exception ex)
            {
                ErrorMessage = "An unexpected error occurred.";
            }

        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {

                // Perform validation
                if (string.IsNullOrWhiteSpace(UserInfo.FullName))
            {
                ModelState.AddModelError(nameof(UserInfo.FullName), "Full Name is required.");
            }
            else if (!Regex.IsMatch(UserInfo.FullName, @"^[\p{L}\sĐđ]+$"))
            {
                ModelState.AddModelError(nameof(UserInfo.FullName), "Please enter a valid full name (letters and spaces only).");
            }

            if (string.IsNullOrWhiteSpace(UserInfo.PhoneNumber))
            {
                ModelState.AddModelError(nameof(UserInfo.PhoneNumber), "Phone Number is required.");
            }
            else if (!Regex.IsMatch(UserInfo.PhoneNumber, @"^0\d{9,10}$"))
            {
                ModelState.AddModelError(nameof(UserInfo.PhoneNumber), "Please enter a valid phone number (starting with 0 and containing 10-11 digits).");
            }

            if (string.IsNullOrWhiteSpace(UserInfo.Address))
            {
                ModelState.AddModelError(nameof(UserInfo.Address), "Address is required.");
            }
            else if (!Regex.IsMatch(UserInfo.Address, @"^[\p{L}\p{N}\s,\.Đđ]+$"))
            {
                ModelState.AddModelError(nameof(UserInfo.Address), "Please enter a valid address (can include letters, numbers, spaces, commas, and periods).");
            }

            // Check if ModelState is valid
            if (!ModelState.IsValid)
            {
                // If validation fails, return to the page with the validation messages
                CartItems = await GetCartItemsAsync();
                Token = HttpContext.Session.GetString("Token");
                return Page();
            }

            return RedirectToPage("/ProcessPayment");
            }
            catch (BaseException.ErrorException ex)
            {
                ErrorMessage = ex.ErrorDetail.ErrorCode;
                ErrorDetail = ex.ErrorDetail.ErrorMessage?.ToString();
            }
            catch (Exception ex)
            {
                ErrorMessage = "An unexpected error occurred.";
            }

            return Page();

        }

        private async Task<List<CartItemGroupDto>> GetCartItemsAsync()
        {
            var response = await _apiResponseHelper.GetAsync<List<CartItemGroupDto>>(Constants.ApiBaseUrl + "/api/cartitem");

            if (response.StatusCode == StatusCodeHelper.OK && response.Data != null)
            {
                var cartItems = response.Data;

                Subtotal = cartItems.Sum(group => group.CartItems.Sum(item => item.TotalPriceEachProduct));
                Total = Subtotal;

                return cartItems;
            }
            return new List<CartItemGroupDto>();
        }

        private async Task<UserInfoDto> GetUserInfoAsync()
        {
            var response = await _apiResponseHelper.GetAsync<UserInfoDto>(Constants.ApiBaseUrl + "/api/userinfo");

            if (response.StatusCode == StatusCodeHelper.OK && response.Data != null)
            {
                return response.Data;
            }
            return new UserInfoDto();
        }
    }
}
