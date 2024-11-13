using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Store;
using HandmadeProductManagement.ModelViews.DashboardModelViews;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UI.Pages.Dashboard
{
    public class DashboardModel : PageModel
    {
        private readonly ApiResponseHelper _apiResponseHelper;

        public decimal TotalSales { get; set; }
        public List<string> SalesTrendDates { get; set; }
        public List<decimal> SalesTrendData { get; set; }
        public List<TopShopDto> TopShops { get; set; }
        public List<string> TypeDistributionLabels { get; set; }
        public List<int> TypeDistributionData { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ErrorDetail { get; set; }
        public DashboardModel(ApiResponseHelper apiResponseHelper)
        {
            _apiResponseHelper = apiResponseHelper;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var totalSalesResponse = await _apiResponseHelper.GetAsync<decimal>($"{Constants.ApiBaseUrl}/api/dashboard/total-sales");
                TotalSales = totalSalesResponse.Data;

                //Fetch sales trend data
                var salesTrendResponse = await _apiResponseHelper.GetAsync<SalesTrendDto>($"{Constants.ApiBaseUrl}/api/dashboard/sales/trend");
                SalesTrendDates = salesTrendResponse.Data.Dates;
                SalesTrendData = salesTrendResponse.Data.Sales;

                // Fetch top 10 most-sale shops
                var topShopsResponse = await _apiResponseHelper.GetAsync<List<TopShopDto>>($"{Constants.ApiBaseUrl}/api/dashboard/top-sales-shops");
                TopShops = topShopsResponse.Data;

                //Fetch type distribution
                var typeDistributionResponse = await _apiResponseHelper.GetAsync<List<CategoryStockDistributionDTO>>($"{Constants.ApiBaseUrl}/api/dashboard/type-distribution");
                TypeDistributionLabels = typeDistributionResponse.Data.Select(x => x.CategoryName).ToList();
                TypeDistributionData = typeDistributionResponse.Data.Select(x => x.ProductCount).ToList();
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
