using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Store;
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
        public List<Shop> TopShops { get; set; }
        public List<string> TypeDistributionLabels { get; set; }
        public List<int> TypeDistributionData { get; set; }
        public DashboardModel(ApiResponseHelper apiResponseHelper)
        {
            _apiResponseHelper = apiResponseHelper;
        }

        public async Task OnGetAsync()
        {
            var totalSalesResponse = await _apiResponseHelper.GetAsync<BaseResponse<decimal>>("api/sales/total");
            TotalSales = totalSalesResponse.Data;

            // Fetch sales trend data
            //var salesTrendResponse = await _apiResponseHelper.GetAsync<BaseResponse<SalesTrendDto>>("api/sales/trend");
            //SalesTrendDates = salesTrendResponse.Data.Dates;
            //SalesTrendData = salesTrendResponse.Data.Sales;

            // Fetch top 10 most-sale shops
            //var topShopsResponse = await _apiResponseHelper.GetAsync<BaseResponse<List<Shop>>>("api/shops/top");
            //TopShops = topShopsResponse.Data;

            // Fetch type distribution
            //var typeDistributionResponse = await _apiResponseHelper.GetAsync<BaseResponse<TypeDistributionDto>>("api/sales/type-distribution");
            //TypeDistributionLabels = typeDistributionResponse.Data.Labels;
            //TypeDistributionData = typeDistributionResponse.Data.Distribution;
        }
    }
}
