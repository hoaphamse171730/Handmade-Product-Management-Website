using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Store;
using HandmadeProductManagement.ModelViews.CategoryModelViews;
using HandmadeProductManagement.ModelViews.ProductModelViews;
using HandmadeProductManagement.ModelViews.ShopModelViews;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace UI.Pages.Seller
{
    public class ShopModel : PageModel
    {
        private readonly ApiResponseHelper _apiResponseHelper;
        private readonly IHttpClientFactory _httpClientFactory;

        public ShopModel(ApiResponseHelper apiResponseHelper, IHttpClientFactory httpClientFactory)
        {
            _apiResponseHelper = apiResponseHelper ?? throw new ArgumentNullException(nameof(apiResponseHelper));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public ShopResponseModel Shop { get; private set; } = new ShopResponseModel();
        public List<ProductSearchVM>? Products { get; private set; }
        public List<CategoryDto>? Categories { get; private set; }

        public async Task OnGetAsync(
            string? name,
            string? categoryId,
            string? status,
            decimal? minRating,
            string sortOption = "Name",
            bool sortDescending = false)
        {
            Shop = await GetCurrentUserShop();
            Products = await GetProducts(name, categoryId, status, minRating, sortOption, sortDescending);
            await LoadCategoriesAsync();
        }

        private async Task<ShopResponseModel> GetCurrentUserShop()
        {
            var response = await _apiResponseHelper.GetAsync<ShopResponseModel>(Constants.ApiBaseUrl + "/api/shop/user");

            return response.StatusCode == StatusCodeHelper.OK && response.Data != null
                ? response.Data
                : new ShopResponseModel();
        }

        private async Task<List<ProductSearchVM>> GetProducts(
            string? name,
            string? categoryId,
            string? status,
            decimal? minRating,
            string sortOption,
            bool sortDescending)
        {
            var searchFilter = new ProductSearchFilter
            {
                Name = name,
                CategoryId = categoryId,
                Status = status,
                MinRating = minRating,
                SortOption = sortOption,
                SortDescending = sortDescending
            };

            var response = await _apiResponseHelper.GetAsync<List<ProductSearchVM>>(
                $"{Constants.ApiBaseUrl}/api/product/search-seller", searchFilter);

            return response.StatusCode == StatusCodeHelper.OK && response.Data != null
                ? response.Data
                : new List<ProductSearchVM>();
        }

        private async Task LoadCategoriesAsync()
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync(Constants.ApiBaseUrl + "/api/category");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var baseResponse = JsonSerializer.Deserialize<BaseResponse<IList<CategoryDto>>>(content, options);

                Categories = baseResponse?.StatusCode == StatusCodeHelper.OK && baseResponse.Data != null
                    ? baseResponse.Data.ToList()
                    : new List<CategoryDto>();
            }
            else
            {
                ModelState.AddModelError(string.Empty, "An error occurred while fetching categories.");
            }
        }
    }
}
