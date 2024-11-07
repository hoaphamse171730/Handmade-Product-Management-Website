using GraphQLParser;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Store;
using HandmadeProductManagement.ModelViews.OrderModelViews;
using HandmadeProductManagement.ModelViews.ProductModelViews;
using HandmadeProductManagement.ModelViews.ReviewModelViews;
using HandmadeProductManagement.ModelViews.StatusChangeModelViews;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;

namespace UI.Pages.Order
{
    public class OrderDetailModel : PageModel
    {
        private readonly ApiResponseHelper _apiResponseHelper;
        private readonly IHttpClientFactory _httpClientFactory;


        public OrderDetailModel(ApiResponseHelper apiResponseHelper, IHttpClientFactory httpClientFactory)
        {
            _apiResponseHelper = apiResponseHelper ?? throw new ArgumentNullException(nameof(apiResponseHelper));
            _httpClientFactory = httpClientFactory;
        }
        [BindProperty]
        public OrderWithDetailDto OrderWithDetail { get; set; }
        [BindProperty]
        public List<StatusChangeDto>? StatusChanges { get; set; }
        [BindProperty]
        public string OrderId { get; set; } = string.Empty;
        public async Task OnGetAsync(string orderId)
        {
            OrderId = orderId;
            await FetchStatusChangesAsync(orderId);
            await FetchOrderDetailsAsync(orderId);
        }

        [BindProperty]
        public string ProductId { get; set; }
        public string ErrorMessage { get; set; }
        [BindProperty]
        public string Content { get; set; }
        [BindProperty]
        public int Rating { get; set; }
        public string Token { get; set; }

        private async Task FetchOrderDetailsAsync(string orderId)
        {
            var response = await _apiResponseHelper.GetAsync<OrderWithDetailDto>($"{Constants.ApiBaseUrl}/api/Order/{orderId}");

            if (response?.StatusCode == StatusCodeHelper.OK && response.Data != null)
            {
                OrderWithDetail = response.Data;
            }
            else
            {
                ModelState.AddModelError(string.Empty, response?.Message ?? "An error occurred while fetching order details.");
            }
        }

        private async Task FetchStatusChangesAsync(string orderId)
        {
            var response = await _apiResponseHelper.GetAsync<List<StatusChangeDto>>($"{Constants.ApiBaseUrl}/api/StatusChange/Order/{orderId}?sortAsc=true");

            if (response?.StatusCode == StatusCodeHelper.OK && response.Data != null)
            {
                StatusChanges = response.Data;
            }
            else
            {
                ModelState.AddModelError(string.Empty, response?.Message ?? "An error occurred while fetching status changes.");
            }
        }

        public async Task<IActionResult> OnPostReviewAsync()
        {
            ModelState.Clear();

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "There was an issue submitting your review. Please check the form and try again.";
                return RedirectToPage("/Order/OrderDetail", new { orderId = OrderId });
            }

            // Prepare the API URL and payload for creating a review
            var reviewCreateDto = new ReviewForCreationDto
            {
                ProductId = ProductId,
                Rating = Rating,
                Content = Content,
                OrderId = OrderId,
            };
            Token = HttpContext.Session.GetString("Token");
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
            var response = await client.PostAsJsonAsync(
                $"{Constants.ApiBaseUrl}/api/review", 
                reviewCreateDto);

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
                    TempData["SuccessMessage"] = "Review created successfully!";

                    return RedirectToPage("/Order/OrderDetail", new { orderId = OrderId });
                }

                ModelState.AddModelError(string.Empty, baseResponse?.Message ?? "Error updating user information.");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();

                var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(errorContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                ErrorMessage = errorResponse?.Detail ?? "An error occurred while resetting the password.";
                ModelState.AddModelError(string.Empty, ErrorMessage);

                TempData["ErrorMessage"] = $"{ErrorMessage}";
                return RedirectToPage("/Order/OrderDetail", new { orderId = OrderId });
            }

            return RedirectToPage("/Order/OrderDetail", new { orderId = OrderId });
        }
    }
}