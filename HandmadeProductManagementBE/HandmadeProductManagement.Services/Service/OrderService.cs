using Google.Apis.Storage.v1.Data;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Utils;
using HandmadeProductManagement.ModelViews.OrderDetailModelViews;
using HandmadeProductManagement.ModelViews.OrderModelViews;
using HandmadeProductManagement.ModelViews.PaymentDetailModelViews;
using HandmadeProductManagement.ModelViews.StatusChangeModelViews;
using HandmadeProductManagement.Repositories.Entity;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace HandmadeProductManagement.Services.Service
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStatusChangeService _statusChangeService;
        private readonly IOrderDetailService _orderDetailService;
        private readonly ICartItemService _cartItemService;
        private readonly IProductService _productService;
        private readonly IProductImageService _productImageService;
        private readonly IPaymentService _paymentService;

        public OrderService(IUnitOfWork unitOfWork,
            IStatusChangeService statusChangeService,
            IOrderDetailService orderDetailService,
            ICartItemService cartItemService,
            IProductService productService,
            IProductImageService productImageService,
            IPaymentService paymentService)
        {
            _unitOfWork = unitOfWork;
            _statusChangeService = statusChangeService;
            _orderDetailService = orderDetailService;
            _cartItemService = cartItemService;
            _productService = productService;
            _productImageService = productImageService;
            _paymentService = paymentService;
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

                    // Call the offline payment creation method
                    if (createOrder.PaymentMethod == Constants.CODMethod)
                    {
                        await _paymentService.CreatePaymentOfflineAsync(userId, order.Id);
                        await UpdateOrderStatusToProcessingAsync(order.Id, userId);
                    }

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

        public async Task<OrderResponseModel> CreateOrderAsyncReturnOrder(string userId, CreateOrderDto createOrder)
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

            OrderResponseModel lastCreatedOrder = new OrderResponseModel();

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

                    lastCreatedOrder.Id = order.Id;
                    lastCreatedOrder.TotalPrice = order.TotalPrice;
                    lastCreatedOrder.Address = order.Address;
                    lastCreatedOrder.OrderDate = order.OrderDate;
                    lastCreatedOrder.Status = order.Status;
                    lastCreatedOrder.UserId = order.UserId;
                    lastCreatedOrder.Address = order.Address;
                    lastCreatedOrder.CustomerName = order.CustomerName;
                    lastCreatedOrder.Phone = order.Phone;
                    lastCreatedOrder.Note = order.Note;
                    


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

                    // Call the offline payment creation method
                    if (createOrder.PaymentMethod == Constants.CODMethod)
                    {
                        await UpdateOrderStatusToProcessingAsync(order.Id, userId);
                    }

                    await _statusChangeService.Create(statusChangeDto, userId);
                    await _unitOfWork.SaveAsync();
                }

                _unitOfWork.CommitTransaction();
                return lastCreatedOrder; // Return the last created order
            }
            catch (Exception ex)
            {
                _unitOfWork.RollBack();
                throw new BaseException.CoreException(StatusCodeHelper.ServerError.ToString(), ex.Message);
            }
        }

        public async Task<IList<OrderResponseModel>> GetOrdersByPageAsync(int pageNumber, int pageSize)
        {
            var repository = _unitOfWork.GetRepository<Order>();
            var orderDetailRepository = _unitOfWork.GetRepository<Order>();
            var userRepository = _unitOfWork.GetRepository<ApplicationUser>();

            // Query to get non-deleted orders
            var query = from order in repository.Entities
                        join user in userRepository.Entities on order.UserId equals user.Id
                        where !order.DeletedTime.HasValue
                        orderby order.CreatedTime descending
                        select new OrderResponseModel
                        {
                            Id = order.Id,
                            TotalPrice = order.TotalPrice,
                            OrderDate = order.OrderDate,
                            Status = order.Status,
                            UserId = order.UserId,
                            Username = user.UserName!,
                            Address = order.Address,
                            CustomerName = order.CustomerName,
                            Phone = order.Phone,
                            Note = order.Note,
                            CancelReasonId = order.CancelReasonId
                        };

            // Apply pagination
            var orders = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return orders;
        }

        public async Task<OrderWithDetailDto> GetOrderByIdAsync(string orderId, string userId, string role)
        {
            if (string.IsNullOrWhiteSpace(orderId))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageEmptyId);
            }

            if (!Guid.TryParse(orderId, out _))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidGuidFormat);
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
                        .AnyAsync(od => od.OrderId == orderId && od.ProductItem != null && od.ProductItem.CreatedBy == userId && !od.DeletedTime.HasValue);

                    if (!isSellerOrder)
                    {
                        throw new BaseException.ForbiddenException(StatusCodeHelper.Forbidden.ToString(), Constants.ErrorMessageForbidden);
                    }
                }
            }

            // Retrieve order details with product config and variation option value
            var orderDetails = await _unitOfWork.GetRepository<OrderDetail>().Entities
                .Where(od => od.OrderId == orderId && !od.DeletedTime.HasValue)
                .Include(od => od.ProductItem!)
                .ThenInclude(pi => pi.Product!) 
                .ThenInclude(p => p.Shop)
                .ToListAsync();

            var orderInDetailDtos = orderDetails.Select(od => new OrderInDetailDto
            {
                ProductId = od.ProductItem != null && od.ProductItem.Product != null ? od.ProductItem.Product.Id : Guid.Empty.ToString(),
                ProductName = od.ProductItem != null && od.ProductItem.Product != null ? od.ProductItem.Product.Name : "",
                ProductQuantity = od.ProductQuantity,
                DiscountPrice = od.DiscountPrice,
                VariationOptionValues = _unitOfWork.GetRepository<ProductConfiguration>().Entities
                    .Where(pc => pc.ProductItemId == od.ProductItemId && pc.VariationOption != null)
                    .Select(pc => pc.VariationOption!.Value)
                    .ToList(),
            }).ToList();

            if (!orderDetails.Any())
            {
                throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), "No order details found for this order.");
            }

            // Fetch product images and set the first image URL
            foreach (var orderDetail in orderInDetailDtos)
            {
                var images = await _productImageService.GetProductImageById(orderDetail.ProductId);
                if (images != null && images.Count > 0)
                {
                    orderDetail.ProductImage = images.First().Url;
                }
            }
            
            var shopName = orderDetails.FirstOrDefault()?.ProductItem?.Product?.Shop?.Name;
            var shopId = orderDetails.FirstOrDefault()?.ProductItem?.Product?.Shop?.Id;

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
                ShopName = shopName,
                ShopId = shopId,
                CustomerId = order.UserId.ToString(),
                Note = order.Note,
                CancelReasonId = order.CancelReasonId,
                OrderDetails = orderInDetailDtos
            };
        }

        public async Task<bool> UpdateOrderAsync(string userId, string orderId, UpdateOrderDto order)
        {
            if (string.IsNullOrWhiteSpace(orderId) || !Guid.TryParse(orderId, out _))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidGuidFormat);
            }

            var repository = _unitOfWork.GetRepository<Order>();
            var existingOrder = await repository.Entities
                .FirstOrDefaultAsync(o => o.Id == orderId && !o.DeletedTime.HasValue) ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), "Order not found.");

            if (existingOrder.CreatedBy != userId)
            {
                throw new BaseException.ForbiddenException(StatusCodeHelper.Forbidden.ToString(), Constants.ErrorMessageForbidden);
            }

            if (existingOrder.Status != Constants.OrderStatusPending)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidOrderStatus);
            }

            if (!string.IsNullOrWhiteSpace(order.Address))
            {
                if (Regex.IsMatch(order.Address, @"[^a-zA-Z0-9\s,\.]"))
                {
                    throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidAddressFormat);
                }
                existingOrder.Address = order.Address.Trim();
            }

            if (!string.IsNullOrWhiteSpace(order.CustomerName))
            {
                if (Regex.IsMatch(order.CustomerName, @"[^a-zA-Z\s]"))
                {
                    throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidCustomerNameFormat);
                }
                existingOrder.CustomerName = order.CustomerName.Trim();
            }

            if (!string.IsNullOrWhiteSpace(order.Phone))
            {
                if (!Regex.IsMatch(order.Phone, @"^0\d{9,10}$"))
                {
                    throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidPhoneFormat);
                }
                existingOrder.Phone = order.Phone.Trim();
            }

            if (order.Note != null)
            {
                existingOrder.Note = order.Note.Trim();
            }

            existingOrder.LastUpdatedBy = userId;
            existingOrder.LastUpdatedTime = DateTime.UtcNow;

            repository.Update(existingOrder);
            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<IList<OrderByUserDto>> GetOrderByUserIdAsync(Guid userId, int pageNumber, int pageSize)
        {
            var userRepository = _unitOfWork.GetRepository<ApplicationUser>();
            var userExists = await userRepository.Entities
                .AnyAsync(u => u.Id == userId && !u.DeletedTime.HasValue);

            if (!userExists)
            {
                throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageUserNotFound);
            }

            var repository = _unitOfWork.GetRepository<Order>();

            // Query to get orders by userId with pagination applied
            var query = repository.Entities
                .Where(o => o.UserId == userId && !o.DeletedTime.HasValue)
                .OrderByDescending(o => o.CreatedTime) // Sort orders by CreatedTime in descending order
                .Skip((pageNumber - 1) * pageSize) // Apply pagination: Skip the previous pages
                .Take(pageSize) // Take only the items for the current page
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
                });

            var orders = await query.ToListAsync();

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
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageEmptyId);
            }

            if (!Guid.TryParse(updateStatusOrderDto.OrderId, out _))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidGuidFormat);
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
                    { Constants.OrderStatusPending, new List<string> { Constants.OrderStatusCanceled, Constants.OrderStatusProcessing } },
                    { Constants.OrderStatusProcessing, new List<string> { Constants.OrderStatusCanceled, Constants.OrderStatusDelivering } },
                    { Constants.OrderStatusDelivering, new List<string> { Constants.OrderStatusShipped, Constants.OrderStatusDeliveryFailed } },
                    { Constants.OrderStatusDeliveryFailed, new List<string> { Constants.OrderStatusOnHold } },
                    { Constants.OrderStatusOnHold, new List<string> { Constants.OrderStatusDeliveringRetry, Constants.OrderStatusRefundRequested, Constants.OrderStatusReturning } },
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

                    var payment = await _paymentService.GetPaymentByOrderIdAsync(existingOrder.Id, userId);
                    if(payment != null && payment.Method == Constants.PaymentMethodOffline)
                    {
                        await _paymentService.UpdatePaymentStatusAsync(payment.Id, Constants.PaymentStatusFailed, userId);
                    }
                }

                if(updateStatusOrderDto.Status == Constants.OrderStatusShipped)
                {
                    var payment = await _paymentService.GetPaymentByOrderIdAsync(existingOrder.Id, userId);
                    if(payment != null && payment.Method == Constants.PaymentMethodOffline)
                    {
                        await _paymentService.UpdatePaymentStatusAsync(payment.Id, Constants.PaymentStatusCompleted, userId);
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

        public async Task<bool> UpdateOrderStatusToProcessingAsync(string orderId, string userId)
        {
            if (string.IsNullOrWhiteSpace(orderId))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageEmptyId);
            }

            if (!Guid.TryParse(orderId, out _))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidGuidFormat);
            }

            var repository = _unitOfWork.GetRepository<Order>();
            var existingOrder = await repository.Entities
                .FirstOrDefaultAsync(o => o.Id == orderId && !o.DeletedTime.HasValue)
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageOrderNotFound);

            if (existingOrder.Status == Constants.OrderStatusClosed)
            {
                throw new BaseException.ErrorException(400, StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageOrderClosed);
            }

            if (existingOrder.Status != Constants.OrderStatusPending)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(),
                    string.Format(Constants.ErrorMessageCannotTransition, existingOrder.Status, Constants.OrderStatusProcessing));
            }

            // Update order status to Processing
            existingOrder.Status = Constants.OrderStatusProcessing;
            existingOrder.LastUpdatedBy = userId;
            existingOrder.LastUpdatedTime = DateTime.UtcNow;

            // Create a new status change record
            var statusChangeDto = new StatusChangeForCreationDto
            {
                OrderId = orderId,
                Status = Constants.OrderStatusProcessing
            };

            repository.Update(existingOrder);
            await _statusChangeService.Create(statusChangeDto, userId);
            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<IList<OrderResponseModel>> GetOrdersBySellerUserIdAsync(Guid userId, string? filter, int pageNumber, int pageSize)
        {
            if (pageNumber <= 0)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidPageNumber);
            }
            if (pageSize <= 0)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidPageSize);
            }

            // Validate if the shop exists for the given seller user ID
            var shop = await _unitOfWork.GetRepository<Shop>().Entities
                .FirstOrDefaultAsync(s => s.UserId == userId)
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageShopNotFound);

            var orderRepository = _unitOfWork.GetRepository<Order>();
            var ordersQuery = orderRepository.Entities
            .Where(o => o.OrderDetails.Any(od =>
                                        od.ProductItem != null
                                        && od.ProductItem.Product != null
                                        && od.ProductItem.Product.Shop != null
                                        && od.ProductItem.Product.Shop.UserId == userId
                                        && !od.ProductItem.Product.Shop.DeletedTime.HasValue))
            .AsQueryable();

            // Apply filtering based on the filter parameter
            if (!string.IsNullOrEmpty(filter) && filter != "All")
            {
                switch (filter)
                {
                    case Constants.OrderStatusPending:
                        ordersQuery = ordersQuery.Where(o => o.Status == Constants.OrderStatusPending);
                        break;
                    case Constants.OrderStatusAwaitingPayment:
                        ordersQuery = ordersQuery.Where(o => o.Status == Constants.OrderStatusAwaitingPayment);
                        break;
                    case Constants.OrderStatusProcessing:
                        ordersQuery = ordersQuery.Where(o => o.Status == Constants.OrderStatusProcessing);
                        break;
                    case Constants.OrderStatusDelivering:
                        ordersQuery = ordersQuery.Where(o => new[] { Constants.OrderStatusDeliveryFailed, Constants.OrderStatusDelivering, Constants.OrderStatusOnHold, Constants.OrderStatusDeliveringRetry }.Contains(o.Status));
                        break;
                    case Constants.OrderStatusShipped:
                        ordersQuery = ordersQuery.Where(o => o.Status == Constants.OrderStatusShipped);
                        break;
                    case Constants.OrderStatusCanceled:
                        ordersQuery = ordersQuery.Where(o => o.Status == Constants.OrderStatusCanceled);
                        break;
                    case Constants.OrderStatusRefunded:
                        ordersQuery = ordersQuery.Where(o => new[] { Constants.OrderStatusRefundRequested, Constants.OrderStatusRefundDenied, Constants.OrderStatusRefundApprove, Constants.OrderStatusRefunded }.Contains(o.Status));
                        break;
                    default:
                        break;
                }
            }
            ordersQuery = ordersQuery.OrderByDescending(o => o.CreatedTime);


            var totalItems = await ordersQuery.CountAsync();
            var orders = await ordersQuery
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
                }).ToListAsync();

            return orders;
        }

        private void ValidateOrder(CreateOrderDto order)
        {
            if (string.IsNullOrWhiteSpace(order.Address))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidAddress);
            }

            // Allow characters commonly found in Vietnamese addresses (including spaces, commas, periods, and diacritics)
            if (Regex.IsMatch(order.Address, @"[^\p{L}\p{N}\s,\.Đđ]"))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidAddressFormat);
            }

            if (string.IsNullOrWhiteSpace(order.CustomerName))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidCustomerName);
            }

            // Allow characters commonly found in Vietnamese names (including spaces and diacritics)
            if (Regex.IsMatch(order.CustomerName, @"[^\p{L}\sĐđ]"))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidCustomerNameFormat);
            }

            if (string.IsNullOrWhiteSpace(order.Phone))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidPhone);
            }

            // Vietnamese phone number format: starts with 0 followed by 9 to 10 digits
            if (!Regex.IsMatch(order.Phone, @"^0\d{9,10}$"))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidPhoneFormat);
            }
        }

    }
}