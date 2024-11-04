using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Store;
using HandmadeProductManagement.ModelViews.CategoryModelViews;
using HandmadeProductManagement.ModelViews.ProductModelViews;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace UI.Pages
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

        public List<TopSellingProducts> Top10SellingProducts { get; set; }
        public List<ProductForDashboard> Top10NewProducts { get; set; }
        public List<CategoryDto> Categories { get; set; } = new List<CategoryDto>();
        public string Token { get; set; }
        public List<ProductSearchVM>? Products { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ErrorDetail { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public bool HasNextPage { get; set; } = true;
        public string CurrentFilters { get; set; } = string.Empty;

        public async Task OnGetAsync([FromQuery] string? Name, [FromQuery] string? CategoryId, [FromQuery] string? Status, [FromQuery] decimal? MinRating, [FromQuery] string SortOption, [FromQuery] bool SortDescending, int pageNumber = 1, int pageSize = 12)
        {
            try
            {
                Token = HttpContext.Session.GetString("Token");
                ViewData["Token"] = Token;
                Top10SellingProducts = await LoadProductsAsync<TopSellingProducts>("/api/dashboard/top-10-selling-products");
                Top10NewProducts = await LoadProductsAsync<ProductForDashboard>("/api/dashboard/top-10-new-products");
                await LoadCategoriesAsync();

                PageNumber = pageNumber;
                PageSize = pageSize;

                var searchFilter = new ProductSearchFilter
                {
                    Name = Name,
                    CategoryId = CategoryId,
                    Status = Status,
                    MinRating = MinRating,
                    SortOption = SortOption,
                    SortDescending = SortDescending
                };
                var queryParameters = new Dictionary<string, string?>
                {
                    { "Name", Name },
                    { "CategoryId", CategoryId },
                    { "Status", Status },
                    { "MinRating", MinRating?.ToString() },
                    { "SortOption", SortOption },
                    { "SortDescending", SortDescending.ToString() }
                };
                var filteredParams = queryParameters
        .Where(kvp => !string.IsNullOrEmpty(kvp.Value))
        .Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value!)}");

                CurrentFilters = string.Join("&", filteredParams);

                var response = await _apiResponseHelper.GetAsync<List<ProductSearchVM>>($"{Constants.ApiBaseUrl}/api/product/search?pageNumber={PageNumber}&pageSize={PageSize}", searchFilter);

                if (response.StatusCode == StatusCodeHelper.OK && response.Data != null)
                {
                    Products = response.Data;
                    HasNextPage = Products.Count == PageSize;
                }
                else
                {
                    ModelState.AddModelError(string.Empty, response.Message ?? "An error occurred while fetching products.");
                }
            
            } catch (BaseException.ErrorException ex)
            {
                ErrorMessage = ex.ErrorDetail.ErrorCode;
                ErrorDetail = ex.ErrorDetail.ErrorMessage?.ToString();
            }
            catch (Exception ex)
            {
                ErrorMessage = "An unexpected error occurred.";
            }
        }

        private async Task<List<T>> LoadProductsAsync<T>(string endpoint)
        {
            var response = await _apiResponseHelper.GetAsync<List<T>>(Constants.ApiBaseUrl + endpoint);
            return response.StatusCode == StatusCodeHelper.OK && response.Data != null ? response.Data : new List<T>();
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
