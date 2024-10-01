using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.ModelViews.DashboardModelViews;


namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface IDashboardService
    {
        Task<int> GetTotalOrders();

        Task<int> GetTotalProducts();

        Task<int> GetTotalSales();

        Task<List<Shop>> GetTop10Shops();

        Task<int> GetTotalSaleByShopId(string Id, DashboardDTO dashboardDTO);

        Task<List<Product>> GetTopSellingProducts();
    }
}
