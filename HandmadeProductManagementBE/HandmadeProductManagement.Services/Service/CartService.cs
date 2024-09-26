using Microsoft.EntityFrameworkCore;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.ModelViews.CartModelViews;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Services;
using HandmadeProductManagement.Core.Utils;

public class CartService : ICartService
{
    private readonly IUnitOfWork _unitOfWork;

    public CartService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CartModel> GetCartByUserId(Guid userId)
    {
        var cart = await _unitOfWork.GetRepository<Cart>().Entities
                   .Where(x => x.UserId == userId && x.DeletedTime == null)
                   .Include(c => c.CartItems)
                   .FirstOrDefaultAsync();

        return new CartModel
        {
            CartId = cart.Id,
            UserId = cart.UserId,
            CartItems = cart.CartItems.Where(ci => ci.DeletedTime == null).Select(ci => new CartItemModel
            {
                CartItemId = ci.Id,
                ProductItemId = ci.ProductItemId,
                ProductQuantity = ci.ProductQuantity
            }).ToList()
        };
    }

    public async Task<CartModel> CreateOrUpdateCart(Guid userId, CartModel cartModel)
    {
        var cartRepo = _unitOfWork.GetRepository<Cart>();
        var cart = await cartRepo.Entities
                        .Where(c => c.UserId == userId && c.DeletedTime == null)
                        .Include(c => c.CartItems)
                        .FirstOrDefaultAsync();

        if (cart == null)
        {
            cart = new Cart
            {
                UserId = userId,
            };
            cartRepo.Insert(cart);
        }
        cart.LastUpdatedTime = CoreHelper.SystemTimeNow;
        await _unitOfWork.SaveAsync();
        return cartModel; // Return updated model, ideally map from entity
    }

    public async Task<bool> DeleteCart(Guid userId)
    {
        var cart = await _unitOfWork.GetRepository<Cart>().Entities
                   .FirstOrDefaultAsync(c => c.UserId == userId && c.DeletedTime == null);

        if (cart != null)
        {
            cart.DeletedTime = CoreHelper.SystemTimeNow;
            cart.DeletedBy = userId;
            await _unitOfWork.SaveAsync();
            return true;
        }

        return false;
    }
}
