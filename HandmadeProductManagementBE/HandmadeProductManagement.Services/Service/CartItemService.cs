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
        // var cartItemRepo = _unitOfWork.GetRepository<CartItem>();
        var cart = await _unitOfWork.GetRepository<Cart>()
            .Entities
            .Include(c => c.CartItems)
            .Include(c => c.User)
            .SingleOrDefaultAsync(c => c.Id == cartId);

        var productItem = await _unitOfWork.GetRepository<ProductItem>()
            .Entities
            .SingleOrDefaultAsync(pi => pi.Id == createCartItemDto.ProductItemId);
        
        if (cart is null) throw new ArgumentException($"Cart {cartId} not found");
        
        Console.WriteLine(cartId);
        
        var cartItem = new CartItem
        {
            //catch exception here
            ProductItem = productItem!,
            ProductQuantity = createCartItemDto.ProductQuantity,
            CreatedTime = CoreHelper.SystemTimeNow,
            LastUpdatedTime = CoreHelper.SystemTimeNow
        };
        
        cart.CartItems.Add(cartItem);
        
        try
        {
            // await cartItemRepo.InsertAsync(cartItem);
            await _unitOfWork.SaveAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(cartItem.CartId+"|"+cartItem.ProductItemId);
            Console.WriteLine("Error adding cart item: " + ex.Message);
            return false;
        }
        return true;
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