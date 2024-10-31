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
        public List<CategoryDto> Categories { get; set; } = [];
        public string Token { get; set; }
        public List<ProductSearchVM>? Products { get; set; }

        //Step 2: Define PageNumber & PageSize like this
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 12;

        public async Task OnGetAsync([FromQuery] string? Name, [FromQuery] string? CategoryId, [FromQuery] string? Status, [FromQuery] decimal? MinRating, [FromQuery] string SortOption, [FromQuery] bool SortDescending, int pageNumber = 1, int pageSize = 12)
        {
            Token = HttpContext.Session.GetString("Token");
            ViewData["Token"] = Token;
            Top10SellingProducts = GetTop10SellingProducts();
            Top10NewProducts = GetTop10NewProducts();
            Categories = GetCategories();

            await LoadCategoriesAsync();

            //Step 4: PageNumber = pageNumber & PageSize = pageSize
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

            //Final Step: add Page Number and Page Size into url like this: url/api/....?pageNumber={PageNumber}&pageSize={PageSize}
            var response = await _apiResponseHelper.GetAsync<List<ProductSearchVM>>($"{Constants.ApiBaseUrl}/api/product/search?pageNumber={PageNumber}&pageSize={PageSize}", searchFilter);

            if (response.StatusCode == StatusCodeHelper.OK && response.Data != null)
            {
                Products = response.Data;
            }
            else
            {
                ModelState.AddModelError(string.Empty, response.Message ?? "An error occurred while fetching products.");
            }
        }

        private List<TopSellingProducts> GetTop10SellingProducts()
        {
            var response = _apiResponseHelper.GetAsync<List<TopSellingProducts>>(Constants.ApiBaseUrl +"/api/dashboard/top-10-selling-products").Result; // Lấy dữ liệu từ API
            if (response.StatusCode == StatusCodeHelper.OK && response.Data != null)
            {
                return response.Data;
            }
            return new List<TopSellingProducts>();
        }

        private List<ProductForDashboard> GetTop10NewProducts()
        {
            var response = _apiResponseHelper.GetAsync<List<ProductForDashboard>>(Constants.ApiBaseUrl + "/api/dashboard/top-10-new-products").Result; // Lấy dữ liệu từ API
            if (response.StatusCode == StatusCodeHelper.OK && response.Data != null)
            {
                return response.Data;
            }
            return new List<ProductForDashboard>();
        }

        private List<CategoryDto> GetCategories()
        {
            var response = _apiResponseHelper.GetAsync<List<CategoryDto>>(Constants.ApiBaseUrl + "/api/category").Result;
            if (response.StatusCode == StatusCodeHelper.OK && response.Data != null)
            {
                return response.Data;
            }
            return new List<CategoryDto>();
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
