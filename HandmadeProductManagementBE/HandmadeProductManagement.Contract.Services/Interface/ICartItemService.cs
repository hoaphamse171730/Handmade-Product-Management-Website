using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.ModelViews.CartModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface ICartItemService
    {
        Task<bool> AddCartItem(string cartId, CreateCartItemDto createCartItemDto, string userId);
        Task<bool> UpdateCartItem(string cartItemId, int productQuantity, string userId);
        Task<bool> DeleteCartItemByIdAsync(string cartItemId, string userId);
        Task<List<CartItem>> GetCartItemsByUserIdAsync(string userId);
    }
}