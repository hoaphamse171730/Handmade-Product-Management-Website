using Microsoft.EntityFrameworkCore;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.ModelViews.CartModelViews;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Utils;
using HandmadeProductManagement.Contract.Services.Security;

public class CartItemService : ICartItemService
{
    private readonly IUnitOfWork _unitOfWork;

    public CartItemService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> AddCartItem(string cartId, CreateCartItemDto createCartItemDto)
    {
        Console.WriteLine($"Attempting to add item to cart: {cartId}, ProductItem: {createCartItemDto.ProductItemId}");

        var cartRepo = _unitOfWork.GetRepository<Cart>();
        var cart = await cartRepo.Entities
            .Include(c => c.CartItems)
            .SingleOrDefaultAsync(c => c.Id == cartId);
        if (cart is null)
        {
            throw new ArgumentException($"Cart {cartId} not found");
        }

        var productItemRepo = _unitOfWork.GetRepository<ProductItem>();
        var productItem = await productItemRepo.Entities
            .SingleOrDefaultAsync(pi => pi.Id == createCartItemDto.ProductItemId);
        if (productItem is null)
        {
            throw new ArgumentException($"ProductItem {createCartItemDto.ProductItemId} not found");
        }

        var cartItem = new CartItem
        {
            ProductItem = productItem,
            ProductQuantity = createCartItemDto.ProductQuantity,
            CreatedTime = CoreHelper.SystemTimeNow,
            LastUpdatedTime = CoreHelper.SystemTimeNow
        };

        cart.CartItems.Add(cartItem);

        try
        {
            await _unitOfWork.SaveAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding cart item to Cart {cartId} with ProductItem {createCartItemDto.ProductItemId}: {ex.Message}");
            return false;
        }
    }


    public async Task<bool> UpdateCartItem(string cartItemId, CartItemModel cartItemModel)
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

    public async Task<bool> RemoveCartItem(string cartItemId)
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