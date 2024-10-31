using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Store;
using HandmadeProductManagement.ModelViews.CartItemModelViews;
using HandmadeProductManagement.ModelViews.OrderModelViews;
using HandmadeProductManagement.ModelViews.UserInfoModelViews;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace UI.Pages.Checkout
{
    public class IndexModel : PageModel
    {
        private readonly ApiResponseHelper _apiResponseHelper;
        private readonly IHttpClientFactory _httpClientFactory;


        public IndexModel(ApiResponseHelper apiResponseHelper, IHttpClientFactory httpClientFactory)
        {
            _apiResponseHelper = apiResponseHelper ?? throw new ArgumentNullException(nameof(apiResponseHelper));
            _httpClientFactory = httpClientFactory;
        }
        public string? ErrorMessage { get; set; }
        public string? ErrorDetail { get; set; }


        public List<CartItemGroupDto> CartItems { get; set; } = new List<CartItemGroupDto>();

        [BindProperty]
        public UserInfoDto UserInfo { get; set; } = new UserInfoDto();

        public decimal Subtotal { get; set; }
        public decimal Total { get; set; }

        [BindProperty]
        public string Note { get; set; } = string.Empty;

        public string Token { get; set; }

        public async Task OnGetAsync()
        {
            CartItems = await GetCartItemsAsync();
            //if (CartItems == null || !CartItems.Any())
            //{
            //    // Redirect to the previous page if the cart is empty
            //    Response.Redirect("/Index");
            //    return;
            //}
            UserInfo = await GetUserInfoAsync();
            Token = HttpContext.Session.GetString("Token");
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

        public async Task<IActionResult> OnPostAsync(string paymentMethod)
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

                // Handle order creation for COD payment method
                if (paymentMethod == "cod")
                {
                    var orderData = new CreateOrderDto
                    {
                        Address = UserInfo.Address ?? string.Empty,
                        CustomerName = UserInfo.FullName ?? string.Empty,
                        Phone = UserInfo.PhoneNumber ?? string.Empty,
                        Note = Note ?? string.Empty,
                    };

                    var client = _httpClientFactory.CreateClient();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                    var response = await client.PostAsJsonAsync($"{Constants.ApiBaseUrl}/api/order", orderData);

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };
                        var baseResponse = JsonSerializer.Deserialize<BaseResponse<bool>>(content, options);

                        if (baseResponse != null && baseResponse.StatusCode == StatusCodeHelper.OK)
                        {
                            return RedirectToPage("/Checkout/OrderSucess");
                        }

                        ModelState.AddModelError(string.Empty, baseResponse?.Message ?? "Error updating user information.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "An error occurred while updating user information.");
                    }
                }
                else if (paymentMethod == "vnpay")
                {
                    return RedirectToPage("/ProcessPayment");
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "An error occurred while updating user information.");
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

            private async Task<bool> CreateOrderAsync()
            {
                var orderData = new CreateOrderDto
                {
                    Address = UserInfo.Address ?? string.Empty,
                    CustomerName = UserInfo.FullName ?? string.Empty,
                    Phone = UserInfo.PhoneNumber ?? string.Empty,
                    Note = Note ?? string.Empty,
                };

                var response = await _apiResponseHelper.PostAsync<BaseResponse<bool>>(Constants.ApiBaseUrl + "/api/order", orderData);

                if (response != null && response.StatusCode == StatusCodeHelper.OK)
                {
                    return response.Data.Data;
                }

                return false;
            }

        }
    }
