using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Store;
using HandmadeProductManagement.ModelViews.CategoryModelViews;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UI.Pages.Categories
{
    public class DeletedModel : PageModel
    {
        private readonly ApiResponseHelper _apiHelper;

        public DeletedModel(ApiResponseHelper apiHelper)
        {
            _apiHelper = apiHelper;
        }

        public List<CategoryDtoWithDetail> Categories { get; set; } = new List<CategoryDtoWithDetail>();
        public string? ErrorMessage { get; set; }
        public string? ErrorDetail { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 2;
        public bool HasNextPage { get; set; } = true;

        public async Task<IActionResult> OnGetAsync(int pageNumber = 1, int pageSize = 2)
        {
            try
            {
                PageNumber = pageNumber;
                PageSize = pageSize;

                var response = await _apiHelper.GetAsync<List<CategoryDtoWithDetail>>(
                    $"{Constants.ApiBaseUrl}/api/Category/GetAllDelete?pageNumber={PageNumber}&pageSize={PageSize}");

                if (response != null && response.Data != null)
                {
                    Categories = response.Data;
                    HasNextPage = Categories.Count == PageSize;
                }
                else
                {
                    Categories = new List<CategoryDtoWithDetail>();
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

        public async Task<JsonResult> OnPatchRestoreCategoryAsync(string id)
        {
            try
            {
                var response = await _apiHelper.PatchAsync<bool>($"{Constants.ApiBaseUrl}/api/Category/restore/{id}", null);

                if (response != null && response.Data)
                {
                    return new JsonResult(new { success = true, message = "Category restored successfully." });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "Failed to restore category." });
                }
            }
            catch (BaseException.ErrorException ex)
            {
                return new JsonResult(new { success = false, message = ex.ErrorDetail.ErrorMessage });
            }
            catch (Exception)
            {
                return new JsonResult(new { success = false, message = "An unexpected error occurred." });
            }
        }
    }
}
