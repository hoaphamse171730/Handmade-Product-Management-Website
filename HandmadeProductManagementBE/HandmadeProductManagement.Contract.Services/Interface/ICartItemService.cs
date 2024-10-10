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
        Task<BaseResponse<bool>> AddCartItem(string cartId, CreateCartItemDto createCartItemDto);
        Task<BaseResponse<bool>> UpdateCartItem(string cartItemId, int productQuantity);
        Task<BaseResponse<bool>> RemoveCartItem(string cartItemId);
        Task<List<CartItem>> GetCartItemsByUserIdAsync(string userId);
        Task<bool> DeleteCartItemByIdAsync(string cartItemId);
    }
}