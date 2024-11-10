using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Store;
using HandmadeProductManagement.ModelViews.ProductModelViews;
using HandmadeProductManagement.ModelViews.ReplyModelViews;
using HandmadeProductManagement.ModelViews.ReviewModelViews;
using HandmadeProductManagement.ModelViews.ShopModelViews;
using HandmadeProductManagement.ModelViews.UserInfoModelViews;
using HandmadeProductManagement.ModelViews.UserModelViews;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
using System.Net.Http;

namespace UI.Pages.Review
{
    public class IndexModel : PageModel
    {
        private readonly ApiResponseHelper _apiResponseHelper;

        public IndexModel(ApiResponseHelper apiResponseHelper)
        {
            _apiResponseHelper = apiResponseHelper;
        }

        public IList<ReviewModel> Reviews { get; set; } = new List<ReviewModel>();
        public int PageNumber { get; set; } = 1;
        public int TotalPages { get; set; }
        public IList<UserResponseModel> Users { get; set; } = new List<UserResponseModel>();
        public IList<ShopResponseModel> Shops { get; set; } = new List<ShopResponseModel>();

        [TempData]
        public string? StatusMessage { get; set; }
        public ShopResponseModel? CurrentUserShop { get; set; }
        private async Task<ShopResponseModel?> GetCurrentUserShop()
        {
            try
            {
                // Get the current user's shop
                var response = await _apiResponseHelper.GetAsync<ShopResponseModel>($"{Constants.ApiBaseUrl}/api/shop/user");
                if (response.StatusCode == StatusCodeHelper.OK && response.Data != null)
                {
                    return response.Data;
                }
            }
            catch (Exception ex)
            {
                // Log the error if needed
                Console.WriteLine($"Error getting current user shop: {ex.Message}");
            }
            return null;
        }
        public async Task<IActionResult> OnGetAsync(int pageNumber = 1, int pageSize = 10)
        {
            PageNumber = pageNumber;
            CurrentUserShop = await GetCurrentUserShop();
            var response = await _apiResponseHelper.GetAsync<IList<ReviewModel>>($"{Constants.ApiBaseUrl}/api/review/user?pageNumber={pageNumber}&pageSize={pageSize}");
            if (response.StatusCode == StatusCodeHelper.OK && response.Data != null)
            {
                Reviews = response.Data;
                var totalPagesResponse = await _apiResponseHelper.GetAsync<int>($"{Constants.ApiBaseUrl}/api/review/totalpages?pageSize={pageSize}");
                if (totalPagesResponse.StatusCode == StatusCodeHelper.OK)
                {
                    TotalPages = totalPagesResponse.Data;
                }
            }

            await LoadUsersAndShops();
            return Page();
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

        public async Task<ProductDto?> GetProductByIdAsync(string productId)
        {
            try
            {
                var response = await _apiResponseHelper.GetAsync<ProductDto>($"{Constants.ApiBaseUrl}/api/product/{productId}");
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


        private async Task LoadUsersAndShops()
        {
            var userResponse = await _apiResponseHelper.GetAsync<IList<UserResponseModel>>($"{Constants.ApiBaseUrl}/api/users");
            if (userResponse.StatusCode == StatusCodeHelper.OK)
            {
                Users = userResponse.Data ?? new List<UserResponseModel>();
            }

            var shopResponse = await _apiResponseHelper.GetAsync<IList<ShopResponseModel>>($"{Constants.ApiBaseUrl}/api/shop/get-all");
            if (shopResponse.StatusCode == StatusCodeHelper.OK)
            {
                Shops = shopResponse.Data ?? new List<ShopResponseModel>();
            }
        }

        [BindProperty]
        public ReplyModel Reply { get; set; } = new();
        [BindProperty]
        public string EditReplyContent { get; set; } = string.Empty;

        public async Task<IActionResult> OnPostCreateReplyAsync()
        {
            if (string.IsNullOrEmpty(Reply.Content))
            {
                StatusMessage = "Error: Reply content is required.";
                return RedirectToPage("./Index", new { pageNumber = PageNumber });
            }

            try
            {
                // Get current user's shop
                var currentShop = await GetCurrentUserShop();
                if (currentShop == null)
                {
                    StatusMessage = "Error: Could not determine the shop for the current user.";
                    return RedirectToPage("./Index", new { pageNumber = PageNumber });
                }

                // Create reply payload matching the API expectations
                var payload = new
                {
                    content = Reply.Content,
                    reviewId = Reply.ReviewId,
                    shopId = currentShop.Id,
                    date = DateTime.Now
                };

                // Make API call with query parameters instead of body
                var response = await _apiResponseHelper.PostAsync<bool>(
                    $"{Constants.ApiBaseUrl}/api/reply?content={Uri.EscapeDataString(Reply.Content)}&reviewId={Reply.ReviewId}",
                    null  // No body needed since we're using query parameters
                );

                if (response.StatusCode == StatusCodeHelper.OK)
                {
                    StatusMessage = "Reply was created successfully!";
                    return RedirectToPage("./Index", new { pageNumber = PageNumber });
                }

                StatusMessage = "Error: Failed to create reply.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error creating reply: {ex.Message}";
                Console.WriteLine($"Exception details: {ex}");
            }

            return RedirectToPage("./Index", new { pageNumber = PageNumber });
        }

        public async Task<IActionResult> OnPostUpdateReplyAsync(string replyId, string content)
        {
            try
            {
                var response = await _apiResponseHelper.PutAsync<bool>(
                    $"{Constants.ApiBaseUrl}/api/reply/{replyId}?content={Uri.EscapeDataString(content)}"
                );

                if (response.StatusCode == StatusCodeHelper.OK)
                {
                    StatusMessage = "Reply was updated successfully!";
                }
                else
                {
                    StatusMessage = "Error: Failed to update reply.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error updating reply: {ex.Message}";
                Console.WriteLine($"Exception details: {ex}");
            }

            return RedirectToPage("./Index", new { pageNumber = PageNumber });
        }

        public async Task<IActionResult> OnPostDeleteReplyAsync(string replyId)
        {
            try
            {
                var response = await _apiResponseHelper.DeleteAsync<bool>(
                    $"{Constants.ApiBaseUrl}/api/reply/{replyId}/soft-delete"
                );

                if (response.StatusCode == StatusCodeHelper.OK)
                {
                    StatusMessage = "Reply was deleted successfully!";
                    // Explicitly reload the reviews
                    await ReloadReviewsAndUserData();
                }
                else
                {
                    StatusMessage = "Error: Failed to delete reply.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error deleting reply: {ex.Message}";
                Console.WriteLine($"Exception details: {ex}");
            }

            return RedirectToPage("./Index", new { pageNumber = PageNumber });
        }

        private async Task ReloadReviewsAndUserData()
        {
            // Refresh reviews from API after deletion
            var reviewResponse = await _apiResponseHelper.GetAsync<IList<ReviewModel>>(
                $"{Constants.ApiBaseUrl}/api/review?pageNumber={PageNumber}&pageSize=10"
            );

            if (reviewResponse.StatusCode == StatusCodeHelper.OK && reviewResponse.Data != null)
            {
                Reviews = reviewResponse.Data;
                await LoadUsersAndShops();
            }
        }

        public async Task<IActionResult> OnPostEditReviewAsync(string reviewId, string content, int rating)
        {
            try
            {
                var payload = new
                {
                    Content = content,
                    Rating = rating
                };

                var response = await _apiResponseHelper.PutAsync<bool>(
                    $"{Constants.ApiBaseUrl}/api/review/{reviewId}",
                    payload
                );

                if (response.StatusCode == StatusCodeHelper.OK)
                {
                    // Refresh the model after saving to reflect the new data
                    await OnGetAsync();  // Ensure you have a method to reload the updated list of reviews
                    return Page();  // Return Page to refresh the view with updated data
                }

                TempData["ErrorMessage"] = response.Message ?? "Failed to update review.";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating review: {ex.Message}";
                return RedirectToPage();
            }
        }
    }
}
