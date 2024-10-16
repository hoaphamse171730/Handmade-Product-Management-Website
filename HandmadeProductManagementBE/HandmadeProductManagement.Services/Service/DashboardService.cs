using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Constants;
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

        public async Task<TotalOrdersByStatusDTO> GetTotalOrdersByStatus()
        {
            var orders = await _unitOfWork.GetRepository<Order>().Entities.ToListAsync();

            return new TotalOrdersByStatusDTO
            {
                Pending = orders.Count(o => o.Status == "Pending"),
                Cancelled = orders.Count(o => o.Status == "Cancelled"),
                AwaitingPayment = orders.Count(o => o.Status == "Awaiting Payment"),
                PaymentFailed = orders.Count(o => o.Status == "Payment Failed"),
                Processing = orders.Count(o => o.Status == "Processing"),
                Delivering = orders.Count(o => o.Status == "Delivering"),
                Shipped = orders.Count(o => o.Status == "Shipped"),
                DeliveryFailed = orders.Count(o => o.Status == "Delivery Failed"),
                OnHold = orders.Count(o => o.Status == "On Hold"),
                DeliveringRetry = orders.Count(o => o.Status == "Delivering Retry"),
                RefundRequested = orders.Count(o => o.Status == "Refund Requested"),
                RefundApproved = orders.Count(o => o.Status == "Refund Approved"),
                RefundDenied = orders.Count(o => o.Status == "Refund Denied"),
                Returning = orders.Count(o => o.Status == "Returning"),
                ReturnFailed = orders.Count(o => o.Status == "Return Failed"),
                Returned = orders.Count(o => o.Status == "Returned"),
                Refunded = orders.Count(o => o.Status == "Refunded"),
                RefundCancelled = orders.Count(o => o.Status == "Refund Cancelled"),
                Closed = orders.Count(o => o.Status == "Closed"),
            };
        }

        public async Task<List<CategoryStockDistributionDTO>> StockDistribution()
        {
            var categoryDistribution = await _unitOfWork.GetRepository<Category>().Entities
                .Include(c => c.Products)
                .Select(c => new CategoryStockDistributionDTO
                {
                    CategoryName = c.Name,
                    ProductCount = c.Products.Count()
                })
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            return categoryDistribution;
        }

        public async Task<decimal> GetTotalSales()
        {
            decimal totalSales = await _unitOfWork.GetRepository<Order>().Entities
                                     .Where(o => o.Status == "Shipped")
                                     .SumAsync(o => o.TotalPrice);

            return totalSales;
        }

        public async Task<List<TopShopDashboardDTO>> GetTop10Shops()
        {
            var topShops = await _unitOfWork.GetRepository<Shop>().Entities
                .OrderByDescending(s => s.Rating)
                .Take(10)
                .Select(s => new TopShopDashboardDTO
                {
                    ShopId = s.Id,
                    Name = s.Name,
                    Rating = s.Rating
                })
                .ToListAsync();

            return topShops;
        }


        public async Task<decimal> GetTotalSaleByShopId(string Id, DashboardDTO dashboardDTO)
        {
            if (!Guid.TryParse(Id, out Guid userId))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), "Invalid shopId");
            }

            var shop = await _unitOfWork.GetRepository<Shop>().Entities.Where(s=>s.Id == Id).FirstOrDefaultAsync();

            if (shop == null)
            {
                throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), "Shop not found");
            }

            if(dashboardDTO.To  < dashboardDTO.From)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), "toDate must be after fromDate");
            }


            decimal totalSales = await _unitOfWork.GetRepository<Order>()
            .Entities
            .Where(order => order.OrderDetails
                .Any(od => od.ProductItem.Product.ShopId == Id)
                && order.OrderDate >= dashboardDTO.From
                && order.OrderDate <= dashboardDTO.To
                && order.Status == "Shipped")
            .SumAsync(order => order.TotalPrice);

                 return totalSales;

        }

        public async Task<IList<TopSellingProducts>> GetTopSellingProducts()
        {
            var topsellingProducts = await _unitOfWork.GetRepository<Product>()
                                                      .Entities
                                                      .Include(p => p.Category)
                                                      .Include(p => p.ProductItems)
                                                      .OrderByDescending(p => p.SoldCount)
                                                      .Take(10)
                                                      .Select(p => new TopSellingProducts
                                                      { 
                                                            Name = p.Name,
                                                            CategoryName = p.Category.Name,
                                                            Price = p.ProductItems.FirstOrDefault() != null ? p.ProductItems.FirstOrDefault().Price : 0,
                                                            ImageUrls = p.ProductImages.Select(pi => pi.Url).ToList(),
                                                            SoldCount = p.SoldCount
                                                      })
                                                      .ToListAsync();

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