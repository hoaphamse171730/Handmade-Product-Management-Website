using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Services.Interface;
    using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.ModelViews.DashboardModelViews;
using Microsoft.AspNetCore.Mvc;

    namespace HandmadeProductManagementAPI.Controllers
    {

        [Route("api/[controller]")]
        [ApiController]
        public class DashboardController : ControllerBase
        {
            private readonly IDashboardService _dashboardService;

            public  DashboardController(IDashboardService dashboardService)
            {
                _dashboardService = dashboardService;
            }

        [HttpGet("total-orders")]
        public async Task<IActionResult>GetTotalOrders()
        {
            int totalOrders = await _dashboardService.GetTotalOrders();
         
                return Ok(BaseResponse<int>.OkResponse(totalOrders)); ;
            }

        [HttpGet("total-products")]
        public async Task<IActionResult> GetTotalProducts()
        {
            int totalProduct = await _dashboardService.GetTotalProducts();
            
            return Ok(BaseResponse<int>.OkResponse(totalProduct)); ;
        }

        [HttpGet("total-sales")]
        public async Task<IActionResult> GetTotalSales()
        {
            float totalSales = await _dashboardService.GetTotalSales();

            return Ok(BaseResponse<float>.OkResponse(totalSales)); ;
        }

        [HttpGet("top10-shops")]
        public async Task<IActionResult> GetTop10Shops()
        {
           List<Shop> topShops = await _dashboardService.GetTop10Shops();

            return Ok(BaseResponse<List<Shop>>.OkResponse(topShops)); 

        }
        [HttpPost("TotalSaleById")]
        public async Task<IActionResult> TotalSaleByShopId(string Id, DashboardDTO dashboardDTO)
        {
            int totalSale = await _dashboardService.GetTotalSaleByShopId(Id, dashboardDTO);

            return Ok(BaseResponse<float>.OkResponse(totalSale)); 
        }
        [HttpPost("TopSellingProducts")]
        public async Task<IActionResult> TopSellingProducts()
        {
            List<Product> topSellingProducts = await _dashboardService.GetTopSellingProducts();
            return Ok(BaseResponse<List<Product>>.OkResponse(topSellingProducts));
        }
    }
}
