﻿using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Store;
using HandmadeProductManagement.ModelViews.CategoryModelViews;
using HandmadeProductManagement.ModelViews.ProductModelViews;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UI.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApiResponseHelper _apiResponseHelper;

        public IndexModel(ApiResponseHelper apiResponseHelper)
        {
            _apiResponseHelper = apiResponseHelper ?? throw new ArgumentNullException(nameof(apiResponseHelper));
        }

        public List<TopSellingProducts> Top10SellingProducts { get; set; }
        public List<ProductForDashboard> Top10NewProducts { get; set; }
        public List<CategoryDto> Categories { get; set; } = [];
        public string Token { get; set; }

        public void OnGet()
        {


            //var response = await _apiResponseHelper.GetAsync<List<ProductSearchVM>>((Constants.ApiBaseUrl + "/api/product/search"));

            //if (response.StatusCode == StatusCodeHelper.OK && response.Data != null)
            //{
            //    products = response.Data;
            //}
            //else
            //{
            //    ModelState.AddModelError(string.Empty, response.Message ?? "An error occurred while fetching weather forecasts.");
            //}

            //Top10SellingProducts = GetTop10SellingProducts();
            //Top10NewProducts = GetTop10NewProducts();
            //Categories = GetCategories();

            //Token = HttpContext.Session.GetString("Token");

        }

        private List<TopSellingProducts> GetTop10SellingProducts()
        {
            var response = _apiResponseHelper.GetAsync<List<TopSellingProducts>>(Constants.ApiBaseUrl + "/api/dashboard/top-10-selling-products").Result; // Lấy dữ liệu từ API
            if (response.StatusCode == StatusCodeHelper.OK && response.Data != null)
            {
                return response.Data;
            }
            return new List<TopSellingProducts>();
        }

        private List<ProductForDashboard> GetTop10NewProducts()
        {
            var response = _apiResponseHelper.GetAsync<List<ProductForDashboard>>(Constants.ApiBaseUrl + "/api/dashboard/top-10-new-products").Result; // Lấy dữ liệu từ API
            if (response.StatusCode == StatusCodeHelper.OK && response.Data != null)
            {
                return response.Data;
            }
            return new List<ProductForDashboard>();
        }

        private List<CategoryDto> GetCategories()
        {
            var response = _apiResponseHelper.GetAsync<List<CategoryDto>>(Constants.ApiBaseUrl + "/api/category").Result;
            if (response.StatusCode == StatusCodeHelper.OK && response.Data != null)
            {
                return response.Data;
            }
            return new List<CategoryDto>();
        }
    }

}
