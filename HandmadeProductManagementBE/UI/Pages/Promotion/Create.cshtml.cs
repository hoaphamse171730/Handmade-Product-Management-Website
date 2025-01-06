using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Store;
using HandmadeProductManagement.ModelViews.PromotionModelViews;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UI.Pages.Promotion
{
    public class CreateModel : PageModel
    {
        private readonly ApiResponseHelper _apiHelper;

        public CreateModel(ApiResponseHelper apiHelper)
        {
            _apiHelper = apiHelper;
        }
        public string? ErrorMessage { get; set; }
        public string? ErrorDetail { get; set; }

        [BindProperty]
        public PromotionForCreationDto Promotion { get; set; }

        public void OnGet()
        {
            // Initialize if necessary
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Page();
                }

                var response = await _apiHelper.PostAsync<bool>($"{Constants.ApiBaseUrl}/api/promotions", Promotion);

                if (response != null && response.Data)
                {
                    TempData["SuccessMessage"] = "Promotion created successfully.";
                    return RedirectToPage("Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, response?.Message ?? "An error occurred while creating the promotion.");
                    return Page();
                }
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
    }
}
