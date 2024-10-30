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

        public List<VariationDto>? Variations { get; set; }

        [BindProperty]
        public IList<IFormFile> ProductImages { get; set; } = new List<IFormFile>();

        [BindProperty]
        public Dictionary<string, List<VariationOptionDto>> VariationOptions { get; set; } = new();

        public async Task<IActionResult> OnPostUploadImagesAsync(IFormFile file, string productId)
        {
            try
            {
                if (file == null || string.IsNullOrEmpty(productId))
                {
                    _logger.LogError("File or Product ID is missing.");
                    return new JsonResult(new { error = "File or Product ID is missing" }) { StatusCode = 400 };
                }

                _logger.LogInformation("Starting image upload for Product ID: {ProductId} with File: {FileName}", productId, file.FileName);

                var formData = new MultipartFormDataContent();
                var fileContent = new StreamContent(file.OpenReadStream());
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                formData.Add(fileContent, "file", file.FileName);
                formData.Add(new StringContent(productId.ToString()), "productId");

                var response = await _apiResponseHelper.PostMultipartAsync<bool>(
                    $"{Constants.ApiBaseUrl}/api/ProductImage/Upload",
                    formData,
                    file.FileName,    // Pass the file name here
                    productId.ToString()         // Pass the product ID here
                );

                if (response.StatusCode == StatusCodeHelper.OK && response.Data)
                {
                    _logger.LogInformation("Image uploaded successfully for Product ID: {ProductId}", productId);
                    return new JsonResult(new { success = true });
                }

                _logger.LogError("Failed to upload image for Product ID: {ProductId}. API Response: {Message}", productId, response.Message);
                return new JsonResult(new { error = response.Message }) { StatusCode = 500 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error in OnPostUploadImagesAsync for Product ID: {ProductId}", productId);
                return new JsonResult(new { error = "An unexpected error occurred while uploading the image.", details = ex.Message })
                { StatusCode = 500 };
            }
        }

        public async Task<IActionResult> OnPostCreateProductAsync([FromBody] ProductRequestModel productRequest, IList<IFormFile> productImages)
        {
            try
            {
                _logger.LogInformation("Starting product creation process");

                if (!ModelState.IsValid)
                {
                    return new JsonResult(new { error = "Invalid model state" }) { StatusCode = 400 };
                }

                // Create the product
                var createProductRequest = new
                {
                    name = productRequest.Name,
                    categoryId = productRequest.CategoryId,
                    description = productRequest.Description,
                    variations = productRequest.Variations?.Select(v => new 
                    {
                        id = v.Id,
                        categoryId = productRequest.CategoryId,
                        variationOptionIds = v.VariationOptionIds
                    }).ToList(),
                    variationCombinations = productRequest.VariationCombinations?.Select(c => new
                    {
                        variationOptionIds = c.VariationOptionIds,
                        price = c.Price,
                        quantityInStock = c.QuantityInStock
                    }).ToList()
                };

                var createProductResponse = await _apiResponseHelper.PostAsync<bool>(
                    $"{Constants.ApiBaseUrl}/api/product",
                    createProductRequest);

                if (createProductResponse.StatusCode != StatusCodeHelper.OK || !createProductResponse.Data)
                {
                    _logger.LogError("Failed to create product. API Response: {Message}", createProductResponse.Message);
                    return new JsonResult(new { error = createProductResponse.Message }) { StatusCode = 400 };
                }

                // Get the latest product ID
                var userProductsResponse = await _apiResponseHelper.GetAsync<List<ProductOverviewDto>>(
                    $"{Constants.ApiBaseUrl}/api/product/user?pageNumber=1&pageSize=1");

                if (userProductsResponse.StatusCode != StatusCodeHelper.OK ||
                    userProductsResponse.Data == null ||
                    !userProductsResponse.Data.Any())
                {
                    return new JsonResult(new { error = "Failed to retrieve product ID" }) { StatusCode = 500 };
                }

                var newProductId = userProductsResponse.Data.First().Id;

                // Upload images
                if (productImages != null && productImages.Count > 0)
                {
                    foreach (var image in productImages)
                    {
                        var uploadResponse = await OnPostUploadImagesAsync(image, newProductId);
                    }
                }

                return new JsonResult(new
                {
                    success = true,
                    productId = newProductId
                });
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

        public async Task<IActionResult> OnGetVariationsByCategoryAsync(string categoryId)
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

        public async Task<IActionResult> OnGetProductDetailsAsync(string id)
        {
            var response = await _apiResponseHelper.GetAsync<ProductSearchVM>(
                $"{Constants.ApiBaseUrl}/api/product/{id}");

            if (response.StatusCode == StatusCodeHelper.OK && response.Data != null)
            {
                return new JsonResult(response.Data);
            }

            return new JsonResult(new { error = "Product not found" }) { StatusCode = 404 };
        }

        public async Task<IActionResult> OnPostUpdateProductAsync(string id)
        {
            try
            {
                // Read the form data
                var formData = await Request.ReadFormAsync();
                var productRequest = new ProductRequestModel
                {
                    Name = formData["Name"],
                    CategoryId = formData["CategoryId"],
                    Description = formData["Description"],
                    VariationCombinations = JsonSerializer.Deserialize<List<VariationCombinationModel>>(
                        formData["VariationCombinations"])
                };

                // Update product via API
                var updateResponse = await _apiResponseHelper.PatchAsync<bool>(
                    $"{Constants.ApiBaseUrl}/api/product/{id}",
                    productRequest);

                if (updateResponse.StatusCode != StatusCodeHelper.OK)
                {
                    return new JsonResult(new { error = updateResponse.Message }) { StatusCode = 400 };
                }

                // Handle image uploads if any
                if (Request.Form.Files.Count > 0)
                {
                    foreach (var file in Request.Form.Files)
                    {
                        var uploadResponse = await _apiResponseHelper.PostFileAsync<BaseResponse<string>>(
                            $"{Constants.ApiBaseUrl}/api/productimage/upload?productId={id}",
                            file);

                        if (uploadResponse.StatusCode != StatusCodeHelper.OK)
                        {
                            _logger.LogError("Failed to upload image {FileName}", file.FileName);
                        }
                    }
                }

                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product {ProductId}", id);
                return new JsonResult(new { error = "An error occurred while updating the product" })
                { StatusCode = 500 };
            }
        }

        [ValidateAntiForgeryToken]
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

        public async Task<IActionResult> OnPostDeleteProductImageAsync(string imageId)
        {
            try
            {
                var response = await _apiResponseHelper.DeleteAsync<bool>(
                    $"{Constants.ApiBaseUrl}/api/productimage/{imageId}");

                if (response.StatusCode == StatusCodeHelper.OK)
                {
                    return new JsonResult(new { success = true });
                }

                return new JsonResult(new { error = response.Message }) { StatusCode = 400 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product image {ImageId}", imageId);
                return new JsonResult(new { error = "An error occurred while deleting the image" })
                { StatusCode = 500 };
            }
        }

    }

    public class ProductRequestModel
    {
        public string Name { get; set; }
        public string CategoryId { get; set; }
        public string Description { get; set; }
        public List<VariationModel> Variations { get; set; }
        public List<VariationCombinationModel> VariationCombinations { get; set; }
    }

    public class VariationModel
    {
        public string Id { get; set; }
        public List<string> VariationOptionIds { get; set; }
    }

    public class VariationCombinationModel
    {
        public List<string> VariationOptionIds { get; set; }
        public int Price { get; set; }
        public int QuantityInStock { get; set; }
    }

    public class UpdateProductStatusRequest
    {
        [Required]
        public string ProductId { get; set; }

        [Required]
        public bool IsAvailable { get; set; }
    }
}