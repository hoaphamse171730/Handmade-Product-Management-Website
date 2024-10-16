using AutoMapper;
using FluentValidation;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.CartItemModelViews;
using Microsoft.EntityFrameworkCore;

namespace HandmadeProductManagement.Services.Service
{
    public class CartItemService : ICartItemService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<CartItemForCreationDto> _creationValidator;
        private readonly IValidator<CartItemForUpdateDto> _updateValidator;
        private readonly IPromotionService _promotionService;

        public CartItemService(IUnitOfWork unitOfWork, IMapper mapper, IValidator<CartItemForCreationDto> creationValidator, IValidator<CartItemForUpdateDto> updateValidator, IPromotionService promotionService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _creationValidator = creationValidator;
            _updateValidator = updateValidator;
            _promotionService = promotionService;
        }

        public async Task<bool> AddCartItem(CartItemForCreationDto createCartItemDto, string userId)
        {
            var validationResult = await _creationValidator.ValidateAsync(createCartItemDto);
            if (!validationResult.IsValid)
            {
                throw new BaseException.BadRequestException("invalid_cart_item", validationResult.Errors.First().ErrorMessage);
            }

            var productItemRepo = _unitOfWork.GetRepository<ProductItem>();
            var productItem = await productItemRepo.Entities
                                                    .SingleOrDefaultAsync(pi => pi.Id == createCartItemDto.ProductItemId);
            if (productItem == null)
            {
                throw new BaseException.NotFoundException("product_item_not_found", $"ProductItem {createCartItemDto.ProductItemId} not found.");
            }

            if (productItem.CreatedBy == userId)
            {
                throw new BaseException.BadRequestException("cannot_add_own_product", "You cannot add your own product to the cart.");
            }

            if (productItem.QuantityInStock < createCartItemDto.ProductQuantity)
            {
                throw new BaseException.BadRequestException("insufficient_stock", "Not enough quantity in stock.");
            }

            var cartItemRepo = _unitOfWork.GetRepository<CartItem>();
            var existingCartItem = await cartItemRepo.Entities
                .FirstOrDefaultAsync(ci => ci.ProductItemId == productItem.Id && ci.UserId == Guid.Parse(userId) && ci.DeletedTime == null);

            if (existingCartItem != null)
            {
                var newQuantity = existingCartItem.ProductQuantity + createCartItemDto.ProductQuantity;

                // Check if the new quantity exceeds the quantity in stock
                if (newQuantity > productItem.QuantityInStock)
                {
                    throw new BaseException.BadRequestException("insufficient_stock", "Not enough quantity in stock for the updated cart item.");
                }

                existingCartItem.ProductQuantity = newQuantity;
                existingCartItem.LastUpdatedTime = DateTime.UtcNow;
            }
            else
            {
                var cartItem = new CartItem
                {
                    ProductItemId = productItem.Id,
                    ProductQuantity = createCartItemDto.ProductQuantity,
                    UserId = Guid.Parse(userId),
                    CreatedBy = userId,
                    LastUpdatedBy = userId,
                };

                await cartItemRepo.InsertAsync(cartItem);
            }

            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> UpdateCartItem(string cartItemId, CartItemForUpdateDto updateCartItemDto, string userId)
        {
            var validationResult = await _updateValidator.ValidateAsync(updateCartItemDto);
            if (!validationResult.IsValid)
            {
                throw new BaseException.BadRequestException("invalid_cart_item", validationResult.Errors.First().ErrorMessage);
            }

            var cartItemRepo = _unitOfWork.GetRepository<CartItem>();
            var cartItem = await cartItemRepo.Entities
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.UserId == Guid.Parse(userId) && ci.DeletedTime == null);

            if (cartItem == null)
            {
                throw new BaseException.NotFoundException("cart_item_not_found", "Cart item not found.");
            }

            if (cartItem.CreatedBy != userId)
            {
                throw new BaseException.ForbiddenException("forbidden", "You do not have permission to access this resource.");
            }

            if (updateCartItemDto.ProductQuantity.HasValue)
            {
                var productItemRepo = _unitOfWork.GetRepository<ProductItem>();
                var productItem = await productItemRepo.Entities
                    .SingleOrDefaultAsync(pi => pi.Id == cartItem.ProductItemId);

                if (productItem == null)
                {
                    throw new BaseException.NotFoundException("product_item_not_found", $"ProductItem {cartItem.ProductItemId} not found.");
                }

                // Check stock availability
                if (updateCartItemDto.ProductQuantity.Value > productItem.QuantityInStock)
                {
                    throw new BaseException.BadRequestException("invalid_quantity", $"Only {productItem.QuantityInStock} items available.");
                }

                cartItem.ProductQuantity = updateCartItemDto.ProductQuantity.Value;
                cartItem.LastUpdatedBy = userId;
                cartItem.LastUpdatedTime = DateTime.UtcNow;
            }

            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<List<CartItemGroupDto>> GetCartItemsByUserIdAsync(string userId)
        {
            var userIdGuid = Guid.Parse(userId);

            var cartItems = await _unitOfWork.GetRepository<CartItem>()
                .Entities
                .Include(ci => ci.ProductItem)
                    .ThenInclude(pi => pi.Product)
                        .ThenInclude(p => p.Category)
                            .ThenInclude(cat => cat.Promotion)
                .Include(ci => ci.ProductItem.Product.Shop)
                .Where(ci => ci.UserId == userIdGuid && ci.DeletedTime == null)
                .Select(ci => new
                {
                    ci.Id,
                    ci.ProductItemId,
                    ci.ProductQuantity,
                    ShopId = ci.ProductItem.Product.Shop.Id,
                    ShopName = ci.ProductItem.Product.Shop.Name,
                    UnitPrice = ci.ProductItem.Price,
                    DiscountPrice = ci.ProductItem.Price * (1 - (ci.ProductItem.Product.Category.Promotion.Status.Equals("active", StringComparison.OrdinalIgnoreCase) ? ci.ProductItem.Product.Category.Promotion.DiscountRate : 0)),
                    VariationOptionValues = _unitOfWork.GetRepository<ProductConfiguration>().Entities
                        .Where(pc => pc.ProductItemId == ci.ProductItemId)
                        .Select(pc => pc.VariationOption.Value)
                        .ToList()
                })
                .ToListAsync();

            if (!cartItems.Any())
            {
                throw new BaseException.NotFoundException("not_found", "There is nothing in your cart.");
            }

            // Group cart items by ShopId and ShopName
            var cartItemGroups = cartItems
                .GroupBy(ci => new { ci.ShopId, ci.ShopName })
                .Select(group => new CartItemGroupDto
                {
                    ShopId = group.Key.ShopId,
                    ShopName = group.Key.ShopName,
                    CartItems = group.Select(ci => new CartItemDto
                    {
                        Id = ci.Id,
                        ProductItemId = ci.ProductItemId,
                        ProductQuantity = ci.ProductQuantity,
                        UnitPrice = ci.UnitPrice,
                        DiscountPrice = ci.DiscountPrice,
                        TotalPriceEachProduct = ci.DiscountPrice * ci.ProductQuantity,
                        VariationOptionValues = ci.VariationOptionValues
                    }).ToList()
                })
                .ToList();

            return cartItemGroups;
        }

        public async Task<List<CartItem>> GetCartItemsByUserIdForOrderCreation(string userId)
        {
            var userIdGuid = Guid.Parse(userId);

            var cartItems = await _unitOfWork.GetRepository<CartItem>()
                .Entities
                .Include(ci => ci.ProductItem)
                    .ThenInclude(pi => pi.Product)
                        .ThenInclude(p => p.Category)
                            .ThenInclude(cat => cat.Promotion)
                .Where(ci => ci.UserId == userIdGuid && ci.DeletedTime == null)
                .ToListAsync();

            if (!cartItems.Any())
            {
                throw new BaseException.NotFoundException("not_found", "There is nothing in your cart.");
            }

            var cartItemDtos = cartItems.Select(ci =>
            {
                var promotion = ci.ProductItem.Product.Category.Promotion;
                var unitPrice = ci.ProductItem.Price;
                var discountPrice = promotion != null && promotion.Status.Equals("active", StringComparison.OrdinalIgnoreCase)
                    ? unitPrice - (int)(unitPrice * promotion.DiscountRate)
                    : unitPrice;

                return new CartItem
                {
                    Id = ci.Id,
                    ProductItemId = ci.ProductItemId,
                    ProductQuantity = ci.ProductQuantity,
                    UserId = ci.UserId
                };
            }).ToList();

            return cartItemDtos;
        }

        public async Task<bool> DeleteCartItemByIdAsync(string cartItemId, string userId)
        {
            var cartItemRepo = _unitOfWork.GetRepository<CartItem>();
            var cartItem = await cartItemRepo.Entities
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.UserId == Guid.Parse(userId) && ci.DeletedTime == null);

            if (cartItem == null)
            {
                throw new BaseException.NotFoundException("cart_item_not_found", "Cart item not found.");
            }

            if (cartItem.CreatedBy != userId)
            {
                throw new BaseException.ForbiddenException("forbidden", "You do not have permission to access this resource.");
            }

            await cartItemRepo.DeleteAsync(cartItem.Id);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<Decimal> GetTotalCartPrice(string userId)
        {
            if (!Guid.TryParse(userId, out Guid userIdGuid))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), "Invalid userId");
            }

            var cartItems = await _unitOfWork.GetRepository<CartItem>()
                .Entities
                .Include(ci => ci.ProductItem)
                .ThenInclude(pi => pi.Product)
                .ThenInclude(p => p.Category)
                .ThenInclude(cat => cat.Promotion)
                .Where(ci => ci.UserId == userIdGuid && ci.DeletedTime == null)
                .ToListAsync();

            if (cartItems.Count == 0)
            {
                throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), "No items in the cart");
            }

            decimal totalPrice = 0;

            foreach (var cartItem in cartItems)
            {
                var productItemPrice = cartItem.ProductItem.Price;
                var productQuantity = cartItem.ProductQuantity;

                var promotion = cartItem.ProductItem.Product.Category.Promotion;
                decimal discountRate = 1;

                if (promotion != null)
                {
                    await _promotionService.UpdatePromotionStatusByRealtime(promotion.Id);
                    if (promotion.Status.Equals("active", StringComparison.OrdinalIgnoreCase))
                    {
                        discountRate = 1 - (decimal)promotion.DiscountRate;
                    }
                }
                totalPrice += productItemPrice * productQuantity * discountRate;
            }

            return totalPrice;
        }

    }
}
