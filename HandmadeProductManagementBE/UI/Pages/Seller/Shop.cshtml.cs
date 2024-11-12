using GraphQLParser;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Store;
using HandmadeProductManagement.ModelViews.CategoryModelViews;
using HandmadeProductManagement.ModelViews.ProductDetailModelViews;
using HandmadeProductManagement.ModelViews.ProductItemModelViews;
using HandmadeProductManagement.ModelViews.ProductModelViews;
using HandmadeProductManagement.ModelViews.ShopModelViews;
using HandmadeProductManagement.ModelViews.VariationCombinationModelViews;
using HandmadeProductManagement.ModelViews.VariationModelViews;
using HandmadeProductManagement.ModelViews.VariationOptionModelViews;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;
using UI.Pages.Product;

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
        public bool HasNextPage { get; set; } = true;
        public string CurrentFilters { get; set; } = string.Empty;
        [BindProperty]
        public string ShopName { get; set; } = string.Empty;

        [BindProperty]
        public string ShopDescription { get; set; } = string.Empty;
        public List<VariationDto>? Variations { get; set; }
        [BindProperty]
        public ProductForCreationDto NewProduct { get; set; } = new();

        [BindProperty]
        public List<IFormFile> ProductImages { get; set; } = new();
        public string Token { get; set; }

        [BindProperty]
        public VariationForCreationDto NewVariation { get; set; } = new VariationForCreationDto
        {
            Name = string.Empty,      // Initialize with empty string
            CategoryId = string.Empty // Initialize with empty string
        };
        [BindProperty]
        public Dictionary<string, List<VariationOptionDto>> VariationOptions { get; set; } = new();


        public async Task<IActionResult> OnGetAsync(
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
                if (ErrorMessage == "unauthorized") return RedirectToPage("/Login");
            }
            catch (Exception ex)
            {
                ErrorMessage = "An unexpected error occurred.";
            }
            return Page();
        }

        public async Task<IActionResult> OnPostCreateShopAsync()
        {
            try
            {
                var shopCreationDto = new CreateShopDto
                {
                    Name = ShopName,
                    Description = ShopDescription
                };

                Token = HttpContext.Session.GetString("Token");
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);

                var response = await client.PostAsJsonAsync($"{Constants.ApiBaseUrl}/api/shop", shopCreationDto);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var baseResponse = JsonSerializer.Deserialize<BaseResponse<bool>>(content, options);

                    if (baseResponse != null && baseResponse.StatusCode == StatusCodeHelper.OK)
                    {
                        return RedirectToPage("/Seller/Shop");
                    }

                    // Fallback for when the base response does not indicate success
                    ErrorMessage = "Failed to create shop.";
                    ErrorDetail = baseResponse?.Message;
                    ModelState.AddModelError(string.Empty, baseResponse?.Message ?? "Error updating user information.");
                }
                else
                {
                    // Handle error response and extract detail
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(errorContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    // Set the error message to the detail from the error response
                    ErrorMessage = errorResponse?.Detail ?? "An error occurred while creating the shop.";
                    ModelState.AddModelError(string.Empty, ErrorMessage);
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
                _logger.LogError(ex, "Shop creation failed.");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateShopAsync()
        {
            try
            {
                var shopUpdateDto = new CreateShopDto
                {
                    Name = ShopName,
                    Description = ShopDescription
                };

                Token = HttpContext.Session.GetString("Token");
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);

                // Make the PUT request to the API to update the shop
                var response = await client.PutAsJsonAsync($"{Constants.ApiBaseUrl}/api/shop/update", shopUpdateDto);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var baseResponse = JsonSerializer.Deserialize<BaseResponse<bool>>(content, options);

                    if (baseResponse != null && baseResponse.StatusCode == StatusCodeHelper.OK)
                    {
                        return RedirectToPage("/Seller/Shop");
                    }

                    ErrorMessage = "Failed to update shop.";
                    ErrorDetail = baseResponse?.Message;
                    ModelState.AddModelError(string.Empty, baseResponse?.Message ?? "Error updating shop information.");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(errorContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    ErrorMessage = errorResponse?.Detail ?? "An error occurred while updating shop information.";
                    ModelState.AddModelError(string.Empty, ErrorMessage);
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
                _logger.LogError(ex, "Shop update failed.");
            }

            // Return the same page to display validation errors
            return Page();
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
            string? Name,
            string? CategoryId,
            string? Status,
            decimal? MinRating,
            string SortOption,
            bool SortDescending)
        {
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

            var response = await _apiResponseHelper.GetAsync<List<ProductSearchVM>>(
                $"{Constants.ApiBaseUrl}/api/product/search-seller?pageNumber={PageNumber}&pageSize={PageSize}", searchFilter);

            if (response.StatusCode == StatusCodeHelper.OK && response.Data != null)
            {
                Products = response.Data;
                HasNextPage = Products.Count == PageSize;

            }
            else
            {
                ModelState.AddModelError(string.Empty, response.Message ?? "An error occurred while fetching products.");
            }

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

        [BindProperty]
        public List<NewVariationDto> NewVariations { get; set; } = new List<NewVariationDto>();
        public List<VariationDto>? CategoryVariations { get; set; }
        public class NewVariationDto
        {
            public string Name { get; set; } = string.Empty;
            public List<string> Options { get; set; } = new List<string>();
        }
        [BindProperty]
        public ProductForCreationDto ProductCreation { get; set; } = new ProductForCreationDto();

        public async Task<IActionResult> OnPostCreateProductAsync()
        {
            try
            {
                ModelState.Clear();

                // Validate basic fields
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

                // Step 1: Parse variations
                ProductCreation.Variations = Request.Form.Keys
                    .Where(k => k.StartsWith("variation_"))
                    .Select(k => new VariationForProductCreationDto
                    {
                        Id = k.Replace("variation_", ""),
                        VariationOptionIds = Request.Form[k]
                            .ToString()
                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(id => id.Trim())
                            .ToList()
                    })
                    .ToList();

                if (!ProductCreation.Variations.Any())
                {
                    return new JsonResult(new { success = false, message = "At least one variation is required" });
                }

                // Step 2: Parse variation combinations
                ProductCreation.VariationCombinations = ParseVariationCombinations();

                if (!ProductCreation.VariationCombinations.Any())
                {
                    return new JsonResult(new { success = false, message = "At least one variation combination is required" });
                }

                // Ensure images are uploaded before creating the product
                if (ProductImages == null || !ProductImages.Any())
                {
                    return new JsonResult(new { success = false, message = "At least one product image is required" });
                }

                var createProductResponse = await _apiResponseHelper.PostAsync<bool>(
                    $"{Constants.ApiBaseUrl}/api/product",
                    ProductCreation);

                if (createProductResponse.StatusCode != StatusCodeHelper.OK || !createProductResponse.Data)
                {
                    return new JsonResult(new { success = false, message = "Failed to create product" });
                }

                // Handle image uploads if available
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
                        .Select(id => id.Trim())
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
        private async Task HandleImageUploads()
        {
            var latestProductsResponse = await _apiResponseHelper.GetAsync<List<ProductOverviewDto>>(
                $"{Constants.ApiBaseUrl}/api/product/user?pageNumber=1&pageSize=1");

            if (latestProductsResponse.StatusCode == StatusCodeHelper.OK &&
                latestProductsResponse.Data?.Any() == true)
            {
                var latestProductId = latestProductsResponse.Data.First().Id;

                var formData = new MultipartFormDataContent();

                // Thêm các ảnh vào FormData
                foreach (var image in ProductImages)
                {
                    if (image != null && image.Length > 0)
                    {
                        // Create a memory stream to store the file bytes
                        using (var memoryStream = new MemoryStream())
                        {
                            // Copy the file content into the memory stream
                            await image.CopyToAsync(memoryStream);

                            // Convert the memory stream to a byte array
                            var fileBytes = memoryStream.ToArray();

                            // Create ByteArrayContent with the file bytes
                            var fileContent = new ByteArrayContent(fileBytes);

                            // Set the content type (adjust as needed)
                            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");

                            // Add the file to the form data
                            formData.Add(fileContent, "files", image.FileName); // "files" is the expected field name in the API
                        }
                    }
                }

                // Gửi yêu cầu POST để upload ảnh
                var uploadUrl = $"{Constants.ApiBaseUrl}/api/productimage/upload/{latestProductId}"; // API URL for uploading images

                Token = HttpContext.Session.GetString("Token"); // Get the authorization token from the session
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token); // Add authorization header

                var response = await client.PostAsync(uploadUrl, formData); // Send POST request with form data

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Successfully uploaded images for product {latestProductId}");
                }
                else
                {
                    _logger.LogError($"Failed to upload images for product {latestProductId}");
                }
            }
        }

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
        //[BindProperty]
        //public ProductForUpdateDto ProductUpdate { get; set; } = new();

        public List<ProductItemForUpdateDto> ProductItemUpdates { get; set; } = new List<ProductItemForUpdateDto>();
        //public async Task<IActionResult> OnGetProductDetailsAsync(string productId)
        //{
        //    try
        //    {
        //        var response = await _apiResponseHelper.GetAsync<ProductDetailResponseModel>(
        //            $"{Constants.ApiBaseUrl}/api/product/detail/{productId}");

        //        if (response.StatusCode != StatusCodeHelper.OK)
        //        {
        //            return new JsonResult(new { success = false, message = "Failed to load product details" });
        //        }

        //        var result = new
        //        {
        //            success = true,
        //            product = response.Data
        //        };

        //        return new JsonResult(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error loading product details");
        //        return new JsonResult(new
        //        {
        //            success = false,
        //            message = "An error occurred while loading product details"
        //        });
        //    }
        //}

        public async Task<IActionResult> OnPostUpdateBasicInfoAsync([FromBody] JsonElement jsonBody)
        {
            try
            {
                var productId = jsonBody.GetProperty("productId").GetString();
                var productUpdate = jsonBody.GetProperty("productUpdate").Deserialize<ProductForUpdateDto>();

                if (string.IsNullOrEmpty(productId) || productUpdate == null)
                {
                    return new JsonResult(new { success = false, message = "Invalid input data" });
                }

                var updateResponse = await _apiResponseHelper.PatchAsync<bool>(
                    $"{Constants.ApiBaseUrl}/api/product/{productId}",
                    productUpdate);

                if (updateResponse.StatusCode != StatusCodeHelper.OK)
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









        //----------------------------------------------------------------------------------------------------------------------------//
        [BindProperty]
        public ProductForUpdateNewFormatDto ProductUpdate { get; set; } = new ProductForUpdateNewFormatDto();

        public async Task<IActionResult> OnGetProductDetailsAsync(string productId)
        {
            try
            {
                var response = await _apiResponseHelper.GetAsync<ProductForUpdateNewFormatResponseDto>(
                    $"{Constants.ApiBaseUrl}/api/product/updateproductresponse/{productId}");

                if (response.StatusCode == StatusCodeHelper.OK && response.Data != null)
                {
                    return new JsonResult(new
                    {
                        success = true, // Added success flag
                        data = response.Data,
                        message = "Product details retrieved successfully"
                    });
                }

                return new JsonResult(new
                {
                    success = false,
                    statusCode = 404,
                    message = "Failed to fetch product details",
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching product details for edit");
                return new JsonResult(new
                {
                    success = false,
                    statusCode = 500,
                    message = "An error occurred while fetching product details"
                });
            }
        }

        public async Task<IActionResult> OnPostUpdateProductAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new JsonResult(new
                    {
                        success = false,
                        message = "Invalid model state",
                        errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                    });
                }

                var productId = Request.Form["ProductId"].ToString();
                if (string.IsNullOrEmpty(productId))
                {
                    return new JsonResult(new { success = false, message = "Product ID is required" });
                }

                var response = await _apiResponseHelper.PutAsync<bool>(
                    $"{Constants.ApiBaseUrl}/api/product/updateproduct/{productId}",
                    ProductUpdate);

                if (response.StatusCode == StatusCodeHelper.OK && response.Data == true)
                {
                    return new JsonResult(new { success = true, message = "Product updated successfully" });
                }

                return new JsonResult(new
                {
                    success = false,
                    message = response.Message ?? "Failed to update product"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product");
                return new JsonResult(new { success = false, message = "An error occurred while updating the product" });
            }
        }
    }
}
