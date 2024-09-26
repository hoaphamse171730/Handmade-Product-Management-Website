using Microsoft.EntityFrameworkCore;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.ModelViews.CartModelViews;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Utils;

public class CartItemService : ICartItemService
{
    private readonly IUnitOfWork _unitOfWork;

    public CartItemService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> AddCartItem(Guid cartId, CreateCartItemDto createCartItemDto)
    {
        var cartItemRepo = _unitOfWork.GetRepository<CartItem>();
        var cartItem = new CartItem
        {
            CartId = cartId.ToString(),
            ProductItemId = createCartItemDto.ProductItemId,
            ProductQuantity = createCartItemDto.ProductQuantity,
            CreatedTime = CoreHelper.SystemTimeNow,
            LastUpdatedTime = CoreHelper.SystemTimeNow
        };
        cartItemRepo.Insert(cartItem);
        await _unitOfWork.SaveAsync();
        return true;
    }

    public async Task<bool> UpdateCartItem(Guid cartItemId, CartItemModel cartItemModel)
    {
        var cartItemRepo = _unitOfWork.GetRepository<CartItem>();
        var cartItem = await cartItemRepo.Entities
                           .Where(ci => ci.Id == cartItemId.ToString() && ci.DeletedTime == null)
                           .FirstOrDefaultAsync();

        if (cartItem == null)
            throw new InvalidOperationException("Cart item not found.");

        cartItem.ProductQuantity = cartItemModel.ProductQuantity;
        cartItem.LastUpdatedTime = CoreHelper.SystemTimeNow;
        await _unitOfWork.SaveAsync();
        return true;
    }

    public async Task<bool> RemoveCartItem(Guid cartItemId)
    {
        var cartItem = await _unitOfWork.GetRepository<CartItem>().Entities
                         .FirstOrDefaultAsync(ci => ci.Id == cartItemId.ToString() && ci.DeletedTime == null);

        if (cartItem != null)
        {
            cartItem.DeletedTime = CoreHelper.SystemTimeNow;
            cartItem.DeletedBy = "System";//update later after have context accessor
            await _unitOfWork.SaveAsync();
            return true;
        }

        return false;
    }
}