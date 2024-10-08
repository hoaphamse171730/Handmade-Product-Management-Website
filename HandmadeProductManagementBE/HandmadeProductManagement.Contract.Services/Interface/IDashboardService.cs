using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.ModelViews.DashboardModelViews;
using HandmadeProductManagement.ModelViews.ProductModelViews;


namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface IDashboardService
    {
        Task<int> GetTotalOrders();

        Task<int> GetTotalProducts();

        Task<decimal> GetTotalSales();

        Task<List<Shop>> GetTop10Shops();

        Task<decimal> GetTotalSaleByShopId(string Id, DashboardDTO dashboardDTO);

        Task<List<Product>> GetTopSellingProducts();

        Task<IList<ProductForDashboard>> GetTop10NewProducts();
    }
}
