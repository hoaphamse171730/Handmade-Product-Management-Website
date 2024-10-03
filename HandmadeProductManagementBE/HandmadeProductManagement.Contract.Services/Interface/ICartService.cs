using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.ModelViews.CartModelViews;

namespace HandmadeProductManagement.Contract.Services
{
    public interface ICartService
    {
        Task<CartModel> GetCartByUserId(Guid userId);
        //Task<CartModel> CreateCart(Guid userId);
        //Task<bool> DeleteCart(Guid userId);

        Task<Decimal> GetTotalCartPrice(string cartId);
    }
}
