using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Store;
using HandmadeProductManagement.ModelViews.CategoryModelViews;
using HandmadeProductManagement.ModelViews.ProductModelViews;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json; // Chay okay hon 


// Continue with setting pagination

namespace UI.Pages.Product
{
    public class ProductListModel : PageModel
    {
        private readonly ApiResponseHelper _apiResponseHelper;
        private readonly IHttpClientFactory _httpClientFactory;

        public ProductListModel(ApiResponseHelper apiResponseHelper, IHttpClientFactory httpClientFactory)
        {
            _apiResponseHelper = apiResponseHelper ?? throw new ArgumentNullException(nameof(apiResponseHelper));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }
        public List<ProductSearchVM>? Products { get; set; }
        public List<CategoryDto>? Categories { get; set; }

        public string? ErrorMessage { get; set; }

        //Step 2: Define PageNumber & PageSize like this
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 2;
        public bool HasNextPage { get; set; } = true;
        public string CurrentFilters { get; set; } = string.Empty;

        //Step 3: Remember to add pageNumber & pageSize into parameter of OnGetAsync like below
        //Note: in page, the p is not the P
        public async Task OnGetAsync([FromQuery] string? Name, [FromQuery] string? CategoryId, [FromQuery] string? Status, [FromQuery] decimal? MinRating, [FromQuery] string SortOption, [FromQuery] bool SortDescending, int pageNumber = 1, int pageSize = 2)
        {
            try
            {

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

                // Serialize current filters into a query string format
                var queryParameters = new Dictionary<string, string?>
                {
                    { "Name", Name },
                    { "CategoryId", CategoryId },
                    { "Status", Status },
                    { "MinRating", MinRating?.ToString() },
                    { "SortOption", SortOption },
                    { "SortDescending", SortDescending.ToString() }
                };

                // Remove null or empty parameters
                var filteredParams = queryParameters
                .Where(kvp => !string.IsNullOrEmpty(kvp.Value))
                .Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value!)}");
                
                CurrentFilters = string.Join("&", filteredParams);

                //Final Step: add Page Number and Page Size into url like this: url/api/....?pageNumber={PageNumber}&pageSize={PageSize}
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
            }
            catch (BaseException.BadRequestException ex)
            {
                ErrorMessage = ex.Message;
            }
            catch (BaseException.UnauthorizedException ex)
            {
                ErrorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                ErrorMessage = "An unexpected error occurred.";
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
