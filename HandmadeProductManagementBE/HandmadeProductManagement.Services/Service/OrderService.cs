using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
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
        private readonly IPaymentService _paymentService;

        public OrderService(IUnitOfWork unitOfWork, 
            IStatusChangeService statusChangeService, 
            IOrderDetailService orderDetailService,
            ICartItemService cartItemService,
            IProductService productService,
            IPaymentService paymentService)
        {
            _unitOfWork = unitOfWork;
            _statusChangeService = statusChangeService;
            _orderDetailService = orderDetailService;
            _cartItemService = cartItemService;
            _productService = productService;
            _paymentService = paymentService;
        }

        public async Task<bool> CreateOrderAsync(string userId, CreateOrderDto createOrder)
        {
            ValidateOrder(createOrder);

            var orderRepository = _unitOfWork.GetRepository<Order>();
            var cartItemRepository = _unitOfWork.GetRepository<CartItem>();
            var productItemRepository = _unitOfWork.GetRepository<ProductItem>();
            var productRepository = _unitOfWork.GetRepository<Product>();

            // get cartitems
            var cartItems = await _cartItemService.GetCartItemsByUserIdForOrderCreation(userId);
            if (cartItems.Count == 0)
            {
                throw new BaseException.NotFoundException("empty_cart", "Cart is empty.");
            }

            var promotionRepository = _unitOfWork.GetRepository<Promotion>();
            var categoryRepository = _unitOfWork.GetRepository<Category>();

            // get promotions
            var activePromotions = await promotionRepository.Entities
                .Where(p => (p.Status == "active" || p.Status == "Active") && DateTime.UtcNow >= p.StartDate && DateTime.UtcNow <= p.EndDate)
                .ToListAsync();

            var groupedOrderDetails = new List<GroupedOrderDetail>();

            foreach (var cartItem in cartItems)
            {
                var productItem = await productItemRepository.Entities
                    .FirstOrDefaultAsync(p => p.Id.ToString() == cartItem.ProductItemId && !p.DeletedTime.HasValue);

                if (productItem == null)
                {
                    throw new BaseException.NotFoundException("product_item_not_found", $"Product Item {cartItem.ProductItemId} not found.");
                }

                var product = await productRepository.Entities
                    .FirstOrDefaultAsync(p => p.Id == productItem.ProductId && !p.DeletedTime.HasValue);

                if (product == null)
                {
                    throw new BaseException.NotFoundException("product_not_found", $"Product for Item {productItem.Id} not found.");
                }

                var category = await categoryRepository.Entities
                    .FirstOrDefaultAsync(c => c.Id == product.CategoryId);

                decimal finalPrice = productItem.Price;
                if (category != null && !string.IsNullOrWhiteSpace(category.PromotionId))
                {
                    // check promotion
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

            // group order by shop Id
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
                        Status = "Pending",
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
                            throw new BaseException.BadRequestException("insufficient_stock", $"Product {productItem.Id} has insufficient stock.");
                        }

                        // update quantity in stock
                        productItem.QuantityInStock -= cartItem.ProductQuantity;
                        productItemRepository.Update(productItem);

                        var orderDetail = new OrderDetailForCreationDto
                        {
                            OrderId = order.Id,
                            ProductItemId = productItem.Id,
                            ProductQuantity = cartItem.ProductQuantity,
                            DiscountPrice = groupedDetail.DiscountPrice,
                        };

                        // create order detail
                        await _orderDetailService.Create(orderDetail, userId);

                        // clear cart
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
            catch (Exception)
            {
                _unitOfWork.RollBack();
                throw;
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

            if (!orders.Any())
            {
                throw new BaseException.NotFoundException("not_found", "There is no order.");
            }

            return new PaginatedList<OrderResponseModel>(orders, totalItems, pageNumber, pageSize);
        }

        public async Task<OrderWithDetailDto> GetOrderByIdAsync(string orderId, string userId, string role)
        {
            if (string.IsNullOrWhiteSpace(orderId))
            {
                throw new BaseException.BadRequestException("empty_order_id", "Order ID is required.");
            }

            if (!Guid.TryParse(orderId, out _))
            {
                throw new BaseException.BadRequestException("invalid_order_id_format", "Order ID format is invalid. Example: 123e4567-e89b-12d3-a456-426614174000.");
            }

            var repository = _unitOfWork.GetRepository<Order>();
            var order = await repository.Entities
                .FirstOrDefaultAsync(o => o.Id == orderId && !o.DeletedTime.HasValue);

            if (order == null)
            {
                throw new BaseException.NotFoundException("order_not_found", "Order not found.");
            }

            // If user is not an admin, apply buyer/seller checks
            if (role != "Admin")
            {
                // Check if the user is the buyer of the order
                if (order.CreatedBy != userId)
                {
                    // If the user is not the buyer, check if they are the seller of any product in the order
                    bool isSellerOrder = await _unitOfWork.GetRepository<OrderDetail>().Entities
                        .AnyAsync(od => od.OrderId == orderId && od.ProductItem.CreatedBy == userId && !od.DeletedTime.HasValue);

                    if (!isSellerOrder)
                    {
                        throw new BaseException.ForbiddenException("forbidden", "You have no permission to access this order.");
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

                        // Truy vấn ProductConfiguration để lấy các tùy chọn variation của sản phẩm
                        VariationOptionValues = _unitOfWork.GetRepository<ProductConfiguration>().Entities
                            .Where(pc => pc.ProductItemId == od.ProductItemId)
                            .Select(pc => pc.VariationOption.Value)
                            .ToList()
                    })
                    .ToListAsync();

            if (!orderDetails.Any())
            {
                throw new BaseException.NotFoundException("order_details_not_found", "No order details found for this order.");
            }

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
                throw new BaseException.BadRequestException("invalid_order_id", "Order ID is not in the correct format. Ex: 123e4567-e89b-12d3-a456-426614174000.");
            }

            var repository = _unitOfWork.GetRepository<Order>();
            var existingOrder = await repository.Entities
                .FirstOrDefaultAsync(o => o.Id == orderId && !o.DeletedTime.HasValue);

            if (existingOrder.CreatedBy != userId)
            {
                throw new BaseException.ForbiddenException("forbidden", $"You have no permission to access this resource.");
            }

            if (existingOrder == null)
            {
                throw new BaseException.NotFoundException("order_not_found", "Order not found.");
            }

            if (existingOrder.Status != "Pending" && existingOrder.Status != "Awaiting Payment")
            {
                throw new BaseException.BadRequestException("invalid_order_status", "Order is processing, can not update.");
            }

            if (!string.IsNullOrWhiteSpace(order.Address))
            {
                if (Regex.IsMatch(order.Address, @"[^a-zA-Z0-9\s,\.]"))
                {
                    throw new BaseException.BadRequestException("invalid_address_format", "Address cannot contain special characters except commas and periods.");
                }
                existingOrder.Address = order.Address;
            }

            if (!string.IsNullOrWhiteSpace(order.CustomerName))
            {
                if (Regex.IsMatch(order.CustomerName, @"[^a-zA-Z\s]"))
                {
                    throw new BaseException.BadRequestException("invalid_customer_name_format", "Customer name can only contain letters and spaces.");
                }
                existingOrder.CustomerName = order.CustomerName;
            }

            if (!string.IsNullOrWhiteSpace(order.Phone))
            {
                if (!Regex.IsMatch(order.Phone, @"^0\d{9,10}$"))
                {
                    throw new BaseException.BadRequestException("invalid_phone_format", "Phone number must be numeric, start with 0, and be 10 or 11 digits long.");
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
                throw new BaseException.NotFoundException("user_not_found", "User not found.");
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

            if (!orders.Any())
            {
                throw new BaseException.NotFoundException("not_found", "There is no order.");
            }

            return orders;
        }

        public async Task<IList<OrderResponseModel>> GetOrderByUserIdForAdminAsync(Guid userId)
        {
            var userRepository = _unitOfWork.GetRepository<ApplicationUser>();
            var userExists = await userRepository.Entities
                .AnyAsync(u => u.Id == userId && !u.DeletedTime.HasValue);
            if (!userExists)
            {
                throw new BaseException.NotFoundException("user_not_found", "User not found.");
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

            if (!orders.Any())
            {
                throw new BaseException.NotFoundException("not_found", "There is no order.");
            }

            return orders;
        }
        public async Task<bool> UpdateOrderStatusAsync(string userId, UpdateStatusOrderDto updateStatusOrderDto)
        {
            if (string.IsNullOrWhiteSpace(updateStatusOrderDto.OrderId))
            {
                throw new BaseException.BadRequestException("invalid_order_id", "Order ID is required.");
            }

            if (!Guid.TryParse(updateStatusOrderDto.OrderId, out _))
            {
                throw new BaseException.BadRequestException("invalid_order_id_format", "Order ID format is invalid. Example: 123e4567-e89b-12d3-a456-426614174000.");
            }

            if (string.IsNullOrWhiteSpace(updateStatusOrderDto.Status))
            {
                throw new BaseException.BadRequestException("invalid_status", "Status cannot be null or empty.");
            }

            if (updateStatusOrderDto.Status != "Canceled" && updateStatusOrderDto.CancelReasonId != null)
            {
                throw new BaseException.BadRequestException("invalid_input", "CancelReasonId must be null when status is not {Canceled}");
            }

            var repository = _unitOfWork.GetRepository<Order>();
            var existingOrder = await repository.Entities
                .FirstOrDefaultAsync(o => o.Id == updateStatusOrderDto.OrderId && !o.DeletedTime.HasValue);

            if (existingOrder == null)
            {
                throw new BaseException.NotFoundException("order_not_found", "Order not found.");
            }

            if (existingOrder.Status == "Closed")
            {
                throw new BaseException.ErrorException(400, "order_closed", "Order was closed");
            }

            if (updateStatusOrderDto.Status == "Shipped")
            {
                var payment = await _paymentService.GetPaymentByOrderIdAsync(existingOrder.Id);
                if (payment != null && payment.Method == "Offline")
                {
                    await _paymentService.UpdatePaymentStatusAsync(payment.Id, "Completed", userId);
                }
            }

            // Validate Status Flow
            var validStatusTransitions = new Dictionary<string, List<string>>
            {
                { "Pending", new List<string> { "Canceled", "Awaiting Payment" } },
                { "Awaiting Payment", new List<string> { "Canceled", "Processing" } },
                { "Processing", new List<string> { "Delivering" } },
                { "Delivering", new List<string> { "Shipped", "Delivery Failed" } },
                { "Delivery Failed", new List<string> { "On Hold" } },
                { "On Hold", new List<string> { "Delivering Retry", "Refund Requested" } },
                { "Refund Requested", new List<string> { "Refund Denied", "Refund Approve" } },
                { "Refund Approve", new List<string> { "Returning" } },
                { "Returning", new List<string> { "Return Failed", "Returned" } },
                { "Return Failed", new List<string> { "On Hold" } },
                { "Returned", new List<string> { "Refunded" } },
                { "Refunded", new List<string> { "Closed" } },
                { "Canceled", new List<string> { "Closed" } },
                { "Delivering Retry", new List<string> { "Delivering" } }
            };

            var allValidStatuses = validStatusTransitions.Keys
                .Concat(validStatusTransitions.Values.SelectMany(v => v))
                .Distinct()
                .ToList();

            if (!allValidStatuses.Contains(updateStatusOrderDto.Status))
            {
                throw new BaseException.BadRequestException("invalid_status", $"Status {updateStatusOrderDto.Status} is not a valid status.");
            }

            if (!validStatusTransitions.ContainsKey(existingOrder.Status) ||
                !validStatusTransitions[existingOrder.Status].Contains(updateStatusOrderDto.Status))
            {
                throw new BaseException.BadRequestException("invalid_status_transition", 
                    $"Cannot transition from {existingOrder.Status} to {updateStatusOrderDto.Status}.");
            }

            _unitOfWork.BeginTransaction();

            try
            {
                // Validate if updatedStatus is Canceled
                if (updateStatusOrderDto.Status == "Canceled")
                {
                    if (string.IsNullOrWhiteSpace(updateStatusOrderDto.CancelReasonId))
                    {
                        throw new BaseException.BadRequestException("validation_failed", "CancelReasonId is required when updating status to {Canceled}.");
                    }

                    var cancelReason = await _unitOfWork.GetRepository<CancelReason>().GetByIdAsync(updateStatusOrderDto.CancelReasonId);

                    if (cancelReason == null)
                    {
                        throw new BaseException.NotFoundException("not_found", $"Cancel Reason not found. {existingOrder.CancelReasonId}");
                    }

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
                            .FirstOrDefaultAsync(p => p.Id == detail.ProductItemId && !p.DeletedTime.HasValue);

                        if (productItem == null)
                        {
                            throw new BaseException.NotFoundException("product_item_not_found", $"Product Item {detail.ProductItemId} not found.");
                        }

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
                if (updateStatusOrderDto.Status == "Shipped")
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
            var orderRepository = _unitOfWork.GetRepository<Order>();
            var orders = await orderRepository.Entities
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

            if (!orders.Any())
            {
                throw new BaseException.NotFoundException("not_found", "There is no order.");
            }

            return orders;
        }
        private void ValidateOrder(CreateOrderDto order)
        {
            if (string.IsNullOrWhiteSpace(order.Address))
            {
                throw new BaseException.BadRequestException("invalid_address", "Address cannot be null or empty.");
            }

            if (Regex.IsMatch(order.Address, @"[^a-zA-Z0-9\s,\.]"))
            {
                throw new BaseException.BadRequestException("invalid_address_format", "Address cannot contain special characters except commas and periods.");
            }

            if (string.IsNullOrWhiteSpace(order.CustomerName))
            {
                throw new BaseException.BadRequestException("invalid_customer_name", "Customer name cannot be null or empty.");
            }

            if (Regex.IsMatch(order.CustomerName, @"[^a-zA-Z\s]"))
            {
                throw new BaseException.BadRequestException("invalid_customer_name_format", "Customer name can only contain letters and spaces.");
            }

            if (string.IsNullOrWhiteSpace(order.Phone))
            {
                throw new BaseException.BadRequestException("invalid_phone", "Phone number cannot be null or empty.");
            }

            if (!Regex.IsMatch(order.Phone, @"^0\d{9,10}$"))
            {
                throw new BaseException.BadRequestException("invalid_phone_format", "Phone number must be numeric, start with 0, and be 10 or 11 digits long.");
            }
        }

    }
}