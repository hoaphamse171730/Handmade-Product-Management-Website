using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.ModelViews.CartItemModelViews;

namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface ICartItemService
    {
        Task<bool> AddCartItem(CartItemForCreationDto createCartItemDto, string userId);
        Task<bool> UpdateCartItem(string cartItemId, CartItemForUpdateDto updateCartItemDto, string userId);
        Task<bool> DeleteCartItemByIdAsync(string cartItemId, string userId);
        Task<List<CartItemGroupDto>> GetCartItemsByUserIdAsync(string userId);
        Task<List<CartItem>> GetCartItemsByUserIdForOrderCreation(string userId);
        Task<Decimal> GetTotalCartPrice(string cartItemId);
    }
}
