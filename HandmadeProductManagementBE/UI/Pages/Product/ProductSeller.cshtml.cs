using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Store;
using HandmadeProductManagement.ModelViews.CategoryModelViews;
using HandmadeProductManagement.ModelViews.ProductModelViews;
using HandmadeProductManagement.ModelViews.ShopModelViews;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using System.Linq;
using HandmadeProductManagement.ModelViews.VariationModelViews;
using HandmadeProductManagement.ModelViews.VariationOptionModelViews;

namespace UI.Pages.Product
{
    //[Authorize] // Ensure the page is accessible only to authenticated users
    public class ProductSellerModel : PageModel
    {
        private readonly ApiResponseHelper _apiResponseHelper;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ProductSellerModel> _logger;

        public ProductSellerModel(ApiResponseHelper apiResponseHelper, IHttpClientFactory httpClientFactory, ILogger<ProductSellerModel> logger)
        {
            _apiResponseHelper = apiResponseHelper ?? throw new ArgumentNullException(nameof(apiResponseHelper));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public List<ProductSearchVM>? Products { get; set; }
        public List<CategoryDto>? Categories { get; set; }
        public Dictionary<string, List<VariationWithOptionsDto>>? CategoryVariations { get; set; }

        // New properties for product creation
        [BindProperty]
        public ProductForCreationDto ProductCreation { get; set; } = new();

        [BindProperty]
        public List<IFormFile> ProductImages { get; set; } = new();
        [BindProperty]
        public string? SelectedCategoryId { get; set; }

        [BindProperty]
        public List<ProductVariationCombinationDto> VariationCombinations { get; set; } = new();

        public class VariationWithOptionsDto
        {
            public string Id { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public List<VariationOptionDto> Options { get; set; } = new();
        }

        public class ProductVariationCombinationDto
        {
            public List<string> OptionIds { get; set; } = new();
            public decimal Price { get; set; }
            public int StockQuantity { get; set; }
        }
        public class ProductVariationCombinationForCreationDto
        {
            public string ProductId { get; set; } = string.Empty;
            public List<string> VariationOptionIds { get; set; } = new();
            public decimal Price { get; set; }
            public int StockQuantity { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(
            [FromQuery] string? Name,
            [FromQuery] string? CategoryId,
            [FromQuery] string? Status,
            [FromQuery] decimal? MinRating,
            [FromQuery] string SortOption,
            [FromQuery] bool SortDescending)
        {
            await LoadCategoriesAsync();
            await LoadAllCategoryVariationsAsync();

            var searchFilter = new ProductSearchFilter
            {
                Name = Name,
                CategoryId = CategoryId,
                Status = Status,
                MinRating = MinRating,
                SortOption = SortOption,
                SortDescending = SortDescending
            };

            // Step 4: Fetch products based on the search filter
            var response = await _apiResponseHelper.GetAsync<List<ProductSearchVM>>(
                $"{Constants.ApiBaseUrl}/api/product/search-seller",
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

        private async Task LoadAllCategoryVariationsAsync()
        {
            CategoryVariations = new Dictionary<string, List<VariationWithOptionsDto>>();

            if (Categories != null)
            {
                foreach (var category in Categories)
                {
                    var variations = await LoadCategoryVariationsAsync(category.Id);
                    if (variations != null && variations.Any())
                    {
                        CategoryVariations[category.Id] = variations;
                    }
                }
            }
        }

        private async Task<List<VariationWithOptionsDto>?> LoadCategoryVariationsAsync(string categoryId)
        {
            try
            {
                var response = await _apiResponseHelper.GetAsync<List<VariationDto>>(
                    $"{Constants.ApiBaseUrl}/api/variation/category/{categoryId}");

                if (response.StatusCode == StatusCodeHelper.OK && response.Data != null)
                {
                    var variationsWithOptions = new List<VariationWithOptionsDto>();

                    foreach (var variation in response.Data)
                    {
                        var optionsResponse = await _apiResponseHelper.GetAsync<List<VariationOptionDto>>(
                            $"{Constants.ApiBaseUrl}/api/variationoption/variation/{variation.Id}");

                        if (optionsResponse.StatusCode == StatusCodeHelper.OK && optionsResponse.Data != null)
                        {
                            variationsWithOptions.Add(new VariationWithOptionsDto
                            {
                                Id = variation.Id,
                                Name = variation.Name,
                                Options = optionsResponse.Data
                            });
                        }
                    }

                    return variationsWithOptions;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading variations for category {CategoryId}", categoryId);
            }

            return null;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadCategoriesAsync();
                await LoadAllCategoryVariationsAsync();
                return Page();
            }

            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
                ProductCreation.ShopId = userId;

                // Create product
                var createProductResponse = await _apiResponseHelper.PostAsync<BaseResponse<string>>(
                    $"{Constants.ApiBaseUrl}/api/product",
                    ProductCreation);

                if (createProductResponse.StatusCode != StatusCodeHelper.OK)
                {
                    ModelState.AddModelError(string.Empty, "Failed to create product");
                    return Page();
                }

                // Handle variation combinations
                if (VariationCombinations.Any() && createProductResponse.Data != null)
                {
                    foreach (var combination in VariationCombinations)
                    {
                        var combinationDto = new ProductVariationCombinationForCreationDto
                        {
                            ProductId = createProductResponse.Data.ToString(),
                            VariationOptionIds = combination.OptionIds,
                            Price = combination.Price,
                            StockQuantity = combination.StockQuantity
                        };

                        await _apiResponseHelper.PostAsync<BaseResponse<bool>>(
                            $"{Constants.ApiBaseUrl}/api/product-variation-combination",
                            combinationDto);
                    }
                }

                // Handle image uploads
                if (ProductImages.Any() && createProductResponse.Data != null)
                {
                    foreach (var image in ProductImages)
                    {
                        await _apiResponseHelper.PostFileAsync<bool>(
                            $"{Constants.ApiBaseUrl}/api/product-image/upload?productId={createProductResponse.Data}",
                            image);
                    }
                }

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await LoadCategoriesAsync();
                await LoadAllCategoryVariationsAsync();
                return Page();
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

        public async Task<IActionResult> OnGetVariationsAsync(string categoryId)
        {
            try
            {
                var variations = await LoadCategoryVariationsAsync(categoryId);
                return new JsonResult(new { statusCode = 200, data = variations });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading variations for category {CategoryId}", categoryId);
                return new JsonResult(new { statusCode = 500, message = "Error loading variations" });
            }
        }
    }
}