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
        public List<IFormFile> ProductImages { get; set; } = new();
        [BindProperty]
        public Dictionary<string, List<VariationOptionDto>> VariationOptions { get; set; } = new();

        public async Task<IActionResult> OnPostCreateProductAsync([FromBody] ProductRequestModel productRequest)
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
                        variationOptionIds = v.VariationOptionIds
                    }),
                    variationCombinations = productRequest.VariationCombinations?.Select(c => new
                    {
                        variationOptionIds = c.VariationOptionIds,
                        price = c.Price,
                        quantityInStock = c.QuantityInStock
                    })
                };

                var createProductResponse = await _apiResponseHelper.PostAsync<bool>(
                    $"{Constants.ApiBaseUrl}/api/product",
                    createProductRequest);

                if (createProductResponse.StatusCode != StatusCodeHelper.OK)
                {
                    _logger.LogError("Failed to create product. API Response: {Message}", createProductResponse.Message);
                    return new JsonResult(new { error = createProductResponse.Message }) { StatusCode = 400 };
                }

                // Get the productId from the response
                string productId = createProductResponse.Data.ToString();

                await OnPostUploadImagesAsync(productId);

                return new JsonResult(new
                {
                    success = true,
                    productId = productId
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

        public async Task<IActionResult> OnPostUploadImagesAsync(string productId)
        {
            try
            {
                // Validate product ID
                if (string.IsNullOrEmpty(productId))
                {
                    return new JsonResult(new { error = "Product ID is required" }) { StatusCode = 400 };
                }

                // Check if images are provided
                if (ProductImages == null || !ProductImages.Any())
                {
                    return new JsonResult(new { error = "No images provided" }) { StatusCode = 400 };
                }

                // List to store uploaded images for the response
                var uploadedImages = new List<ProductImage>();

                foreach (var image in ProductImages)
                {
                    // Use PostFileAsync for each image upload
                    var uploadResponse = await _apiResponseHelper.PostFileAsync<BaseResponse<string>>(
                        $"{Constants.ApiBaseUrl}/api/productimage/upload?productId={productId}", image);

                    // Check if upload was successful
                    if (uploadResponse.StatusCode == StatusCodeHelper.OK && !string.IsNullOrEmpty(uploadResponse.Data.Data))
                    {
                        var productImage = new ProductImage
                        {
                            Url = uploadResponse.Data.Data,  // URL from response
                            ProductId = productId       // Associate with product ID
                        };

                        uploadedImages.Add(productImage); // Add to list for response
                    }
                    else
                    {
                        // Log any upload errors
                        _logger.LogError("Failed to upload image {FileName}. API Response: {Message}", image.FileName, uploadResponse.Message);
                    }
                }

                // Return successful result with list of uploaded images
                return new JsonResult(new { success = true, images = uploadedImages });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error during image upload");
                return new JsonResult(new { error = "An unexpected error occurred while uploading images.", details = ex.Message })
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

        public async Task<IActionResult> OnDeleteProductAsync(string productId)
        {
            try
            {
                var deleteResponse = await _apiResponseHelper.DeleteAsync<bool>($"{Constants.ApiBaseUrl}/api/product/soft-delete/{productId}");

                if (deleteResponse.StatusCode != StatusCodeHelper.OK)
                {
                    _logger.LogError("Failed to delete product. API Response: {Message}", deleteResponse.Message);
                    return new JsonResult(new { error = deleteResponse.Message }) { StatusCode = 400 };
                }

                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error in product deletion");
                return new JsonResult(new { error = "An unexpected error occurred while deleting the product.", details = ex.Message }) { StatusCode = 500 };
            }
        }

        public async Task<IActionResult> OnPatchEditProductAsync(string productId, [FromBody] ProductEditRequestModel productEdit)
        {
            try
            {
                var editPayload = new { name = productEdit.Name, description = productEdit.Description };

                var editResponse = await _apiResponseHelper.PatchAsync<bool>($"{Constants.ApiBaseUrl}/api/product/{productId}", editPayload);

                if (editResponse.StatusCode != StatusCodeHelper.OK)
                {
                    _logger.LogError("Failed to edit product. API Response: {Message}", editResponse.Message);
                    return new JsonResult(new { error = editResponse.Message }) { StatusCode = 400 };
                }

                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error in product editing");
                return new JsonResult(new { error = "An unexpected error occurred while editing the product.", details = ex.Message }) { StatusCode = 500 };
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
        public decimal Price { get; set; }
        public int QuantityInStock { get; set; }
    }

    public class ProductEditRequestModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}