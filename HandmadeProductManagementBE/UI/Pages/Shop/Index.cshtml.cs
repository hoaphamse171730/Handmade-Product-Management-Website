using HandmadeProductManagement.Contract.Repositories.Entity;
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

namespace UI.Pages.Shop
{
    public class IndexModel : PageModel
    {
        private readonly ApiResponseHelper _apiResponseHelper;
        private readonly IHttpClientFactory _httpClientFactory;

        public IndexModel(ApiResponseHelper apiResponseHelper, IHttpClientFactory httpClientFactory)
        {
            _apiResponseHelper = apiResponseHelper ?? throw new ArgumentNullException(nameof(apiResponseHelper));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }
        public string? ErrorMessage { get; set; }
        public string? ErrorDetail { get; set; }
        public bool HasNextPage { get; set; } = true;
        public string CurrentFilters { get; set; } = string.Empty;

        public ShopResponseModel Shop { get; private set; } = new ShopResponseModel();
        public List<ProductSearchVM>? Products { get; private set; }
        public List<CategoryDto>? Categories { get; private set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 12;

        public async Task OnGetAsync(
            string? name,
            string? categoryId,
            string? status,
            decimal? minRating,
            string sortOption = "Name",
            bool sortDescending = false,
            string? id = null,
            int pageNumber = 1, int pageSize = 2)
        {
            try
            {
                PageNumber = pageNumber;
                PageSize = pageSize;

                if (string.IsNullOrEmpty(id))
                {
                    ModelState.AddModelError(string.Empty, "Shop ID is required.");
                    return;
                }

                Shop = await GetShopById(id);
                Products = await GetProducts(name, categoryId, status, minRating, sortOption, sortDescending, id);
                await LoadCategoriesAsync();
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

        private async Task<ShopResponseModel> GetShopById(string id)
        {
            var response = await _apiResponseHelper.GetAsync<ShopResponseModel>(Constants.ApiBaseUrl + $"/api/shop/{id}");

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
            bool sortDescending,
            string id)
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

            var queryParameters = new Dictionary<string, string?>
            {
                    { "Name", name },
                    { "CategoryId", categoryId },
                    { "Status", status },
                    { "MinRating", minRating?.ToString() },
                    { "SortOption", sortOption },
                    { "SortDescending", sortDescending.ToString() },
                    {"id",id }
                };

            // Remove null or empty parameters
            var filteredParams = queryParameters
            .Where(kvp => !string.IsNullOrEmpty(kvp.Value))
            .Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value!)}");

            CurrentFilters = string.Join("&", filteredParams);


            var response = await _apiResponseHelper.GetAsync<List<ProductSearchVM>>(
                $"{Constants.ApiBaseUrl}/api/product/shop/{id}?pageNumber={PageNumber}&pageSize={PageSize}", searchFilter);
            
            if (response.StatusCode == StatusCodeHelper.OK && response.Data != null)
            { 
                HasNextPage = response.Data.Count == PageSize;
                return response.Data;
            }
            return new List<ProductSearchVM>();
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
