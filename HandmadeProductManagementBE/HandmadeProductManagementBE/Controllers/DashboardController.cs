using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Services.Interface;
    using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.DashboardModelViews;
using HandmadeProductManagement.ModelViews.ProductModelViews;
using HandmadeProductManagement.ModelViews.ReviewModelViews;
using HandmadeProductManagement.Services.Service;
using Microsoft.AspNetCore.Authorization;
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult>GetTotalOrders()
        {
            int totalOrders = await _dashboardService.GetTotalOrders();
         
                return Ok(BaseResponse<int>.OkResponse(totalOrders)); ;
            }

        [HttpGet("total-products")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetTotalProducts()
        {
            int totalProduct = await _dashboardService.GetTotalProducts();
            
            return Ok(BaseResponse<int>.OkResponse(totalProduct)); ;
        }

        [HttpGet("total-sales")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetTotalSales()
        {
            decimal totalSales = await _dashboardService.GetTotalSales();

            return Ok(BaseResponse<decimal>.OkResponse(totalSales)); ;
        }

        [HttpGet("top10-shops")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetTop10Shops()
        {
           List<Shop> topShops = await _dashboardService.GetTop10Shops();

            return Ok(BaseResponse<List<Shop>>.OkResponse(topShops)); 

        }
        [HttpPost("TotalSaleByShopId")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> TotalSaleByShopId(string Id, DashboardDTO dashboardDTO)
        {
            decimal totalSale = await _dashboardService.GetTotalSaleByShopId(Id, dashboardDTO);

            return Ok(BaseResponse<decimal>.OkResponse(totalSale)); 
        }
        [HttpPost("TopSellingProducts")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> TopSellingProducts()
        {
            List<Product> topSellingProducts = await _dashboardService.GetTopSellingProducts();
            return Ok(BaseResponse<List<Product>>.OkResponse(topSellingProducts));
        }

        [HttpGet("GetTop10NewProduct")]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Top10NewProducts()
        {
            var products = await _dashboardService.GetTop10NewProducts();
            var response = new BaseResponse<IList<ProductForDashboard>>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Reviews retrieved successfully.",
                Data = products
            };
            return Ok(response);
        }
    }
}
