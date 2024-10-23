using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Store;
using HandmadeProductManagement.ModelViews.CategoryModelViews;
using HandmadeProductManagement.ModelViews.ProductModelViews;
using HandmadeProductManagementBE;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Reflection.Metadata;
using System.Text.Json;

namespace UI.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApiResponseHelper _apiResponseHelper;

        public IndexModel(ApiResponseHelper apiResponseHelper)
        {
            _apiResponseHelper = apiResponseHelper ?? throw new ArgumentNullException(nameof(apiResponseHelper));
        }

        public List<ProductSearchVM>? products { get; set; }

        public List<CategoryDto>? categories { get; set; }

        public async Task OnGetAsync()
        {
            var response = await _apiResponseHelper.GetAsync<List<ProductSearchVM>>((Constants.ApiBaseUrl + "/api/product/search"));

            if (response.StatusCode == StatusCodeHelper.OK && response.Data != null)
            {
                products = response.Data;
            }
            else
            {
                ModelState.AddModelError(string.Empty, response.Message ?? "An error occurred while fetching weather forecasts.");
            }

            
        }
        public async Task OnGetcategoryAsync()
        {
            var categoryresponse = await _apiResponseHelper.GetAsync<List<CategoryDto>>((Constants.ApiBaseUrl + "/api/category"));

            if (categoryresponse.StatusCode == StatusCodeHelper.OK && categoryresponse.Data != null)
            {
                categories = categoryresponse.Data;
            }
            else
            {
                ModelState.AddModelError(string.Empty, categoryresponse.Message ?? "An error occurred while fetching weather forecasts.");
            }
        }
    }
}
