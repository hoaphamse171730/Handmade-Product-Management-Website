using Microsoft.EntityFrameworkCore;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.ModelViews.CartModelViews;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Utils;
using HandmadeProductManagement.Contract.Services.Security;
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
            return BaseResponse<bool>.FailResponse("Product item ID is required.", StatusCodeHelper.BadRequest);
        }

        if (!int.TryParse(createCartItemDto.ProductQuantity.ToString(), out int quantity) || quantity < 0)
        {
            return BaseResponse<bool>.FailResponse("Invalid product quantity. Quantity must be a non-negative integer.", StatusCodeHelper.BadRequest);
        }

        var cartRepo = _unitOfWork.GetRepository<Cart>();
        var cart = await cartRepo.Entities
            .Include(c => c.CartItems)
            .SingleOrDefaultAsync(c => c.Id == cartId);
        if (cart == null)
        {
            return BaseResponse<bool>.FailResponse($"Cart {cartId} not found", StatusCodeHelper.NotFound);
        }

        var productItemRepo = _unitOfWork.GetRepository<ProductItem>();
        var productItem = await productItemRepo.Entities
            .SingleOrDefaultAsync(pi => pi.Id == createCartItemDto.ProductItemId);
        if (productItem == null)
        {
            return BaseResponse<bool>.FailResponse($"ProductItem {createCartItemDto.ProductItemId} not found", StatusCodeHelper.NotFound);
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
            Console.WriteLine($"Error adding cart item to Cart {cartId} with ProductItem {createCartItemDto.ProductItemId}: {ex.Message}");
            return BaseResponse<bool>.FailResponse("Error adding cart item. Please try again.", StatusCodeHelper.ServerError);
        }
    }




    public async Task<BaseResponse<bool>> UpdateCartItem(string cartItemId, int productQuantity)
    {
        if (productQuantity < 0)
        {
            return BaseResponse<bool>.FailResponse("Product quantity must be non-negative", StatusCodeHelper.BadRequest);
        }

        var cartItemRepo = _unitOfWork.GetRepository<CartItem>();
        var cartItem = await cartItemRepo.Entities
            .Where(ci => ci.Id == cartItemId && ci.DeletedTime == null)
            .FirstOrDefaultAsync();

        if (cartItem == null)
        {
            return BaseResponse<bool>.FailResponse("Cart item not found", StatusCodeHelper.NotFound);
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
            return BaseResponse<bool>.FailResponse("Internal server error: " + ex.Message, StatusCodeHelper.ServerError);
        }
    }



    public async Task<BaseResponse<bool>> RemoveCartItem(string cartItemId)
    {
        var cartItemRepo = _unitOfWork.GetRepository<CartItem>();
        var cartItem = await cartItemRepo.Entities
                         .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.DeletedTime == null);

        if (cartItem == null)
        {
            return BaseResponse<bool>.FailResponse("Cart item not found", StatusCodeHelper.NotFound);
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
            return BaseResponse<bool>.FailResponse("Internal server error: " + ex.Message, StatusCodeHelper.ServerError);
        }
    }

}