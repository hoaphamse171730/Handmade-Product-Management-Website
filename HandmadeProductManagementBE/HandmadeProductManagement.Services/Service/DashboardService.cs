using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.DashboardModelViews;
using HandmadeProductManagement.ModelViews.ProductModelViews;
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

        public async Task<TotalOrdersByStatusDTO> GetTotalOrdersByStatus()
        {
            var orders = await _unitOfWork.GetRepository<Order>().Entities.ToListAsync();

            return new TotalOrdersByStatusDTO
            {
                Pending = orders.Count(o => o.Status == Constants.OrderStatusPending),
                Cancelled = orders.Count(o => o.Status == Constants.OrderStatusCanceled),
                AwaitingPayment = orders.Count(o => o.Status == Constants.OrderStatusAwaitingPayment),
                PaymentFailed = orders.Count(o => o.Status == Constants.OrderStatusPaymentFailed),
                Processing = orders.Count(o => o.Status == Constants.OrderStatusProcessing),
                Delivering = orders.Count(o => o.Status == Constants.OrderStatusDelivering),
                Shipped = orders.Count(o => o.Status == Constants.OrderStatusShipped),
                DeliveryFailed = orders.Count(o => o.Status == Constants.OrderStatusDeliveryFailed),
                OnHold = orders.Count(o => o.Status == Constants.OrderStatusOnHold),
                DeliveringRetry = orders.Count(o => o.Status == Constants.OrderStatusDeliveringRetry),
                RefundRequested = orders.Count(o => o.Status == Constants.OrderStatusRefundRequested),
                RefundApproved = orders.Count(o => o.Status == Constants.OrderStatusRefundApprove),
                RefundDenied = orders.Count(o => o.Status == Constants.OrderStatusRefundDenied),
                Returning = orders.Count(o => o.Status == Constants.OrderStatusReturning),
                ReturnFailed = orders.Count(o => o.Status == Constants.OrderStatusReturnFailed),
                Returned = orders.Count(o => o.Status == Constants.OrderStatusReturned),
                Refunded = orders.Count(o => o.Status == Constants.OrderStatusRefunded),
                RefundCancelled = orders.Count(o => o.Status == Constants.OrderStatusCanceled),
                Closed = orders.Count(o => o.Status == Constants.OrderStatusClosed),
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
                                     .Where(o => o.Status == Constants.OrderStatusShipped)
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
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidGuidFormat);
            }

            var shop = await _unitOfWork.GetRepository<Shop>()
                .Entities
                .FirstOrDefaultAsync(s => s.Id == Id)
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageShopNotFound);

            if (dashboardDTO.To < dashboardDTO.From)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidDateRange);
            }

            decimal totalSales = await _unitOfWork.GetRepository<Order>()
            .Entities
            .Where(order => order.OrderDetails
                .Any(od => od.ProductItem != null && od.ProductItem.Product != null && od.ProductItem.Product.ShopId == Id)
                && order.OrderDate >= dashboardDTO.From
                && order.OrderDate <= dashboardDTO.To
                && order.Status == Constants.OrderStatusShipped)
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
                                                            Id = p.Id,
                                                            Name = p.Name,
                                                            CategoryName = p.Category != null ? p.Category.Name : "",
                                                            Price = p.ProductItems.FirstOrDefault() != null ? p.ProductItems.FirstOrDefault()!.Price : 0,
                                                            ImageUrl = p.ProductImages.Any()
                                                            ? p.ProductImages.OrderByDescending(pi => pi.CreatedTime).First().Url
                                                            : string.Empty,
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
                                                    Id = p.Id,
                                                    Name = p.Name,
                                                    CategoryName = p.Category != null ? p.Category.Name : "",
                                                    Price = p.ProductItems.FirstOrDefault() != null ? p.ProductItems.FirstOrDefault()!.Price : 0,
                                                    ImageUrl = p.ProductImages.Any()
                                                    ? p.ProductImages.OrderByDescending(pi => pi.CreatedTime).First().Url
                                                    : string.Empty,
                                               })
                                              .ToListAsync();
            return topProducts;
        }
    }
}