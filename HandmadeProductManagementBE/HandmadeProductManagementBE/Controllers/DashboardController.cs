﻿using HandmadeProductManagement.Contract.Repositories.Entity;
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

        [HttpGet("sales/trend")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetSalesTrend()
        {
            var salesTrendDto = await _dashboardService.GetSalesTrendAsync();
            return Ok(BaseResponse<SalesTrendDto>.OkResponse(salesTrendDto));
        }

        [HttpGet("total-orders-by-status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetTotalOrdersByStatus()
        {
            var ordersByStatus = await _dashboardService.GetTotalOrdersByStatus();
            return Ok(BaseResponse<TotalOrdersByStatusDTO>.OkResponse(ordersByStatus));
        }

        [HttpGet("type-distribution")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> StockDistribution()
        {
            var stockDistribution = await _dashboardService.StockDistribution();
            return Ok(BaseResponse<List<CategoryStockDistributionDTO>>.OkResponse(stockDistribution));
        }

        [HttpGet("total-sales")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetTotalSales()
        {
            decimal totalSales = await _dashboardService.GetTotalSales();

            return Ok(BaseResponse<decimal>.OkResponse(totalSales)); ;
        }

        [HttpGet("top10-shops")]
        public async Task<IActionResult> GetTop10Shops()
        {
            var topShops = await _dashboardService.GetTop10Shops();
            return Ok(BaseResponse<List<TopShopDashboardDTO>>.OkResponse(topShops));
        }

        [HttpGet("top-sales-shops")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetTop10SalesShops()
        {
            var topShops = await _dashboardService.GetTop10ShopsByTotalSalesAsync();
            return Ok(BaseResponse<List<TopShopDto>>.OkResponse(topShops));
        }


        [HttpPost("TotalSaleByShopId")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> TotalSaleByShopId(string Id, DashboardDTO dashboardDTO)
        {
            decimal totalSale = await _dashboardService.GetTotalSaleByShopId(Id, dashboardDTO);

            return Ok(BaseResponse<decimal>.OkResponse(totalSale)); 
        }
        [HttpGet("top-10-selling-products")]
        public async Task<IActionResult> TopSellingProducts()
        {
            var topSellingProducts = await _dashboardService.GetTopSellingProducts();
            return Ok(BaseResponse<IList<TopSellingProducts>>.OkResponse(topSellingProducts));
        }

        [HttpGet("top-10-new-products")]
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
