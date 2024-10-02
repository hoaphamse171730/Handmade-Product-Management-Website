using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.ModelViews.DashboardModelViews;
using Microsoft.EntityFrameworkCore;


namespace HandmadeProductManagement.Services.Service
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DashboardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<int> GetTotalOrders()
        {
            return await _unitOfWork.GetRepository<Order>().CountAsync();
        }

        public async Task<int> GetTotalProducts()
        {
            return await _unitOfWork.GetRepository<Product>().CountAsync();
        }

        public async Task<decimal> GetTotalSales()
        {
            decimal totalSales = await _unitOfWork.GetRepository<Order>().Entities
                                     .Where(o => o.Status == "Shipped") 
                                     .SumAsync(o => o.TotalPrice); 

            return totalSales;
        }

        public async Task<List<Shop>> GetTop10Shops()
        {
            var topShops = await _unitOfWork.GetRepository<Shop>().Entities
                .OrderByDescending(s => s.Rating)
                .Take(10).ToListAsync();

            return topShops;
        }


       public async Task<decimal> GetTotalSaleByShopId(string Id, DashboardDTO dashboardDTO)
        {
            decimal totalSales = await _unitOfWork.GetRepository<Order>()
       .Entities
       .Where(order => order.OrderDetails
           .Any(od => od.Product.ShopId == Id) 
           && order.OrderDate >= dashboardDTO.from
           && order.OrderDate <= dashboardDTO.to
           && order.Status == "Shipped") 
       .SumAsync(order => order.TotalPrice);

            return totalSales;
        }

        public async Task<List<Product>> GetTopSellingProducts()
        {
            var topsellingProducts = await _unitOfWork.GetRepository<Product>()
            .Entities
            .OrderByDescending(p => p.SoldCount)
            .Take(10).ToListAsync();

            return topsellingProducts;
        }
    }
}
