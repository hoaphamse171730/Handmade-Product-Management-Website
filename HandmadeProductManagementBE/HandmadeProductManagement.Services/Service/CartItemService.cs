using AutoMapper;
using FluentValidation;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Utils;
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
            // Validate the incoming DTO
            var validationResult = await _creationValidator.ValidateAsync(createCartItemDto);
            if (!validationResult.IsValid)
            {
                throw new BaseException.BadRequestException("invalid_cart_item", validationResult.Errors.First().ErrorMessage);
            }

            // Check if the product item exists
            var productItemRepo = _unitOfWork.GetRepository<ProductItem>();
            var productItem = await productItemRepo.Entities
                                                    .SingleOrDefaultAsync(pi => pi.Id == createCartItemDto.ProductItemId);
            if (productItem == null)
            {
                throw new BaseException.NotFoundException("product_item_not_found", $"ProductItem {createCartItemDto.ProductItemId} not found.");
            }

            // Check if the cart item already exists for this user
            var cartItemRepo = _unitOfWork.GetRepository<CartItem>();
            var existingCartItem = await cartItemRepo.Entities
                .FirstOrDefaultAsync(ci => ci.ProductItemId == productItem.Id && ci.UserId == Guid.Parse(userId) && ci.DeletedTime == null);

            if (existingCartItem != null)
            {
                existingCartItem.ProductQuantity += createCartItemDto.ProductQuantity.Value;
                existingCartItem.LastUpdatedTime = CoreHelper.SystemTimeNow;
            }
            else
            {
                var cartItem = new CartItem
                {
                    Id = Guid.NewGuid().ToString(),
                    ProductItemId = productItem.Id,
                    ProductQuantity = createCartItemDto.ProductQuantity.Value,
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
            // Validate the incoming DTO
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

        public async Task<List<CartItemDto>> GetCartItemsByUserIdAsync(string userId)
        {
            var cartItems = await _unitOfWork.GetRepository<CartItem>()
                .Entities
                .Where(ci => ci.UserId == Guid.Parse(userId) && ci.DeletedTime == null)
                .ToListAsync();

            return _mapper.Map<List<CartItemDto>>(cartItems);
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

            await cartItemRepo.DeleteAsync(cartItem.Id);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<Decimal> GetTotalCartPrice(string userId)
        {
            var cartItems = await _unitOfWork.GetRepository<CartItem>()
                .Entities
                .Where(ci => ci.UserId == Guid.Parse(userId) && ci.DeletedTime == null)
                .ToListAsync();

            if (cartItems.Count == 0)
            {
                throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), "No items in the cart");
            }

            decimal totalPrice = 0;

            foreach (var cartItem in cartItems)
            {
                var productItem = await _unitOfWork.GetRepository<ProductItem>()
                    .Entities
                    .SingleOrDefaultAsync(pi => pi.Id == cartItem.ProductItemId);

                if (productItem != null)
                {
                    totalPrice += cartItem.ProductQuantity * productItem.Price;
                }
            }

            return totalPrice;
        }
    }
}
