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
using System.Net.Http.Headers;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using HandmadeProductManagement.ModelViews.VariationCombinationModelViews;
using System.Security.Claims;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;
using static HandmadeProductManagement.Core.Base.BaseException;
using HandmadeProductManagement.ModelViews.ProductDetailModelViews;
using HandmadeProductManagement.ModelViews.ProductItemModelViews;
using HandmadeProductManagement.ModelViews.ProductImageModelViews;

namespace UI.Pages.Product
{
    //[Authorize] // Ensure the page is accessible only to authenticated users
    public class ProductSellerModel : PageModel
    {
        private readonly ApiResponseHelper _apiResponseHelper;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ProductSellerModel> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProductSellerModel(ApiResponseHelper apiResponseHelper, IHttpClientFactory httpClientFactory, ILogger<ProductSellerModel> logger, IHttpContextAccessor httpContextAccessor)
        {
            _apiResponseHelper = apiResponseHelper ?? throw new ArgumentNullException(nameof(apiResponseHelper));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public List<ProductSearchVM>? Products { get; set; }
        public List<CategoryDto>? Categories { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 12;

        public async Task<IActionResult> OnGetAsync(
            [FromQuery] string? Name,
            [FromQuery] string? CategoryId,
            [FromQuery] string? Status,
            [FromQuery] decimal? MinRating,
            [FromQuery] string SortOption,
            [FromQuery] bool SortDescending,
            int pageNumber = 1,
            int pageSize = 12)
        {
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

            // Step 4: Fetch products based on the search filter
            var response = await _apiResponseHelper.GetAsync<List<ProductSearchVM>>(
                $"{Constants.ApiBaseUrl}/api/product/search-seller?pageNumber={PageNumber}&pageSize={PageSize}",
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

        private async Task LoadCategoriesAsync()
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{Constants.ApiBaseUrl}/api/category");

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

        public async Task<IActionResult> OnPostToggleProductStatusAsync(string productId, bool isAvailable)
        {
            try
            {
                _logger.LogInformation($"Attempting to toggle product status. ProductId: {productId}, IsAvailable: {isAvailable}");

                var apiUrl = $"{Constants.ApiBaseUrl}/api/product/update-status/{productId}?isAvailable={isAvailable}";
                var response = await _apiResponseHelper.PutProductStatusUpdateAsync(apiUrl, isAvailable);

                if (response != null && response.Data)
                {
                    return new JsonResult(new { success = true });
                }
                else
                {
                    return new JsonResult(new { success = false, error = response?.Message ?? "Failed to update product status" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error toggling product status for ProductId: {productId}");
                return new JsonResult(new { success = false, error = "An error occurred while processing your request" });
            }
        }

        public async Task<IActionResult> OnPostDeleteProductAsync(string id)
        {
            try
            {
                var response = await _apiResponseHelper.DeleteAsync<bool>(
                    $"{Constants.ApiBaseUrl}/api/product/soft-delete/{id}");

                if (response.StatusCode == StatusCodeHelper.OK)
                {
                    return new JsonResult(new { success = true });
                }

                return new JsonResult(new { error = response.Message }) { StatusCode = 400 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product {ProductId}", id);
                return new JsonResult(new { error = "An error occurred while deleting the product" })
                { StatusCode = 500 };
            }
        }

        //public async Task<IActionResult> OnPostDeleteProductImageAsync(string imageId)
        //{
        //    try
        //    {
        //        var response = await _apiResponseHelper.DeleteAsync<bool>(
        //            $"{Constants.ApiBaseUrl}/api/productimage/{imageId}");

        //        if (response.StatusCode == StatusCodeHelper.OK)
        //        {
        //            return new JsonResult(new { success = true });
        //        }

        //        return new JsonResult(new { error = response.Message }) { StatusCode = 400 };
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error deleting product image {ImageId}", imageId);
        //        return new JsonResult(new { error = "An error occurred while deleting the image" })
        //        { StatusCode = 500 };
        //    }
        //}

        [BindProperty]
        public ProductForCreationDto ProductCreation { get; set; } = new ProductForCreationDto();

        [BindProperty]
        public List<IFormFile> ProductImages { get; set; } = new List<IFormFile>();

        [BindProperty]
        public List<NewVariationDto> NewVariations { get; set; } = new List<NewVariationDto>();
        public List<VariationDto>? CategoryVariations { get; set; }
        public List<VariationOptionDto>? VariationOptions { get; set; }
        public class NewVariationDto
        {
            public string Name { get; set; } = string.Empty;
            public List<string> Options { get; set; } = new List<string>();
        }

        public async Task<IActionResult> OnPostCreateProductAsync()
        {
            try
            {
                if (!ModelState.IsValid ||
                    string.IsNullOrEmpty(ProductCreation.Name) ||
                    string.IsNullOrEmpty(ProductCreation.Description) ||
                    string.IsNullOrEmpty(ProductCreation.CategoryId))
                {
                    return new JsonResult(new { success = false, message = "Invalid product data" });
                }

                var shopResponse = await _apiResponseHelper.GetAsync<ShopResponseModel>($"{Constants.ApiBaseUrl}/api/shop/user");
                if (shopResponse.StatusCode != StatusCodeHelper.OK || shopResponse.Data == null)
                {
                    return new JsonResult(new { success = false, message = "Could not retrieve shop information" });
                }

                ProductCreation.ShopId = shopResponse.Data.Id;

                var variations = new List<VariationForProductCreationDto>();
                var existingVariationInputs = Request.Form.Keys
                    .Where(k => k.StartsWith("variation_"))
                    .Select(k => new
                    {
                        VariationId = k.Replace("variation_", ""),
                        OptionId = Request.Form[k].ToString()
                    })
                    .Where(v => !string.IsNullOrEmpty(v.OptionId))
                    .Select(v => new VariationForProductCreationDto
                    {
                        Id = v.VariationId,
                        VariationOptionIds = new List<string> { v.OptionId }
                    })
                    .ToList();

                variations.AddRange(existingVariationInputs);

                if (!variations.Any())
                {
                    return new JsonResult(new { success = false, message = "At least one variation is required" });
                }

                var variationCombinations = ParseVariationCombinations();
                if (!variationCombinations.Any())
                {
                    return new JsonResult(new { success = false, message = "At least one variation combination is required" });
                }

                var productCreation = new
                {
                    name = ProductCreation.Name,
                    description = ProductCreation.Description,
                    categoryId = ProductCreation.CategoryId,
                    shopId = shopResponse.Data.Id,
                    variations = variations,
                    variationCombinations = variationCombinations
                };

                var createProductResponse = await _apiResponseHelper.PostAsync<bool>(
                    $"{Constants.ApiBaseUrl}/api/product",
                    productCreation);

                if (createProductResponse.StatusCode != StatusCodeHelper.OK || !createProductResponse.Data)
                {
                    return new JsonResult(new { success = false, message = "Failed to create product" });
                }

                if (ProductImages?.Any() == true)
                {
                    await HandleImageUploads();
                }

                return RedirectToPage("/Product/ProductSeller");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return new JsonResult(new { success = false, message = $"Error creating product: {ex.Message}" });
            }
        }

        private List<VariationCombinationDto> ParseVariationCombinations()
        {
            var combinations = new List<VariationCombinationDto>();
            var combinationCount = Request.Form
                .Where(x => x.Key.StartsWith("VariationCombinations[") && x.Key.EndsWith("].Price"))
                .Count();

            for (int i = 0; i < combinationCount; i++)
            {
                if (int.TryParse(Request.Form[$"VariationCombinations[{i}].Price"], out int price) &&
                    int.TryParse(Request.Form[$"VariationCombinations[{i}].Stock"], out int stock))
                {
                    var optionIds = Request.Form[$"VariationCombinations[{i}].OptionIds"]
                        .ToString()
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .ToList();

                    combinations.Add(new VariationCombinationDto
                    {
                        VariationOptionIds = optionIds,
                        Price = price,
                        QuantityInStock = stock
                    });
                }
            }

            return combinations;
        }

        private async Task HandleImageUploads()
        {
            var latestProductsResponse = await _apiResponseHelper.GetAsync<List<ProductOverviewDto>>(
                $"{Constants.ApiBaseUrl}/api/product/user?pageNumber=1&pageSize=1");

            if (latestProductsResponse.StatusCode == StatusCodeHelper.OK &&
                latestProductsResponse.Data?.Any() == true)
            {
                var latestProductId = latestProductsResponse.Data.First().Id;

                foreach (var image in ProductImages)
                {
                    if (image != null && image.Length > 0)
                    {
                        var uploadResponse = await _apiResponseHelper.UploadImageAsync(
                            $"{Constants.ApiBaseUrl}/api/productimage/Upload",
                            image,
                            latestProductId);

                        if (uploadResponse.StatusCode != StatusCodeHelper.OK || !uploadResponse.Data)
                        {
                            _logger.LogError($"Failed to upload image for product {latestProductId}");
                        }
                    }
                }
            }
        }

        public async Task<IActionResult> OnPostCreateVariationAsync([FromBody] CreateVariationRequest request)
        {
            try
            {
                // Create variation
                var variationCreationDto = new
                {
                    name = request.Name,
                    categoryId = request.CategoryId
                };

                var createVariationResponse = await _apiResponseHelper.PostAsync<bool>(
                    $"{Constants.ApiBaseUrl}/api/variation",
                    variationCreationDto);

                if (createVariationResponse.StatusCode != StatusCodeHelper.OK || !createVariationResponse.Data)
                {
                    return new JsonResult(new { success = false, message = "Failed to create variation" });
                }

                // Get latest variation ID
                var latestVariationResponse = await _apiResponseHelper.GetAsync<LatestVariationId>(
                    $"{Constants.ApiBaseUrl}/api/variation/latest?categoryId={request.CategoryId}");

                if (latestVariationResponse.StatusCode != StatusCodeHelper.OK ||
                    string.IsNullOrEmpty(latestVariationResponse.Data?.Id))
                {
                    return new JsonResult(new { success = false, message = "Failed to retrieve variation ID" });
                }

                var variationId = latestVariationResponse.Data.Id;
                var optionIds = new List<string>();

                // Create options
                foreach (var optionValue in request.Options)
                {
                    var optionDto = new
                    {
                        value = optionValue,
                        variationId = variationId
                    };

                    var createOptionResponse = await _apiResponseHelper.PostAsync<bool>(
                        $"{Constants.ApiBaseUrl}/api/variationoption",
                        optionDto);

                    if (createOptionResponse.StatusCode != StatusCodeHelper.OK || !createOptionResponse.Data)
                    {
                        return new JsonResult(new { success = false, message = $"Failed to create option: {optionValue}" });
                    }

                    var latestOptionResponse = await _apiResponseHelper.GetAsync<LatestVariationOptionId>(
                        $"{Constants.ApiBaseUrl}/api/variationoption/latest?variationId={variationId}");

                    if (latestOptionResponse.StatusCode != StatusCodeHelper.OK ||
                        string.IsNullOrEmpty(latestOptionResponse.Data?.Id))
                    {
                        return new JsonResult(new { success = false, message = "Failed to retrieve option ID" });
                    }

                    optionIds.Add(latestOptionResponse.Data.Id);
                }

                return new JsonResult(new
                {
                    success = true,
                    variationId = variationId,
                    optionIds = optionIds
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating variation");
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> OnPostCreateVariationOptionAsync([FromBody] CreateVariationOptionRequest request)
        {
            try
            {
                var optionDto = new
                {
                    value = request.Value,
                    variationId = request.VariationId
                };

                var createOptionResponse = await _apiResponseHelper.PostAsync<bool>(
                    $"{Constants.ApiBaseUrl}/api/variationoption",
                    optionDto);

                if (createOptionResponse.StatusCode != StatusCodeHelper.OK || !createOptionResponse.Data)
                {
                    return new JsonResult(new { success = false, message = "Failed to create option" });
                }

                var latestOptionResponse = await _apiResponseHelper.GetAsync<LatestVariationOptionId>(
                    $"{Constants.ApiBaseUrl}/api/variationoption/latest?variationId={request.VariationId}");

                if (latestOptionResponse.StatusCode != StatusCodeHelper.OK ||
                    string.IsNullOrEmpty(latestOptionResponse.Data?.Id))
                {
                    return new JsonResult(new { success = false, message = "Failed to retrieve option ID" });
                }

                return new JsonResult(new
                {
                    success = true,
                    optionId = latestOptionResponse.Data.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating variation option");
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        public class CreateVariationRequest
        {
            public string Name { get; set; }
            public string CategoryId { get; set; }
            public List<string> Options { get; set; }
        }

        public class CreateVariationOptionRequest
        {
            public string Value { get; set; }
            public string VariationId { get; set; }
        }

        //public async Task<IActionResult> OnPostCreateVariationAsync([FromBody] dynamic data)
        //{
        //    try
        //    {
        //        var name = (string)data.name;
        //        var categoryId = (string)data.categoryId;

        //        var variationCreationDto = new { name, categoryId };
        //        var createVariationResponse = await _apiResponseHelper.PostAsync<bool>($"{Constants.ApiBaseUrl}/api/variation", variationCreationDto);

        //        if (createVariationResponse.StatusCode != StatusCodeHelper.OK || !createVariationResponse.Data)
        //        {
        //            return new JsonResult(new { success = false, message = $"Failed to create variation: {name}" });
        //        }

        //        var latestVariationResponse = await _apiResponseHelper.GetAsync<LatestVariationId>($"{Constants.ApiBaseUrl}/api/variation/latest?categoryId={categoryId}");
        //        if (latestVariationResponse.StatusCode != StatusCodeHelper.OK || string.IsNullOrEmpty(latestVariationResponse.Data?.Id))
        //        {
        //            return new JsonResult(new { success = false, message = "Failed to retrieve variation ID" });
        //        }

        //        return new JsonResult(new { success = true, variationId = latestVariationResponse.Data.Id });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error creating variation");
        //        return new JsonResult(new { success = false, message = "An unexpected error occurred" });
        //    }
        //}

        //public async Task<IActionResult> OnPostCreateVariationOptionAsync([FromBody] dynamic data)
        //{
        //    try
        //    {
        //        var value = (string)data.value;
        //        var variationId = (string)data.variationId;

        //        var optionDto = new { value, variationId };
        //        var createOptionResponse = await _apiResponseHelper.PostAsync<bool>($"{Constants.ApiBaseUrl}/api/variationoption", optionDto);

        //        if (createOptionResponse.StatusCode != StatusCodeHelper.OK || !createOptionResponse.Data)
        //        {
        //            return new JsonResult(new { success = false, message = $"Failed to create option: {value}" });
        //        }

        //        var latestOptionResponse = await _apiResponseHelper.GetAsync<LatestVariationOptionId>($"{Constants.ApiBaseUrl}/api/variationoption/latest?variationId={variationId}");
        //        if (latestOptionResponse.StatusCode != StatusCodeHelper.OK || string.IsNullOrEmpty(latestOptionResponse.Data?.Id))
        //        {
        //            return new JsonResult(new { success = false, message = "Failed to retrieve option ID" });
        //        }

        //        return new JsonResult(new { success = true, optionId = latestOptionResponse.Data.Id });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error creating variation option");
        //        return new JsonResult(new { success = false, message = "An unexpected error occurred" });
        //    }
        //}

        //private async Task<string> GetVariationIdByName(string variationName, string categoryId)
        //{
        //    var variationsResponse = await _apiResponseHelper.GetAsync<List<VariationDto>>(
        //        $"{Constants.ApiBaseUrl}/api/variation/category/{categoryId}");

        //    if (variationsResponse.StatusCode == StatusCodeHelper.OK && variationsResponse.Data != null)
        //    {
        //        var variation = variationsResponse.Data.FirstOrDefault(v => v.Name.Equals(variationName, StringComparison.OrdinalIgnoreCase));
        //        return variation?.Id ?? string.Empty;
        //    }

        //    return string.Empty;
        //}

        //public async Task<IActionResult> OnGetLatestVariationIdAsync(string categoryId)
        //{
        //    // Call the API to get the latest variation ID for the specified category
        //    var latestVariationIdResponse = await _apiResponseHelper.GetAsync<LatestVariationId>(
        //        $"{Constants.ApiBaseUrl}/api/variation/latest?categoryId={categoryId}");

        //    // Check if the response was successful
        //    if (latestVariationIdResponse.StatusCode == StatusCodeHelper.OK && !string.IsNullOrEmpty(latestVariationIdResponse.Data!.Id))
        //    {
        //        // Return the latest variation ID as a JSON result
        //        return new JsonResult(new { LatestVariationId = latestVariationIdResponse.Data });
        //    }

        //    // Return a JSON result with a null or empty string if no latest variation ID is found
        //    return new JsonResult(new { LatestVariationId = string.Empty });
        //}

        //public async Task<IActionResult> OnGetLatestVariationOptionIdAsync(string variationId)
        //{
        //    // Call the API to get the latest variation option ID for the specified variation
        //    var latestOptionIdResponse = await _apiResponseHelper.GetAsync<LatestVariationOptionId>(
        //        $"{Constants.ApiBaseUrl}/api/variationoption/latest?variationId={variationId}");

        //    // Check if the response was successful
        //    if (latestOptionIdResponse.StatusCode == StatusCodeHelper.OK && !string.IsNullOrEmpty(latestOptionIdResponse.Data!.Id))
        //    {
        //        // Return the latest variation option ID as a JSON result
        //        return new JsonResult(new { LatestVariationOptionId = latestOptionIdResponse.Data });
        //    }

        //    // Return a JSON result with a null or empty string if no latest variation option ID is found
        //    return new JsonResult(new { LatestVariationOptionId = string.Empty });
        //}

        //public async Task<IActionResult> OnGetLatestVariationIdByCategoryAsync(string categoryId)
        //{
        //    // Call the API endpoint to get the latest variation ID for the specified category
        //    var latestVariationIdResponse = await _apiResponseHelper.GetAsync<string>(
        //        $"{Constants.ApiBaseUrl}/api/variation/latest?categoryId={categoryId}");

        //    // Check if the response is successful and return the result
        //    if (latestVariationIdResponse.StatusCode == StatusCodeHelper.OK)
        //    {
        //        var latestVariationId = latestVariationIdResponse.Data;
        //        return new JsonResult(latestVariationId);
        //    }

        //    // If the call fails, return an empty string or null
        //    return new JsonResult(string.Empty);
        //}

        public async Task<IActionResult> OnGetCategoryVariationsAsync(string categoryId)
        {
            var variationsResponse = await _apiResponseHelper.GetAsync<List<VariationDto>>(
                $"{Constants.ApiBaseUrl}/api/variation/category/{categoryId}");

            if (variationsResponse.StatusCode == StatusCodeHelper.OK)
            {
                CategoryVariations = variationsResponse.Data;
                return new JsonResult(CategoryVariations);
            }

            return new JsonResult(new List<VariationDto>());
        }

        public async Task<IActionResult> OnGetVariationOptionsAsync(string variationId)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(variationId))
                {
                    return new JsonResult(new List<VariationOptionDto>());
                }

                var optionsResponse = await _apiResponseHelper.GetAsync<List<VariationOptionDto>>(
                    $"{Constants.ApiBaseUrl}/api/variationoption/variation/{variationId}");

                if (optionsResponse.StatusCode == StatusCodeHelper.OK && optionsResponse.Data != null)
                {
                    // Log the options for debugging
                    _logger.LogInformation($"Loaded {optionsResponse.Data.Count} variation options for variation {variationId}");

                    return new JsonResult(optionsResponse.Data);
                }
                else
                {
                    // Log the error
                    _logger.LogWarning($"Failed to load variation options. Status: {optionsResponse.StatusCode}, Message: {optionsResponse.Message}");

                    return new JsonResult(new List<VariationOptionDto>());
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                _logger.LogError(ex, $"Exception occurred while fetching variation options for variation {variationId}");

                return new JsonResult(new List<VariationOptionDto>());
            }
        }

        //public async Task<IActionResult> OnPostRemoveVariationAsync(string variationId)
        //{
        //    if (string.IsNullOrEmpty(variationId))
        //        return new JsonResult(new { success = false, message = "Invalid variation ID" });

        //    var response = await _apiResponseHelper.DeleteAsync<bool>($"{Constants.ApiBaseUrl}/api/variation/{variationId}");
        //    return new JsonResult(new
        //    {
        //        success = response.StatusCode == StatusCodeHelper.OK,
        //        message = response.StatusCode == StatusCodeHelper.OK ? "Variation removed successfully" : "Failed to remove variation"
        //    });
        //}


        //Edit Product
        // DTO Models
        public class ProductImageByIdResponse
        {
            public string ImageUrl { get; set; }
            public string ProductId { get; set; }
        }

        public class CategoryVariationResponse
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public List<VariationOptionResponse> Options { get; set; } = new();
        }

        public class VariationOptionResponse
        {
            public string Id { get; set; }
            public string Value { get; set; }
        }

        public class CategoryVariationApiResponse
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string CategoryId { get; set; }
        }



        [BindProperty]
        public List<IFormFile> NewProductImages { get; set; } = new();
        [BindProperty]
        public Dictionary<string, string> VariationUpdates { get; set; } = new();

        [BindProperty]
        public Dictionary<string, string> VariationOptionUpdates { get; set; } = new();
        [BindProperty]
        public ProductForUpdateDto ProductUpdate { get; set; } = new();

        [BindProperty]
        public Dictionary<string, ProductItemForUpdateDto> ProductItemUpdates { get; set; }
        public async Task<IActionResult> OnGetProductDetailsAsync(string productId)
        {
            try
            {
                var response = await _apiResponseHelper.GetAsync<ProductDetailResponseModel>(
                    $"{Constants.ApiBaseUrl}/api/product/detail/{productId}");

                if (response.StatusCode != StatusCodeHelper.OK)
                {
                    return new JsonResult(new { success = false, message = "Failed to load product details" });
                }

                var result = new
                {
                    success = true,
                    product = response.Data
                };

                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product details");
                return new JsonResult(new
                {
                    success = false,
                    message = "An error occurred while loading product details"
                });
            }
        }

        public async Task<IActionResult> OnPostUpdateBasicInfoAsync(string productId)
        {
            try
            {
                if (string.IsNullOrEmpty(ProductUpdate.Name))
                {
                    ModelState.AddModelError("ProductUpdate.Name", "Product name is required");
                }

                if (!ModelState.IsValid)
                {
                    await LoadCategoriesAsync();
                    var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
                    _logger.LogWarning("Validation errors: {Errors}", string.Join("; ", errors));
                    return new JsonResult(new { success = false, message = "Invalid data", errors });
                }

                var productUpdateResponse = await _apiResponseHelper.PatchAsync<bool>(
                    $"{Constants.ApiBaseUrl}/api/product/{productId}",
                    ProductUpdate);

                if (productUpdateResponse.StatusCode != StatusCodeHelper.OK)
                {
                    return new JsonResult(new { success = false, message = "Failed to update product information" });
                }

                return new JsonResult(new { success = true, message = "Product updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product basic information");
                return new JsonResult(new { success = false, message = "An error occurred while updating the product information" });
            }
        }

        public async Task<IActionResult> OnPostUpdateProductItemsAsync(string productId)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new JsonResult(new { success = false, message = "Invalid data" });
                }

                var hasErrors = false;
                foreach (var itemUpdate in ProductItemUpdates)
                {
                    var itemUpdateResponse = await _apiResponseHelper.PatchAsync<bool>(
                        $"{Constants.ApiBaseUrl}/api/productitem/{itemUpdate.Key}",
                        new
                        {
                            QuantityInStock = itemUpdate.Value.QuantityInStock,
                            Price = itemUpdate.Value.Price
                        });

                    if (itemUpdateResponse.StatusCode != StatusCodeHelper.OK)
                    {
                        _logger.LogWarning($"Failed to update product item {itemUpdate.Key}");
                        hasErrors = true;
                    }
                }

                if (hasErrors)
                {
                    return new JsonResult(new { success = false, message = "Some product items failed to update" });
                }

                return new JsonResult(new { success = true, message = "Product items updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product items");
                return new JsonResult(new { success = false, message = "An error occurred while updating the product items" });
            }
        }

        public async Task<IActionResult> OnPostDeleteImageAsync(string imageId)
        {
            try
            {
                var response = await _apiResponseHelper.DeleteAsync<bool>(
                    $"{Constants.ApiBaseUrl}/api/productimage/Delete?imageId={imageId}");

                return new JsonResult(new { success = response.StatusCode == StatusCodeHelper.OK });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product image");
                return new JsonResult(new { success = false });
            }
        }

        public async Task<IActionResult> OnPostDeleteVariationAsync(string variationId)
        {
            try
            {
                var response = await _apiResponseHelper.DeleteAsync<bool>(
                    $"{Constants.ApiBaseUrl}/api/variation/{variationId}");

                return new JsonResult(new { success = response.StatusCode == StatusCodeHelper.OK });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting variation");
                return new JsonResult(new { success = false });
            }
        }

        public async Task<IActionResult> OnGetProductImagesAsync(string productId)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(productId))
                {
                    return new JsonResult(new List<ProductImageByIdResponse>());
                }

                var imagesResponse = await _apiResponseHelper.GetAsync<IList<ProductImageByIdResponse>>(
                    $"{Constants.ApiBaseUrl}/api/productimage/GetImage?productId={productId}");

                if (imagesResponse.StatusCode == StatusCodeHelper.OK && imagesResponse.Data != null)
                {
                    _logger.LogInformation($"Loaded {imagesResponse.Data.Count} images for product {productId}");
                    return new JsonResult(imagesResponse.Data);
                }
                else
                {
                    _logger.LogWarning($"Failed to load product images. Status: {imagesResponse.StatusCode}, Message: {imagesResponse.Message}");
                    return new JsonResult(new List<ProductImageByIdResponse>());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception occurred while fetching images for product {productId}");
                return new JsonResult(new List<ProductImageByIdResponse>());
            }
        }
    }
}