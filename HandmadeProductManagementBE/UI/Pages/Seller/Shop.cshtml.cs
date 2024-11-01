using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Store;
using HandmadeProductManagement.ModelViews.CategoryModelViews;
using HandmadeProductManagement.ModelViews.ProductModelViews;
using HandmadeProductManagement.ModelViews.ShopModelViews;
using HandmadeProductManagement.ModelViews.VariationModelViews;
using HandmadeProductManagement.ModelViews.VariationOptionModelViews;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Text.Json;
using UI.Pages.Product;
using static HandmadeProductManagement.Core.Base.BaseException;

namespace UI.Pages.Seller
{
    public class ShopModel : PageModel
    {
        private readonly ApiResponseHelper _apiResponseHelper;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ProductSellerModel> _logger;

        public ShopModel(ApiResponseHelper apiResponseHelper, IHttpClientFactory httpClientFactory, ILogger<ProductSellerModel> logger)
        {
            _apiResponseHelper = apiResponseHelper ?? throw new ArgumentNullException(nameof(apiResponseHelper));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public string? ErrorMessage { get; set; }
        public string? ErrorDetail { get; set; }

        public ShopResponseModel Shop { get; private set; } = new ShopResponseModel();
        public List<ProductSearchVM>? Products { get; private set; }
        public List<CategoryDto>? Categories { get; private set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public List<VariationDto>? Variations { get; set; }
        [BindProperty]
        public ProductForCreationDto NewProduct { get; set; } = new();

        [BindProperty]
        public List<IFormFile> ProductImages { get; set; } = new();

        [BindProperty]
        public VariationForCreationDto NewVariation { get; set; } = new VariationForCreationDto
        {
            Name = string.Empty,      // Initialize with empty string
            CategoryId = string.Empty // Initialize with empty string
        };
        [BindProperty]
        public Dictionary<string, List<VariationOptionDto>> VariationOptions { get; set; } = new();


        public async Task OnGetAsync(
            [FromQuery] string? Name,
            [FromQuery] string? CategoryId,
            [FromQuery] string? Status,
            [FromQuery] decimal? MinRating,
            [FromQuery] string SortOption,
            [FromQuery] bool SortDescending,
            int pageNumber = 1, int pageSize = 12)
        {
            try
            {

                await LoadCategoriesAsync();
                PageNumber = pageNumber;
                PageSize = pageSize;

                Shop = await GetCurrentUserShop();

                Products = await GetProducts(Name, CategoryId, Status, MinRating, SortOption, SortDescending);

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

        private async Task<ShopResponseModel> GetCurrentUserShop()
        {
            var response = await _apiResponseHelper.GetAsync<ShopResponseModel>(Constants.ApiBaseUrl + "/api/shop/user");

            return response.StatusCode == StatusCodeHelper.OK && response.Data != null
                ? response.Data
                : new ShopResponseModel();
        }
        //
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
                $"{Constants.ApiBaseUrl}/api/product/search-seller?pageNumber={PageNumber}&pageSize={PageSize}", searchFilter);

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

        public async Task<IActionResult> OnPostCreateProductAsync([FromBody] ProductForCreationDto NewProduct)
        {
            try
            {

                try
                {
                    _logger.LogInformation("Starting product creation process");

                    if (!ModelState.IsValid)
                    {
                        return new JsonResult(ModelState.ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                        ))
                        { StatusCode = 400 };
                    }

                    // First create the product
                    var createProductResponse = await _apiResponseHelper.PostAsync<string>(
                        $"{Constants.ApiBaseUrl}/api/product",
                        NewProduct);

                    if (createProductResponse.StatusCode != StatusCodeHelper.OK)
                    {
                        _logger.LogError("Failed to create product. API Response: {Message}",
                            createProductResponse.Message);
                        return new JsonResult(new { error = createProductResponse.Message })
                        { StatusCode = 400 };
                    }

                    string productId = createProductResponse.Data ?? string.Empty;

                    // Handle image uploads if product creation was successful
                    if (ProductImages != null && ProductImages.Any())
                    {
                        foreach (var image in ProductImages)
                        {
                            try
                            {
                                var formData = new MultipartFormDataContent();
                                var fileContent = new StreamContent(image.OpenReadStream());
                                fileContent.Headers.ContentType =
                                    MediaTypeHeaderValue.Parse(image.ContentType);
                                formData.Add(fileContent, "file", image.FileName);

                                var uploadResponse = await _apiResponseHelper.PostMultipartAsync<bool>(
                                    $"{Constants.ApiBaseUrl}/api/productimage/upload?productId={productId}",
                                    formData);

                                if (uploadResponse.StatusCode != StatusCodeHelper.OK)
                                {
                                    _logger.LogError("Failed to upload image {FileName}. Response: {Message}",
                                        image.FileName, uploadResponse.Message);
                                }
                            }
                            catch (Exception imageEx)
                            {
                                _logger.LogError(imageEx, "Error uploading image {FileName}",
                                    image.FileName);
                            }
                        }
                    }

                    _logger.LogInformation("Product creation completed successfully");
                    return new JsonResult(new { success = true, productId });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled error in product creation");
                    return new JsonResult(new
                    {
                        error = "An unexpected error occurred while creating the product.",
                        details = ex.Message
                    })
                    { StatusCode = 500 };
                }
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
            return null;
        }

        public async Task<IActionResult> OnGetVariationsByCategoryAsync(string categoryId)
        {
            try
            {
                try
                {
                    var response = await _apiResponseHelper.GetAsync<List<VariationDto>>(
                        $"{Constants.ApiBaseUrl}/api/variation/category/{categoryId}");

                    if (response.StatusCode == StatusCodeHelper.OK && response.Data != null)
                    {
                        var variationsWithOptions = new List<object>();

                        // Load variation options for each variation
                        foreach (var variation in response.Data)
                        {
                            var optionsResponse = await _apiResponseHelper.GetAsync<List<VariationOptionDto>>(
                                $"{Constants.ApiBaseUrl}/api/variationoption/variation/{variation.Id}");

                            if (optionsResponse.StatusCode == StatusCodeHelper.OK)
                            {
                                // Create an anonymous object combining variation and its options
                                variationsWithOptions.Add(new
                                {
                                    id = variation.Id,
                                    name = variation.Name,
                                    categoryId = variation.CategoryId,
                                    variationOptions = optionsResponse.Data
                                });
                            }
                        }

                        return new JsonResult(variationsWithOptions);
                    }

                    return new JsonResult(new List<object>());
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading variations for category {CategoryId}", categoryId);
                    return new JsonResult(new List<object>());
                }
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
            return null;
        }

        private async Task<List<VariationDto>> LoadVariationsAsync(string categoryId)
        {
            var response = await _apiResponseHelper.GetAsync<List<VariationDto>>(
                $"{Constants.ApiBaseUrl}/api/variation/category/{categoryId}");

            if (response.StatusCode == StatusCodeHelper.OK && response.Data != null)
            {
                // Store variations
                Variations = response.Data;

                // Load and store options for each variation
                foreach (var variation in response.Data)
                {
                    var optionsResponse = await _apiResponseHelper.GetAsync<List<VariationOptionDto>>(
                        $"{Constants.ApiBaseUrl}/api/variationoption/variation/{variation.Id}");

                    if (optionsResponse.StatusCode == StatusCodeHelper.OK && optionsResponse.Data != null)
                    {
                        VariationOptions[variation.Id] = optionsResponse.Data;
                    }
                }

                return response.Data;
            }

            return new List<VariationDto>();
        }
    }
}
