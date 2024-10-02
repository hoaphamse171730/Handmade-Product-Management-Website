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
                   .Where(x => x.UserId == userId && x.DeletedTime == null)
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

 /*   public async Task<Decimal> GetTotalCartPrice(string cartId)
    {

        var cart = await _unitOfWork.GetRepository<Cart>()
            .Entities
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.ProductItem)
            .ThenInclude(pi => pi.Product)
            .ThenInclude(p => p.Category)
            .ThenInclude(cat => cat.Promotion)
            .FirstOrDefaultAsync(c => c.Id == cartId);

        if (cart.Is || cart.CartItems.Count == 0)
        {
            throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), "Cart not found");

        }

        decimal totalPrice = 0;

        foreach (var cartItem in cart.CartItems)
        {
            var productItemPrice = cartItem.ProductItem.Price;
            var productQuantity = cartItem.ProductQuantity;

            var promotion = cartItem.ProductItem.Product.Category.Promotion;
            decimal discountRate = 1; 

            if (promotion != null && promotion.StartDate <= DateTime.UtcNow && promotion.EndDate >= DateTime.UtcNow)
            {
                discountRate = 1 - (decimal)promotion.DiscountRate;
            }

            totalPrice += productItemPrice * productQuantity * discountRate;
        }

        return totalPrice;
    }
*/
}
