using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Utils;
using HandmadeProductManagement.ModelViews.OrderDetailModelViews;
using HandmadeProductManagement.ModelViews.OrderModelViews;
using HandmadeProductManagement.ModelViews.StatusChangeModelViews;
using HandmadeProductManagement.Repositories.Entity;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace HandmadeProductManagement.Services.Service
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStatusChangeService _statusChangeService;
        private readonly IOrderDetailService _orderDetailService;
        private readonly ICartItemService _cartItemService;
        private readonly IProductService _productService;

        public OrderService(IUnitOfWork unitOfWork,
            IStatusChangeService statusChangeService,
            IOrderDetailService orderDetailService,
            ICartItemService cartItemService,
            IProductService productService)
        {
            _unitOfWork = unitOfWork;
            _statusChangeService = statusChangeService;
            _orderDetailService = orderDetailService;
            _cartItemService = cartItemService;
            _productService = productService;
        }

        public async Task<bool> CreateOrderAsync(string userId, CreateOrderDto createOrder)
        {
            ValidateOrder(createOrder);

            var orderRepository = _unitOfWork.GetRepository<Order>();
            var cartItemRepository = _unitOfWork.GetRepository<CartItem>();
            var productItemRepository = _unitOfWork.GetRepository<ProductItem>();
            var productRepository = _unitOfWork.GetRepository<Product>();

            // Get cart items
            var cartItems = await _cartItemService.GetCartItemsByUserIdForOrderCreation(userId);
            if (cartItems.Count == 0)
            {
                throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageEmptyCart);
            }

            var promotionRepository = _unitOfWork.GetRepository<Promotion>();
            var categoryRepository = _unitOfWork.GetRepository<Category>();

            // Get promotions
            var activePromotions = await promotionRepository.Entities
                .Where(p => (p.Status == Constants.PromotionStatusActive) && DateTime.UtcNow >= p.StartDate && DateTime.UtcNow <= p.EndDate)
                .ToListAsync();

            var groupedOrderDetails = new List<GroupedOrderDetail>();

            foreach (var cartItem in cartItems)
            {
                var productItem = await productItemRepository.Entities
                    .FirstOrDefaultAsync(p => p.Id.ToString() == cartItem.ProductItemId && (!p.DeletedTime.HasValue || p.DeletedBy == null))
                    ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageProductItemNotFound);

                var product = await productRepository.Entities
                    .FirstOrDefaultAsync(p => p.Id == productItem.ProductId && (!p.DeletedTime.HasValue || p.DeletedBy == null))
                    ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageProductNotFound);

                var category = await categoryRepository.Entities
                    .FirstOrDefaultAsync(c => c.Id == product.CategoryId && (!c.DeletedTime.HasValue || c.DeletedBy == null))
                    ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageCategoryNotFound);

                decimal finalPrice = productItem.Price;
                if (category != null && !string.IsNullOrWhiteSpace(category.PromotionId))
                {
                    // Check promotion
                    var applicablePromotion = activePromotions
                        .FirstOrDefault(p => p.Id == category.PromotionId);

                    if (applicablePromotion != null)
                    {
                        finalPrice = productItem.Price - (productItem.Price * applicablePromotion.DiscountRate);
                    }
                }

                groupedOrderDetails.Add(new GroupedOrderDetail
                {
                    ShopId = product.ShopId,
                    CartItem = cartItem,
                    ProductItem = productItem,
                    DiscountPrice = finalPrice,
                });
            }

            // Group order by shop Id
            var groupedByShop = groupedOrderDetails.GroupBy(x => x.ShopId).ToList();

            _unitOfWork.BeginTransaction();

            try
            {
                foreach (var shopGroup in groupedByShop)
                {
                    var totalPrice = shopGroup.Sum(x => x.DiscountPrice * x.CartItem.ProductQuantity);
                    var order = new Order
                    {
                        TotalPrice = (decimal)totalPrice,
                        OrderDate = DateTime.UtcNow,
                        Status = Constants.OrderStatusPending,
                        UserId = Guid.Parse(userId),
                        Address = createOrder.Address,
                        CustomerName = createOrder.CustomerName,
                        Phone = createOrder.Phone,
                        Note = createOrder.Note,
                        CreatedBy = userId,
                        LastUpdatedBy = userId,
                    };

                    await orderRepository.InsertAsync(order);
                    await _unitOfWork.SaveAsync();

                    foreach (var groupedDetail in shopGroup)
                    {
                        var cartItem = groupedDetail.CartItem;
                        var productItem = groupedDetail.ProductItem;

                        if (productItem.QuantityInStock - cartItem.ProductQuantity < 0)
                        {
                            throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInsufficientStock);
                        }

                        // Update quantity in stock
                        productItem.QuantityInStock -= cartItem.ProductQuantity;
                        productItemRepository.Update(productItem);

                        var orderDetail = new OrderDetailForCreationDto
                        {
                            OrderId = order.Id,
                            ProductItemId = productItem.Id,
                            ProductQuantity = cartItem.ProductQuantity,
                            DiscountPrice = groupedDetail.DiscountPrice,
                        };

                        // Create order detail
                        await _orderDetailService.Create(orderDetail, userId);

                        // Clear cart
                        await _cartItemService.DeleteCartItemByIdAsync(cartItem.Id, userId);
                    }

                    await _unitOfWork.SaveAsync();

                    // Create status change
                    var statusChangeDto = new StatusChangeForCreationDto
                    {
                        OrderId = order.Id.ToString(),
                        Status = order.Status
                    };

                    await _statusChangeService.Create(statusChangeDto, userId);
                    await _unitOfWork.SaveAsync();
                }

                _unitOfWork.CommitTransaction();
                return true;
            }
            catch (Exception ex)
            {
                _unitOfWork.RollBack();
                throw new BaseException.CoreException(StatusCodeHelper.ServerError.ToString(), ex.Message);
            }
        }

        public async Task<PaginatedList<OrderResponseModel>> GetOrdersByPageAsync(int pageNumber, int pageSize)
        {
            var repository = _unitOfWork.GetRepository<Order>();
            var orderDetailRepository = _unitOfWork.GetRepository<OrderDetail>();

            var query = repository.Entities.Where(order => !order.DeletedTime.HasValue);

            var totalItems = await query.CountAsync();

            var orders = await query
                .OrderByDescending(order => order.CreatedTime) // Sort by the most recent orders
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(order => new OrderResponseModel
                {
                    Id = order.Id,
                    TotalPrice = order.TotalPrice,
                    OrderDate = order.OrderDate,
                    Status = order.Status,
                    UserId = order.UserId,
                    Address = order.Address,
                    CustomerName = order.CustomerName,
                    Phone = order.Phone,
                    Note = order.Note,
                    CancelReasonId = order.CancelReasonId
                })
                .ToListAsync();

            return new PaginatedList<OrderResponseModel>(orders, totalItems, pageNumber, pageSize);
        }

        public async Task<OrderWithDetailDto> GetOrderByIdAsync(string orderId, string userId, string role)
        {
            if (string.IsNullOrWhiteSpace(orderId))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageEmptyOrderId);
            }

            if (!Guid.TryParse(orderId, out _))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidOrderIdFormat);
            }

            var repository = _unitOfWork.GetRepository<Order>();
            var order = await repository.Entities
                .FirstOrDefaultAsync(o => o.Id == orderId && !o.DeletedTime.HasValue)
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageOrderNotFound);

            // If user is not an admin, apply buyer/seller checks
            if (role != Constants.RoleAdmin)
            {
                // Check if the user is the buyer of the order
                if (order.CreatedBy != userId)
                {
                    // If the user is not the buyer, check if they are the seller of any product in the order
                    bool isSellerOrder = await _unitOfWork.GetRepository<OrderDetail>().Entities
                        .AnyAsync(od => od.OrderId == orderId && od.ProductItem.CreatedBy == userId && !od.DeletedTime.HasValue);

                    if (!isSellerOrder)
                    {
                        throw new BaseException.ForbiddenException(StatusCodeHelper.Forbidden.ToString(), Constants.ErrorMessageForbiddenAccess);
                    }
                }
            }

            // Retrieve order details with product config and variation option value
            var orderDetails = await _unitOfWork.GetRepository<OrderDetail>().Entities
                .Where(od => od.OrderId == orderId && !od.DeletedTime.HasValue)
                .Select(od => new OrderInDetailDto
                {
                    ProductId = od.ProductItem.Product.Id,
                    ProductName = od.ProductItem.Product.Name,
                    ProductQuantity = od.ProductQuantity,
                    DiscountPrice = od.DiscountPrice,

                    // Query ProductConfiguration to get variation options of the product
                    VariationOptionValues = _unitOfWork.GetRepository<ProductConfiguration>().Entities
                        .Where(pc => pc.ProductItemId == od.ProductItemId)
                        .Select(pc => pc.VariationOption.Value)
                        .ToList()
                })
                .ToListAsync();

            return new OrderWithDetailDto
            {
                Id = order.Id,
                TotalPrice = order.TotalPrice,
                OrderDate = order.OrderDate,
                Status = order.Status,
                UserId = order.UserId,
                Address = order.Address,
                CustomerName = order.CustomerName,
                Phone = order.Phone,
                Note = order.Note,
                CancelReasonId = order.CancelReasonId,
                OrderDetails = orderDetails
            };
        }

        public async Task<bool> UpdateOrderAsync(string userId, string orderId, UpdateOrderDto order)
        {
            if (string.IsNullOrWhiteSpace(orderId) || !Guid.TryParse(orderId, out _))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidOrderIdFormat);
            }

            var repository = _unitOfWork.GetRepository<Order>();
            var existingOrder = await repository.Entities
                .FirstOrDefaultAsync(o => o.Id == orderId && !o.DeletedTime.HasValue)
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageOrderNotFound);

            if (existingOrder.CreatedBy != userId)
            {
                throw new BaseException.ForbiddenException(StatusCodeHelper.Forbidden.ToString(), Constants.ErrorMessageForbiddenAccess);
            }

            if (existingOrder.Status != Constants.OrderStatusPending && existingOrder.Status != Constants.OrderStatusAwaitingPayment)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidOrderStatus);
            }

            if (!string.IsNullOrWhiteSpace(order.Address))
            {
                if (Regex.IsMatch(order.Address, @"[^a-zA-Z0-9\s,\.]"))
                {
                    throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidAddressFormat);
                }
                existingOrder.Address = order.Address;
            }

            if (!string.IsNullOrWhiteSpace(order.CustomerName))
            {
                if (Regex.IsMatch(order.CustomerName, @"[^a-zA-Z\s]"))
                {
                    throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidCustomerNameFormat);
                }
                existingOrder.CustomerName = order.CustomerName;
            }

            if (!string.IsNullOrWhiteSpace(order.Phone))
            {
                if (!Regex.IsMatch(order.Phone, @"^0\d{9,10}$"))
                {
                    throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidPhoneFormat);
                }
                existingOrder.Phone = order.Phone;
            }

            if (!string.IsNullOrWhiteSpace(order.Note))
            {
                existingOrder.Note = order.Note;
            }

            existingOrder.LastUpdatedBy = userId;
            existingOrder.LastUpdatedTime = DateTime.UtcNow;

            repository.Update(existingOrder);
            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<IList<OrderByUserDto>> GetOrderByUserIdAsync(Guid userId)
        {
            var userRepository = _unitOfWork.GetRepository<ApplicationUser>();
            var userExists = await userRepository.Entities
                .AnyAsync(u => u.Id == userId && !u.DeletedTime.HasValue);

            if (!userExists)
            {
                throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageUserNotFound);
            }

            var repository = _unitOfWork.GetRepository<Order>();
            var orders = await repository.Entities
                .Where(o => o.UserId == userId && !o.DeletedTime.HasValue)
                .OrderByDescending(o => o.CreatedTime) // Sort orders by CreatedTime in descending order
                .Select(order => new OrderByUserDto
                {
                    Id = order.Id,
                    TotalPrice = order.TotalPrice,
                    OrderDate = order.OrderDate,
                    Status = order.Status,
                    Address = order.Address,
                    CustomerName = order.CustomerName,
                    Phone = order.Phone,
                    Note = order.Note,
                    CancelReasonId = order.CancelReasonId
                }).ToListAsync();

            return orders;
        }

        public async Task<IList<OrderResponseModel>> GetOrderByUserIdForAdminAsync(Guid userId)
        {
            var userRepository = _unitOfWork.GetRepository<ApplicationUser>();
            var userExists = await userRepository.Entities
                .AnyAsync(u => u.Id == userId && !u.DeletedTime.HasValue);

            if (!userExists)
            {
                throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageUserNotFound);
            }

            var repository = _unitOfWork.GetRepository<Order>();
            var orders = await repository.Entities
                .Where(o => o.UserId == userId && !o.DeletedTime.HasValue)
                .OrderByDescending(o => o.CreatedTime) // Sort orders by CreatedTime in descending order
                .Select(order => new OrderResponseModel
                {
                    Id = order.Id,
                    TotalPrice = order.TotalPrice,
                    OrderDate = order.OrderDate,
                    UserId = userId,
                    Status = order.Status,
                    Address = order.Address,
                    CustomerName = order.CustomerName,
                    Phone = order.Phone,
                    Note = order.Note,
                    CancelReasonId = order.CancelReasonId
                }).ToListAsync();

            return orders;
        }

        public async Task<bool> UpdateOrderStatusAsync(string userId, UpdateStatusOrderDto updateStatusOrderDto)
        {
            if (string.IsNullOrWhiteSpace(updateStatusOrderDto.OrderId))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageEmptyOrderId);
            }

            if (!Guid.TryParse(updateStatusOrderDto.OrderId, out _))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidOrderIdFormat);
            }

            if (string.IsNullOrWhiteSpace(updateStatusOrderDto.Status))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidStatus);
            }

            if (updateStatusOrderDto.Status != Constants.OrderStatusCanceled && updateStatusOrderDto.CancelReasonId != null)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(),
                    Constants.ErrorMessageInvalidCancelReason);
            }

            var repository = _unitOfWork.GetRepository<Order>();
            var existingOrder = await repository.Entities
                .FirstOrDefaultAsync(o => o.Id == updateStatusOrderDto.OrderId && !o.DeletedTime.HasValue)
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageOrderNotFound);

            if (existingOrder.Status == Constants.OrderStatusClosed)
            {
                throw new BaseException.ErrorException(400, StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageOrderClosed);
            }

            // Validate Status Flow
            var validStatusTransitions = new Dictionary<string, List<string>>
            {
                { Constants.OrderStatusPending, new List<string> { Constants.OrderStatusCanceled, Constants.OrderStatusAwaitingPayment } },
                { Constants.OrderStatusAwaitingPayment, new List<string> { Constants.OrderStatusCanceled, Constants.OrderStatusProcessing } },
                { Constants.OrderStatusProcessing, new List<string> { Constants.OrderStatusDelivering } },
                { Constants.OrderStatusDelivering, new List<string> { Constants.OrderStatusShipped, Constants.OrderStatusDeliveryFailed } },
                { Constants.OrderStatusDeliveryFailed, new List<string> { Constants.OrderStatusOnHold } },
                { Constants.OrderStatusOnHold, new List<string> { Constants.OrderStatusDeliveringRetry, Constants.OrderStatusRefundRequested } },
                { Constants.OrderStatusRefundRequested, new List<string> { Constants.OrderStatusRefundDenied, Constants.OrderStatusRefundApprove } },
                { Constants.OrderStatusRefundApprove, new List<string> { Constants.OrderStatusReturning } },
                { Constants.OrderStatusReturning, new List<string> { Constants.OrderStatusReturnFailed, Constants.OrderStatusReturned } },
                { Constants.OrderStatusReturnFailed, new List<string> { Constants.OrderStatusOnHold } },
                { Constants.OrderStatusReturned, new List<string> { Constants.OrderStatusRefunded } },
                { Constants.OrderStatusRefunded, new List<string> { Constants.OrderStatusClosed } },
                { Constants.OrderStatusCanceled, new List<string> { Constants.OrderStatusClosed } },
                { Constants.OrderStatusDeliveringRetry, new List<string> { Constants.OrderStatusDelivering } }
            };

            var allValidStatuses = validStatusTransitions.Keys
                .Concat(validStatusTransitions.Values.SelectMany(v => v))
                .Distinct()
                .ToList();

            if (!allValidStatuses.Contains(updateStatusOrderDto.Status))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(),
                    string.Format(Constants.ErrorMessageInvalidStatus, updateStatusOrderDto.Status));
            }

            if (!(validStatusTransitions.ContainsKey(existingOrder.Status) &&
                validStatusTransitions[existingOrder.Status].Contains(updateStatusOrderDto.Status)))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(),
                    string.Format(Constants.ErrorMessageCannotTransition, existingOrder.Status, updateStatusOrderDto.Status));
            }

            _unitOfWork.BeginTransaction();

            try
            {
                // Validate if updatedStatus is Canceled
                if (updateStatusOrderDto.Status == Constants.OrderStatusCanceled)
                {
                    if (string.IsNullOrWhiteSpace(updateStatusOrderDto.CancelReasonId))
                    {
                        throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(),
                            Constants.ErrorMessageCancelReasonRequired);
                    }

                    var cancelReason = await _unitOfWork.GetRepository<CancelReason>().GetByIdAsync(updateStatusOrderDto.CancelReasonId)
                        ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(),
                            string.Format(Constants.ErrorMessageCancelReasonNotFound, existingOrder.CancelReasonId));

                    existingOrder.CancelReasonId = updateStatusOrderDto.CancelReasonId;

                    // Retrieve the order details to update product stock
                    var orderDetailRepository = _unitOfWork.GetRepository<OrderDetail>();
                    var orderDetails = await orderDetailRepository.Entities
                        .Where(od => od.OrderId == updateStatusOrderDto.OrderId && !od.DeletedTime.HasValue)
                        .ToListAsync();

                    var productItemRepository = _unitOfWork.GetRepository<ProductItem>();

                    // Add back the product quantities to the stock
                    foreach (var detail in orderDetails)
                    {
                        var productItem = await productItemRepository.Entities
                            .FirstOrDefaultAsync(p => p.Id == detail.ProductItemId && !p.DeletedTime.HasValue)
                            ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(),
                                string.Format(Constants.ErrorMessageProductItemNotFound, detail.ProductItemId));

                        productItem.QuantityInStock += detail.ProductQuantity;

                        productItemRepository.Update(productItem);
                    }
                }

                // Update order status
                existingOrder.Status = updateStatusOrderDto.Status;
                existingOrder.LastUpdatedBy = userId;
                existingOrder.LastUpdatedTime = DateTime.UtcNow;

                // Create a new status change record after updating the order status
                var statusChangeDto = new StatusChangeForCreationDto
                {
                    OrderId = updateStatusOrderDto.OrderId,
                    Status = updateStatusOrderDto.Status
                };

                repository.Update(existingOrder);
                await _statusChangeService.Create(statusChangeDto, userId);

                // Update product sold count if the new status is "Shipped"
                if (updateStatusOrderDto.Status == Constants.OrderStatusShipped)
                {
                    await _productService.UpdateProductSoldCountAsync(updateStatusOrderDto.OrderId);
                }

                await _unitOfWork.SaveAsync();
                _unitOfWork.CommitTransaction();

                return true;
            }
            catch (Exception)
            {
                _unitOfWork.RollBack();
                throw;
            }
        }

        public async Task<IList<OrderResponseModel>> GetOrdersBySellerUserIdAsync(Guid userId)
        {
            // Validate if the shop exists for the given seller user ID
            var shop = await _unitOfWork.GetRepository<Shop>().Entities
                .FirstOrDefaultAsync(s => s.UserId == userId)
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageShopNotFound);

            // Fetch orders associated with the products from the seller's shop
            var orders = await _unitOfWork.GetRepository<Order>().Entities
                .Where(o => o.OrderDetails.Any(od => od.ProductItem.Product.Shop.UserId == userId && !od.ProductItem.Product.Shop.DeletedTime.HasValue))
                .OrderByDescending(o => o.CreatedTime) // Sort by CreatedTime in descending order
                .Select(order => new OrderResponseModel
                {
                    Id = order.Id,
                    TotalPrice = order.TotalPrice,
                    OrderDate = order.OrderDate,
                    Status = order.Status,
                    UserId = order.UserId,
                    Address = order.Address,
                    CustomerName = order.CustomerName,
                    Phone = order.Phone,
                    Note = order.Note,
                    CancelReasonId = order.CancelReasonId
                }).ToListAsync();

            return orders;
        }

        private void ValidateOrder(CreateOrderDto order)
        {
            if (string.IsNullOrWhiteSpace(order.Address))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidAddress);
            }

            if (Regex.IsMatch(order.Address, @"[^a-zA-Z0-9\s,\.]"))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidAddressFormat);
            }

            if (string.IsNullOrWhiteSpace(order.CustomerName))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidCustomerName);
            }

            if (Regex.IsMatch(order.CustomerName, @"[^a-zA-Z\s]"))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidCustomerNameFormat);
            }

            if (string.IsNullOrWhiteSpace(order.Phone))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidPhone);
            }

            if (!Regex.IsMatch(order.Phone, @"^0\d{9,10}$"))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidPhoneFormat);
            }
        }

    }
}