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

    public async Task<bool> AddOrUpdateCartItem(Guid cartId, CartItemModel cartItemModel)
    {
        var cartItemRepo = _unitOfWork.GetRepository<CartItem>();
        var cartItem = await cartItemRepo.Entities
                           .Where(ci => ci.Id == cartItemModel.CartItemId && ci.DeletedTime == null)
                           .FirstOrDefaultAsync();

        if (cartItem == null)
        {
            cartItem = new CartItem
            {
                CartId = cartId.ToString(),
                ProductItemId = cartItemModel.ProductItemId,
                ProductQuantity = cartItemModel.ProductQuantity
            };
            cartItemRepo.Insert(cartItem);
        }
        else
        {
            cartItem.ProductQuantity = cartItemModel.ProductQuantity; // Update existing item
            cartItem.LastUpdatedTime = CoreHelper.SystemTimeNow;
        }

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
            cartItem.DeletedBy = "System"; // Example, use actual user context if available

            await _unitOfWork.SaveAsync();
            return true;
        }

        return false;
    }
}
