﻿using AutoMapper;
using FluentValidation;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.CartItemModelViews;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

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
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), validationResult.Errors.First().ErrorMessage);
            }

            var productItem = await _unitOfWork.GetRepository<ProductItem>().Entities
                                                    .SingleOrDefaultAsync(pi => pi.Id == createCartItemDto.ProductItemId)
                                                    ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageProductItemNotFound);

            if (productItem.CreatedBy == userId)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageCannotAddOwnProduct);
            }

            if (productItem.QuantityInStock < createCartItemDto.ProductQuantity)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInsufficientStock);
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
                    throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInsufficientStockForUpdate);
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
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), validationResult.Errors.First().ErrorMessage);
            }

            var cartItemRepo = _unitOfWork.GetRepository<CartItem>();
            var cartItem = await cartItemRepo.Entities
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.UserId == Guid.Parse(userId) && ci.DeletedTime == null)
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageCartItemNotFound);

            if (cartItem.CreatedBy != userId)
            {
                throw new BaseException.ForbiddenException(StatusCodeHelper.Forbidden.ToString(), Constants.ErrorMessageForbidden);
            }

            if (updateCartItemDto.ProductQuantity.HasValue)
            {
                var productItemRepo = _unitOfWork.GetRepository<ProductItem>();
                var productItem = await productItemRepo.Entities
                    .SingleOrDefaultAsync(pi => pi.Id == cartItem.ProductItemId)
                    ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageProductItemNotFound);

                // Check stock availability
                if (updateCartItemDto.ProductQuantity.Value > productItem.QuantityInStock)
                {
                    throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(),
                        string.Format(Constants.ErrorMessageInsufficientStock, productItem.QuantityInStock));
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
                .Include(ci => ci.ProductItem!)
                    .ThenInclude(pi => pi.Product!)
                        .ThenInclude(p => p.Category!)
                            .ThenInclude(cat => cat.Promotion)
                .Include(ci => ci.ProductItem!.Product!.Shop)
                .Include(ci => ci.ProductItem!.Product!.ProductImages)
                .Where(ci => ci.UserId == userIdGuid
                    && ci.DeletedTime == null
                    && ci.ProductItem != null
                    && ci.ProductItem.Product != null
                    && ci.ProductItem.Product.Shop != null
                    && ci.ProductItem.Product.Category != null)
                .Select(ci => new
                {
                    ci.Id,
                    ci.ProductItemId,
                    ci.ProductQuantity,
                    ci.CreatedTime,
                    ShopId = ci.ProductItem!.Product!.Shop!.Id,
                    ShopName = ci.ProductItem.Product.Shop.Name,
                    UnitPrice = ci.ProductItem.Price,
                    StockQuantity = ci.ProductItem.QuantityInStock,
                    DiscountPrice = ci.ProductItem.Price * (1 - (ci.ProductItem.Product!.Category!.Promotion != null
                        && ci.ProductItem.Product.Category.Promotion.Status.Equals("active", StringComparison.OrdinalIgnoreCase)
                        ? ci.ProductItem.Product.Category.Promotion.DiscountRate : 0)),
                    ProductId = ci.ProductItem.Product.Id,
                    ProductName = ci.ProductItem.Product.Name,
                    ImageUrl = ci.ProductItem.Product.ProductImages
                        .OrderByDescending(pi => pi.CreatedTime)
                        .Select(pi => pi.Url)
                        .FirstOrDefault() ?? string.Empty,
                    VariationOptionValues = _unitOfWork.GetRepository<ProductConfiguration>().Entities
                        .Where(pc => pc.ProductItemId == ci.ProductItemId)
                        .Select(pc => pc.VariationOption!.Value)
                        .ToList(),
                    ExceedsStock = ci.ProductQuantity > ci.ProductItem.QuantityInStock // Flag if quantity exceeds stock
                })
                .OrderByDescending(ci => ci.CreatedTime)
                .ToListAsync();

            var cartItemGroups = cartItems
                .GroupBy(ci => new { ci.ShopId, ci.ShopName })
                .Select(group => new CartItemGroupDto
                {
                    ShopId = group.Key.ShopId,
                    ShopName = group.Key.ShopName,
                    CartItems = group.Select(ci => new CartItemDto
                    {
                        Id = ci.Id,
                        ProductId = ci.ProductId,
                        ProductName = ci.ProductName,
                        ImageUrl = ci.ImageUrl,
                        ProductItemId = ci.ProductItemId,
                        ProductQuantity = ci.ProductQuantity,
                        StockQuantity = ci.StockQuantity,
                        UnitPrice = ci.UnitPrice,
                        DiscountPrice = ci.DiscountPrice,
                        TotalPriceEachProduct = ci.DiscountPrice * ci.ProductQuantity,
                        VariationOptionValues = ci.VariationOptionValues,
                        InStock = !ci.ExceedsStock // Check the flag for stock status
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
                    .ThenInclude(pi => pi!.Product)
                        .ThenInclude(p => p!.Category)
                            .ThenInclude(cat => cat!.Promotion)
                .Where(ci => ci.UserId == userIdGuid && ci.DeletedTime == null)
                .ToListAsync();

            var cartItemDtos = cartItems.Select(ci =>
            {
                var promotion = ci.ProductItem!.Product!.Category!.Promotion;
                var unitPrice = ci.ProductItem.Price;

                var discountPrice = promotion != null && promotion.Status.Equals(Constants.PromotionStatusActive, StringComparison.OrdinalIgnoreCase)
                    ? unitPrice - (int)(unitPrice * promotion.DiscountRate)
                    : unitPrice;

                return new CartItem
                {
                    Id = ci.Id,
                    ProductItemId = ci.ProductItemId,
                    ProductQuantity = ci.ProductQuantity,
                    UserId = ci.UserId, 
                };
            }).ToList();

            return cartItemDtos;
        }

        public async Task<bool> DeleteCartItemByIdAsync(string cartItemId, string userId)
        {
            var cartItemRepo = _unitOfWork.GetRepository<CartItem>();
            var cartItem = await cartItemRepo.Entities
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.UserId == Guid.Parse(userId) && ci.DeletedTime == null)
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageCartItemNotFound);

            if (cartItem.CreatedBy != userId)
            {
                throw new BaseException.ForbiddenException(StatusCodeHelper.Forbidden.ToString(), Constants.ErrorMessageForbidden);
            }

            await cartItemRepo.DeleteAsync(cartItem.Id);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<decimal> GetTotalCartPrice(string userId)
        {
            if (!Guid.TryParse(userId, out Guid userIdGuid))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidGuidFormat);
            }

            var cartItems = await _unitOfWork.GetRepository<CartItem>()
                .Entities
                .Include(ci => ci.ProductItem)
                .ThenInclude(pi => pi!.Product)
                .ThenInclude(p => p!.Category)
                .ThenInclude(cat => cat!.Promotion)
                .Where(ci => ci.UserId == userIdGuid && ci.DeletedTime == null)
                .ToListAsync();

            if (cartItems.Count == 0)
            {
                throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageNoItemsInCart);
            }

            decimal totalPrice = 0;

            foreach (var cartItem in cartItems)
            {
                var productItemPrice = cartItem.ProductItem!.Price;
                var productQuantity = cartItem.ProductQuantity;

                var promotion = cartItem.ProductItem!.Product!.Category!.Promotion;
                decimal discountRate = 1;

                if (promotion != null)
                {
                    await _promotionService.UpdatePromotionStatusByRealtime(promotion.Id);
                    if (promotion.Status.Equals(Constants.PromotionStatusActive, StringComparison.OrdinalIgnoreCase))
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
