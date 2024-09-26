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
        Task<bool> AddOrUpdateCartItem(Guid cartId, CartItemModel cartItemModel);
        Task<bool> RemoveCartItem(Guid cartItemId);
    }
}