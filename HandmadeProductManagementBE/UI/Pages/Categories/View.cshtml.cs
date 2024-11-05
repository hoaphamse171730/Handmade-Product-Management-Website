using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Store;
using HandmadeProductManagement.ModelViews.CategoryModelViews;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UI.Pages.Categories
{
    public class ViewModel : PageModel
    {
        private readonly ApiResponseHelper _apiHelper;

        public ViewModel(ApiResponseHelper apiHelper)
        {
            _apiHelper = apiHelper;
        }

        public List<CategoryDtoWithDetail> Categories { get; set; } = new List<CategoryDtoWithDetail>();
        public string? ErrorMessage { get; set; }
        public string? ErrorDetail { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool HasNextPage { get; set; } = true;

        public async Task OnGetAsync(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                PageNumber = pageNumber;
                PageSize = pageSize;

                var response = await _apiHelper.GetAsync<List<CategoryDtoWithDetail>>(
                    $"{Constants.ApiBaseUrl}/api/Category/pagedetail?pageNumber={PageNumber}&pageSize={PageSize}");

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
            }
            catch (Exception)
            {
                ErrorMessage = "An unexpected error occurred.";
            }
        }

        public async Task<JsonResult> OnPostCreateCategoryAsync([FromBody] CategoryForCreationDto categoryForCreation)
        {
            try
            {
                var response = await _apiHelper.PostAsync<bool>($"{Constants.ApiBaseUrl}/api/Category", categoryForCreation);

                if (response != null && response.Data)
                {
                    return new JsonResult(new { success = true, message = "Category created successfully." });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "Failed to create category." });
                }
            }
            catch (BaseException.ErrorException ex)
            {
                return new JsonResult(new { success = false, message = ex.ErrorDetail.ErrorMessage?.ToString() });
            }
            catch (Exception)
            {
                return new JsonResult(new { success = false, message = "An unexpected error occurred." });
            }
        }

        public async Task<JsonResult> OnPutUpdateCategoryAsync(string categoryId, [FromBody] CategoryForUpdateDto categoryForUpdate)
        {
            try
            {
                var response = await _apiHelper.PutAsync<bool>($"{Constants.ApiBaseUrl}/api/Category/{categoryId}", categoryForUpdate);

                if (response != null && response.Data)
                {
                    return new JsonResult(new { success = true, message = "Category updated successfully." });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "Failed to update category." });
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


        public async Task<JsonResult> OnDeleteDeleteCategoryAsync(string id)
        {
            try
            {
                var response = await _apiHelper.DeleteAsync<bool>($"{Constants.ApiBaseUrl}/api/Category/{id}");

                if (response != null && response.Data)
                {
                    return new JsonResult(new { success = true, message = "Category deleted successfully." });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "Failed to delete category." });
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


        //public async Task<JsonResult> RestoreCategoryAsync(string id)
        //{
        //    try
        //    {
        //        var response = await _apiHelper.PatchAsync<bool>($"{Constants.ApiBaseUrl}/api/Category/restore/{id}", null);

        //        if (response != null && response.Data)
        //        {
        //            return new JsonResult(new { success = true, message = "Category restored successfully." });
        //        }
        //        else
        //        {
        //            return new JsonResult(new { success = false, message = "Failed to restore category." });
        //        }
        //    }
        //    catch (BaseException.ErrorException ex)
        //    {
        //        return new JsonResult(new { success = false, message = ex.ErrorDetail.ErrorMessage });
        //    }
        //    catch (Exception)
        //    {
        //        return new JsonResult(new { success = false, message = "An unexpected error occurred." });
        //    }
        //}

    }
}
