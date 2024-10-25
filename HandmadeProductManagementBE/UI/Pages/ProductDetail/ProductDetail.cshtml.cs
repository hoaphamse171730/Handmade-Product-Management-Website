using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Store;
using HandmadeProductManagement.ModelViews.ProductDetailModelViews;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Reflection.Metadata;

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

        public ProductDetailResponseModel? productDetail { get; set; }

        public async Task OnGet(string id)
        {
            string productId = id;
            // Gọi API để lấy chi tiết sản phẩm theo id
            var response = await _apiResponseHelper.GetAsync<ProductDetailResponseModel>($"{Constants.ApiBaseUrl}/api/product/detail/{productId}");

            // Gán thông tin sản phẩm cho thuộc tính productDetail
            productDetail = response.Data;
        }
    }
}
