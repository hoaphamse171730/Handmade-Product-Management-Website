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

    public async Task<CartModel?> GetCartByUserId(Guid userId)
    {
        var cart = await _unitOfWork.GetRepository<Cart>().Entities
                   .Where(x => x.UserId == userId && x.DeletedTime == null)
                   .Include(c => c.CartItems)
                   .FirstOrDefaultAsync();

        if (cart == null)
        {
            return null;  // Return null when no cart is found
        }

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



    //public async Task<CartModel> CreateCart(Guid userId)
    //{
    //    var cartRepo = _unitOfWork.GetRepository<Cart>();
    //    var cartExists = await cartRepo.Entities
    //                           .AnyAsync(c => c.UserId == userId && c.DeletedTime == null);

    //    if (cartExists)
    //    {
    //        throw new InvalidOperationException($"A cart already exists for user ID {userId}.");
    //    }

    //    var newCart = new Cart
    //    {
    //        UserId = userId,
    //        CreatedTime = CoreHelper.SystemTimeNow,
    //        LastUpdatedTime = CoreHelper.SystemTimeNow
    //    };

    //    cartRepo.Insert(newCart);
    //    await _unitOfWork.SaveAsync();

    //    return new CartModel
    //    {
    //        CartId = newCart.Id,
    //        UserId = newCart.UserId,
    //    };
    //}


    //public async Task<bool> DeleteCart(Guid userId)
    //{
    //    var cart = await _unitOfWork.GetRepository<Cart>().Entities
    //               .FirstOrDefaultAsync(c => c.UserId == userId && c.DeletedTime == null);

    //    if (cart != null)
    //    {
    //        cart.DeletedTime = CoreHelper.SystemTimeNow;
    //        cart.DeletedBy = userId.ToString();
    //        await _unitOfWork.SaveAsync();
    //        return true;
    //    }

    //    return false;
    //}
}
