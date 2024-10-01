using Microsoft.EntityFrameworkCore;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.ModelViews.CartModelViews;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Services;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Constants;

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
                   .Where(x => x.UserId == userId && !x.DeletedTime.HasValue && x.DeletedBy == null)
                   .Include(c => c.CartItems)
                   .FirstOrDefaultAsync();

        if (cart == null)
        {
            throw new BaseException.CoreException("cart_not_found", $"Cart not found for user ID {userId}", (int)StatusCodeHelper.NotFound);
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

}
