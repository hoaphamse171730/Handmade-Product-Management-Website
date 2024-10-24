using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Store;
using HandmadeProductManagement.ModelViews.CategoryModelViews;
using Microsoft.AspNetCore.Mvc;

namespace UI.Pages.Shared
{
    public class CategoryMenuViewComponent : ViewComponent
    {
        private readonly ApiResponseHelper _apiResponseHelper;

        public CategoryMenuViewComponent(ApiResponseHelper apiResponseHelper)
        {
            _apiResponseHelper = apiResponseHelper;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var categories = await GetCategoriesAsync();
            return View(categories);
        }

        private async Task<List<CategoryDto>> GetCategoriesAsync()
        {
            var response = await _apiResponseHelper.GetAsync<List<CategoryDto>>(Constants.ApiBaseUrl + "/api/category");
            if (response.StatusCode == StatusCodeHelper.OK && response.Data != null)
            {
                return response.Data;
            }
            return new List<CategoryDto>();
        }
    }
}
