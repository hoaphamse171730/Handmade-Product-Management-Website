using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.ModelViews.DashboardModelViews;
using HandmadeProductManagement.ModelViews.ProductModelViews;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;


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
            //     decimal totalSales = await _unitOfWork.GetRepository<Order>()
            //.Entities
            //.Where(order => order.OrderDetails
            //    .Any(od => od.Product.ShopId == Id)
            //    && order.OrderDate >= dashboardDTO.from
            //    && order.OrderDate <= dashboardDTO.to
            //    && order.Status == "Shipped")
            //.SumAsync(order => order.TotalPrice);

            //     return totalSales;
            return 0;
        }

        public async Task<List<Product>> GetTopSellingProducts()
        {
            var topsellingProducts = await _unitOfWork.GetRepository<Product>()
            .Entities
            .OrderByDescending(p => p.SoldCount)
            .Take(10).ToListAsync();

            return topsellingProducts;
        }

        public async Task<IList<ProductForDashboard>> GetTop10NewProducts()
        {
            var topProducts = await _unitOfWork.GetRepository<Product>()
                                               .Entities
                                               .Include(p => p.Category)
                                               .Include(p => p.ProductItems)
                                               .OrderByDescending(p =>  p.CreatedTime)
                                               .ThenByDescending(p => p.LastUpdatedTime)
                                               .Take(10)
                                               .Select(p => new ProductForDashboard
                                                {
                                                    Name = p.Name,
                                                    CategoryName = p.Category.Name,
                                                    Price = p.ProductItems.FirstOrDefault() != null ? p.ProductItems.FirstOrDefault().Price : 0,
                                                    ImageUrls = p.ProductImages.Select(pi => pi.Url).ToList()
                                               })
                                              .ToListAsync();
            return topProducts;
        }
    }
}