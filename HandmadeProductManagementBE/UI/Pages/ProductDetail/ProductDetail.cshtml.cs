using Azure;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Store;
using HandmadeProductManagement.ModelViews.CartItemModelViews;
using HandmadeProductManagement.ModelViews.OrderDetailModelViews;
using HandmadeProductManagement.ModelViews.ProductDetailModelViews;
using HandmadeProductManagement.ModelViews.ProductModelViews;
using HandmadeProductManagement.ModelViews.ReviewModelViews;
using HandmadeProductManagement.ModelViews.ShopModelViews;
using HandmadeProductManagement.ModelViews.UserModelViews;
using HandmadeProductManagement.ModelViews.VariationModelViews;
using HandmadeProductManagement.ModelViews.VariationOptionModelViews;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Reflection.Metadata;
using UI.Pages.Cart;
using static Google.Apis.Requests.BatchRequest;

namespace UI.Pages.ProductDetail
{
    public class ProductDetailModel : PageModel
    {
        private readonly ILogger<ProductDetailModel> _logger;
        private readonly ApiResponseHelper _apiResponseHelper;
        public ProductDetailModel(ApiResponseHelper apiResponseHelper)
        {
            _apiResponseHelper = apiResponseHelper ?? throw new ArgumentNullException(nameof(apiResponseHelper));
        }
        public string? ErrorMessage { get; set; }
        public string? ErrorDetail { get; set; }
        public ProductDetailResponseModel? productDetail { get; set; }
        public CartItemForCreationDto? cart { get;set; }
        public IList<VariationDto> Variations { get; set; } = new List<VariationDto>();
        public IList<VariationWithOptionsDto> VariationOptions { get; set; } = new List<VariationWithOptionsDto>();
        public IList<ReviewModel> Reviews { get; set; } = new List<ReviewModel>();
        public int PageNumber { get; set; } = 1;
        public int TotalPages { get; set; }
        public IList<UserResponseModel> Users { get; set; } = new List<UserResponseModel>();
        public IList<ShopResponseModel> Shops { get; set; } = new List<ShopResponseModel>();
        public ShopResponseModel Shop { get; private set; } = new ShopResponseModel();
        public async Task<PageResult> OnGet(string id, int pageNumber = 1, int pageSize = 10)
        {
            string productId = id;
            try
            {
                // Gọi API để lấy chi tiết sản phẩm theo id
                var response = await _apiResponseHelper.GetAsync<ProductDetailResponseModel>($"{Constants.ApiBaseUrl}/api/product/detail/{productId}");

                // Gán thông tin sản phẩm cho thuộc tính productDetail
                productDetail = response.Data;

                Shop = await GetShopById(productDetail?.ShopId);

                if (productDetail != null && productDetail.CategoryId != null)
                {
                    string categoryId = productDetail.CategoryId;

                    // Gọi API để lấy danh sách variations theo categoryId
                    var variationResponse = await _apiResponseHelper.GetAsync<IList<VariationDto>>($"{Constants.ApiBaseUrl}/api/variation/category/{categoryId}");

                    // Gán danh sách variations cho thuộc tính Variations
                    Variations = variationResponse.Data ?? new List<VariationDto>(); // Gán kết quả hoặc một danh sách rỗng nếu null

                    // Khởi tạo danh sách variationsWithOptions
                    var variationsWithOptions = new List<VariationWithOptionsDto>();

                    // Tạo danh sách các tác vụ gọi API cho mỗi variation
                    var tasks = Variations.Select(async variation =>
                    {
                        // Gọi API để lấy variationOptions cho mỗi variation
                        var optionResponse = await _apiResponseHelper.GetAsync<IList<VariationOptionDto>>($"{Constants.ApiBaseUrl}/api/variationoption/variation/{variation.Id}");

                        // Lọc các options chỉ lấy những options thực sự tồn tại trong productItems và có thể tạo nên 1 productItem
                        var filteredOptions = optionResponse.Data
                            .Where(option => productDetail.ProductItems
                                .Any(item => item.Configurations
                                    .Any(config =>
                                        config.VariationName.Equals(variation.Name, StringComparison.OrdinalIgnoreCase) &&
                                        config.OptionName.Equals(option.Value, StringComparison.OrdinalIgnoreCase)
                                    )))

                            // Group by option name to handle duplicates
                            .GroupBy(option => option.Value, StringComparer.OrdinalIgnoreCase) // Group by option name to handle duplicates
                            .Select(group => group.First()) // Select the first option in each group to avoid duplicates
                            .Select(option => new OptionsDto
                            {
                                Id = option.Id,
                                Name = option.Value
                            })
                            .ToList();

                        // Chỉ thêm vào danh sách nếu variation có ít nhất một option phù hợp và có thể tạo productItem
                        if (filteredOptions.Any())
                        {
                            return new VariationWithOptionsDto
                            {
                                Id = variation.Id,
                                Name = variation.Name,
                                Options = filteredOptions
                            };
                        }
                        else
                        {
                            return null; // Trả về null nếu không có option nào khớp và không thể tạo nên productItem
                        }
                    }).Where(task => task != null).ToList(); // Loại bỏ các null results từ task

                    // Chờ tất cả các tác vụ hoàn thành
                    var variationsWithOptionsResults = (await Task.WhenAll(tasks))
                        .Where(variation => variation != null)
                        .ToList();

                    // Gộp các variation có cùng tên
                    variationsWithOptions = variationsWithOptionsResults
                        .GroupBy(v => v.Name, StringComparer.OrdinalIgnoreCase)
                        .Select(group => new VariationWithOptionsDto
                        {
                            Name = group.Key,
                            Id = group.First().Id, // Chọn ID của variation đầu tiên trong nhóm
                            Options = group.SelectMany(v => v.Options).DistinctBy(o => o.Name).ToList() // Kết hợp options và loại bỏ trùng lặp
                        })
                        .ToList();

                    // Gán danh sách variationsWithOptions cho thuộc tính VariationOptions
                    VariationOptions = variationsWithOptions;
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Không thể lấy thông tin chi tiết sản phẩm hoặc categoryID.");
                }

                PageNumber = pageNumber;

                var reviewResponse = await _apiResponseHelper.GetAsync<IList<ReviewModel>>($"{Constants.ApiBaseUrl}/api/review/product/{productId}?pageNumber={pageNumber}&pageSize={pageSize}");

                if (response.StatusCode == StatusCodeHelper.OK && response.Data != null)
                {
                    Reviews = reviewResponse.Data;
                    var totalPagesResponse = await _apiResponseHelper.GetAsync<int>($"{Constants.ApiBaseUrl}/api/review/totalpages?pageSize={pageSize}");
                    if (totalPagesResponse.StatusCode == StatusCodeHelper.OK)
                    {
                        TotalPages = totalPagesResponse.Data;
                    }
                }

                // Fetch all Users
                var userResponse = await _apiResponseHelper.GetAsync<IList<UserResponseModel>>($"{Constants.ApiBaseUrl}/api/users");
                if (userResponse.StatusCode == StatusCodeHelper.OK)
                {
                    Users = userResponse.Data ?? new List<UserResponseModel>(); // Fallback to empty list if null
                }

                // Fetch all Shops
                var shopResponse = await _apiResponseHelper.GetAsync<IList<ShopResponseModel>>($"{Constants.ApiBaseUrl}/api/shop/get-all");
                if (shopResponse.StatusCode == StatusCodeHelper.OK)
                {
                    Shops = shopResponse.Data ?? new List<ShopResponseModel>();  // Fallback to empty list if null;
                }

                return Page();
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
            return Page();
        }

        private async Task<ShopResponseModel> GetShopById(string id)
        {
            var response = await _apiResponseHelper.GetAsync<ShopResponseModel>(Constants.ApiBaseUrl + $"/api/shop/{id}");

            return response.StatusCode == StatusCodeHelper.OK && response.Data != null
                ? response.Data
                : new ShopResponseModel();
        }

        public async Task<UserResponseModel?> GetUserByIdAsync(string userId)
        {
            try
            {
                var response = await _apiResponseHelper.GetAsync<UserResponseModel>($"{Constants.ApiBaseUrl}/api/users/{userId}");
                if (response.StatusCode == StatusCodeHelper.OK && response.Data != null)
                {
                    return response.Data;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user by ID: {ex.Message}");
            }
            return null;
        }

        // Nhận ProductId từ chuỗi truy vấn
        [BindProperty(SupportsGet = true)]
        public string Id { get; set; }

        public List<CartItemGroupDto> cartItemGroups { get; set; } = new List<CartItemGroupDto>();
        [BindProperty]
        public CartItemForCreationDto CartItem { get; set; }
        public async Task<IActionResult> OnPostAsync()
        {
            // Kiểm tra đầu vào
            if (string.IsNullOrEmpty(CartItem.ProductItemId) || CartItem.ProductQuantity <= 0)
            {
                TempData["Message"] = "Please select a valid product and quantity.";
                return RedirectToPage(new { id = Id });
            }

            var GETResponse = await _apiResponseHelper.GetAsync<List<CartItemGroupDto>>($"{Constants.ApiBaseUrl}/api/cartitem");

            cartItemGroups = GETResponse.Data ?? new List<CartItemGroupDto>();

            // Kiểm tra xem sản phẩm đã tồn tại trong giỏ hàng hay chưa
            bool isProductInCart = cartItemGroups
                .SelectMany(group => group.CartItems)
                .Any(item => item.ProductItemId == CartItem.ProductItemId);

            if (isProductInCart)
            {
                TempData["Message"] = "This product is already in the cart. Please update the quantity if needed.";
                return RedirectToPage(new { id = Id });
            }


            // Gửi yêu cầu POST với đối tượng CartItem
            var POSTResponse = await _apiResponseHelper.PostAsync<bool>($"{Constants.ApiBaseUrl}/api/cartitem", CartItem);
            //var response = await _apiHelper.PostAsync<bool>($"{Constants.ApiBaseUrl}/api/promotions", Promotion);


            if (POSTResponse != null && POSTResponse.Data)
            {
                TempData["SuccessMessage"] = "Add successfully.";
                return RedirectToPage(new { id = Id });
            }
            else
            {
                ModelState.AddModelError(string.Empty, POSTResponse?.Message ?? "An error occurred while creating the promotion.");
                return RedirectToPage(new { id = Id });
            }
        }

    }
}
