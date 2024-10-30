using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.ModelViews.DashboardModelViews;
using HandmadeProductManagement.ModelViews.ProductModelViews;


namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface IDashboardService
    {
        Task<TotalOrdersByStatusDTO> GetTotalOrdersByStatus();

        Task<List<CategoryStockDistributionDTO>> StockDistribution();

        Task<decimal> GetTotalSales();

        Task<List<TopShopDashboardDTO>> GetTop10Shops();

        Task<decimal> GetTotalSaleByShopId(string Id, DashboardDTO dashboardDTO);

        Task<IList<TopSellingProducts>> GetTopSellingProducts();

        Task<IList<ProductForDashboard>> GetTop10NewProducts();
        Task<SalesTrendDto> GetSalesTrendAsync();
        Task<List<TopShopDto>> GetTop10ShopsByTotalSalesAsync();
    }
}
