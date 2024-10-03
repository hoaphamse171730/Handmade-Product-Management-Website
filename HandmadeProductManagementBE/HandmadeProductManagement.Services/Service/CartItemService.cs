using Microsoft.EntityFrameworkCore;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.ModelViews.CartModelViews;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Utils;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Constants;

public class CartItemService : ICartItemService
{
    private readonly IUnitOfWork _unitOfWork;

    public CartItemService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<BaseResponse<bool>> AddCartItem(string cartId, CreateCartItemDto createCartItemDto)
    {
        Console.WriteLine($"Attempting to add item to cart: {cartId}, ProductItem: {createCartItemDto.ProductItemId}");

        if (string.IsNullOrEmpty(createCartItemDto.ProductItemId))
        {
            throw new BaseException.BadRequestException("required_product_item_id", "Product item ID is required.");
        }

        if (!int.TryParse(createCartItemDto.ProductQuantity.ToString(), out int quantity) || quantity < 0)
        {
            throw new BaseException.BadRequestException("invalid_quantity", "Invalid product quantity. Quantity must be a non-negative integer.");
        }

        var cartRepo = _unitOfWork.GetRepository<Cart>();
        var cart = await cartRepo.Entities
            .Include(c => c.CartItems)
            .SingleOrDefaultAsync(c => c.Id == cartId);
        if (cart == null)
        {
            throw new BaseException.NotFoundException("cart_not_found", $"Cart {cartId} not found.");
        }

        var productItemRepo = _unitOfWork.GetRepository<ProductItem>();
        var productItem = await productItemRepo.Entities
            .SingleOrDefaultAsync(pi => pi.Id == createCartItemDto.ProductItemId);
        if (productItem == null)
        {
            throw new BaseException.NotFoundException("product_item_not_found", $"ProductItem {createCartItemDto.ProductItemId} not found.");
        }

        if (quantity > productItem.QuantityInStock)
        {
            throw new BaseException.BadRequestException("invalid_quantity", $"Only {productItem.QuantityInStock} items available.");
        }

        var cartItem = new CartItem
        {
            ProductItem = productItem,
            ProductQuantity = quantity,
            CreatedTime = CoreHelper.SystemTimeNow,
            LastUpdatedTime = CoreHelper.SystemTimeNow
        };

        cart.CartItems.Add(cartItem);

        try
        {
            await _unitOfWork.SaveAsync();
            return BaseResponse<bool>.OkResponse(true);
        }
        catch (Exception ex)
        {
            throw new BaseException.CoreException("server_error", "Error adding cart item. Please try again.", (int)StatusCodeHelper.ServerError);
        }
    }

    public async Task<BaseResponse<bool>> UpdateCartItem(string cartItemId, int productQuantity)
    {
        if (productQuantity < 0)
        {
            throw new BaseException.BadRequestException("non_negative_quantity", "Product quantity must be non-negative.");
        }


        var cartItemRepo = _unitOfWork.GetRepository<CartItem>();
        var cartItem = await cartItemRepo.Entities
            .Where(ci => ci.Id == cartItemId && ci.DeletedTime == null)
            .FirstOrDefaultAsync();

        if (cartItem == null)
        {
            throw new BaseException.BadRequestException("cart_item_not_found", "Cart item not found.");
        }

        var productItemRepo = _unitOfWork.GetRepository<ProductItem>();
        var productItem = await productItemRepo.Entities
        .SingleOrDefaultAsync(pi => pi.Id == cartItem.ProductItemId && pi.DeletedTime == null);

        if (productItem == null)
        {
            throw new BaseException.NotFoundException("product_item_not_found", $"ProductItem {cartItem.ProductItemId} not found.");
        }

        if (productQuantity > productItem.QuantityInStock)
        {
            throw new BaseException.BadRequestException("invalid_quantity", $"Only {productItem.QuantityInStock} items available.");
        }


        cartItem.ProductQuantity = productQuantity;
        cartItem.LastUpdatedTime = CoreHelper.SystemTimeNow;

        try
        {
            await _unitOfWork.SaveAsync();
            return BaseResponse<bool>.OkResponse(true);
        }
        catch (Exception ex)
        {
            throw new BaseException.CoreException("server_error", "Internal server error updating cart item.", (int)StatusCodeHelper.ServerError);
        }
    }

    public async Task<BaseResponse<bool>> RemoveCartItem(string cartItemId)
    {
        var cartItemRepo = _unitOfWork.GetRepository<CartItem>();
        var cartItem = await cartItemRepo.Entities
                         .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.DeletedTime == null);

        if (cartItem == null)
        {
            throw new BaseException.BadRequestException("cart_item_not_found", "Cart item not found.");
        }

        cartItem.DeletedTime = CoreHelper.SystemTimeNow;
        cartItem.DeletedBy = "System"; // Update later after having context accessor

        try
        {
            await _unitOfWork.SaveAsync();
            return BaseResponse<bool>.OkResponse(true);
        }
        catch (Exception ex)
        {
            throw new BaseException.CoreException("server_error", "Internal server error removing cart item.", (int)StatusCodeHelper.ServerError);
        }
    }

}
