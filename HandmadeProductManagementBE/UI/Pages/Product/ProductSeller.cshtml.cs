using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Store;
using HandmadeProductManagement.ModelViews.CategoryModelViews;
using HandmadeProductManagement.ModelViews.ProductModelViews;
using HandmadeProductManagement.ModelViews.ShopModelViews;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization; // Added for authorization
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace UI.Pages.Product
{
    //[Authorize] // Ensure the page is accessible only to authenticated users
    public class ProductSellerModel : PageModel
    {
        private readonly ApiResponseHelper _apiResponseHelper;
        private readonly IHttpClientFactory _httpClientFactory;

        public ProductSellerModel(ApiResponseHelper apiResponseHelper, IHttpClientFactory httpClientFactory)
        {
            _apiResponseHelper = apiResponseHelper ?? throw new ArgumentNullException(nameof(apiResponseHelper));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public List<ProductSearchVM>? Products { get; set; }
        public List<CategoryDto>? Categories { get; set; }

        public async Task<IActionResult> OnGetAsync(
            [FromQuery] string? Name,
            [FromQuery] string? CategoryId,
            [FromQuery] string? Status,
            [FromQuery] decimal? MinRating,
            [FromQuery] string SortOption,
            [FromQuery] bool SortDescending)
        {
            // Load categories for the filter dropdown
            await LoadCategoriesAsync();

            // Step 1: Extract userId from JWT token

            //var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            //if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
            //{
            //    ModelState.AddModelError(string.Empty, "User is not authenticated.");
            //    return Page();
            //}

            //if (!Guid.TryParse(userIdClaim.Value, out Guid userId))
            //{
            //    ModelState.AddModelError(string.Empty, "Invalid user identifier.");
            //    return Page();
            //}

            // Step 2: Fetch shopId using userId by calling the shop API
            //var shopResponse = await GetShopByUserIdAsync(userId);
            //if (shopResponse == null)
            //{
            //    // Error message already added in GetShopByUserIdAsync
            //    return Page();
            //}

            //var shopId = shopResponse.Id; 
            var shopId = "a61db0e7f95245ea86fbcfc7361ffcbc";

            // Step 3: Create the product search filter including shopId
            var searchFilter = new ProductSearchFilter
            {
                Name = Name,
                CategoryId = CategoryId,
                Status = Status,
                MinRating = MinRating,
                SortOption = SortOption,
                SortDescending = SortDescending,
                ShopId = shopId // Include shopId in the filter
            };

            // Step 4: Fetch products based on the search filter
            var response = await _apiResponseHelper.GetAsync<List<ProductSearchVM>>(
                $"{Constants.ApiBaseUrl}/api/product/search",
                searchFilter);

            if (response.StatusCode == StatusCodeHelper.OK && response.Data != null)
            {
                Products = response.Data;
            }
            else
            {
                ModelState.AddModelError(string.Empty, response.Message ?? "An error occurred while fetching products.");
            }

            return Page();
        }

        private async Task<ShopResponseModel?> GetShopByUserIdAsync(Guid userId)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",
                        await HttpContext.GetTokenAsync("access_token"));

                var response = await client.GetAsync($"{Constants.ApiBaseUrl}/api/shop/user");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var baseResponse = JsonSerializer.Deserialize<BaseResponse<ShopResponseModel>>(content, options);
                    if (baseResponse != null && baseResponse.StatusCode == StatusCodeHelper.OK && baseResponse.Data != null)
                    {
                        return baseResponse.Data;
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, baseResponse?.Message ?? "Failed to retrieve shop information.");
                        return null;
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "An error occurred while fetching shop information.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An unexpected error occurred: {ex.Message}");
                return null;
            }
        }

        private async Task LoadCategoriesAsync()
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync(Constants.ApiBaseUrl + "/api/category");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var baseResponse = JsonSerializer.Deserialize<BaseResponse<IList<CategoryDto>>>(content, options);
                if (baseResponse != null && baseResponse.StatusCode == StatusCodeHelper.OK && baseResponse.Data != null)
                {
                    Categories = baseResponse.Data.ToList();
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "An error occurred while fetching categories.");
            }
        }
    }
}