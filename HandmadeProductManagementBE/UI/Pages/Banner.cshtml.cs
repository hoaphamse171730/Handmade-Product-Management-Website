using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Store;
using HandmadeProductManagement.ModelViews.ProductModelViews;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UI.Pages
{
    public class BannerModel : PageModel
    {
        private readonly ApiResponseHelper _apiResponseHelper;

        public BannerModel(ApiResponseHelper apiResponseHelper)
        {
            _apiResponseHelper = apiResponseHelper ?? throw new ArgumentNullException(nameof(apiResponseHelper));
        }

        public List<TopSellingProducts>? products { get; set; }

        public async void OnGetAsync()
        {
            var response = await _apiResponseHelper.GetAsync<List<TopSellingProducts>>(Constants.ApiBaseUrl + "/api/dashboard/top-10-selling-products");

            if (response.StatusCode == StatusCodeHelper.OK && response.Data != null)
            {
                products = response.Data;
            }
            else
            {
                ModelState.AddModelError(string.Empty, response.Message ?? Constants.ErrorMessageFetchingFailed);
            }
        }
    }
}
