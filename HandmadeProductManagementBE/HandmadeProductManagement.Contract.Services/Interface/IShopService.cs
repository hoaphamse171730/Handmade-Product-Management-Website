using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.ModelViews.ShopModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface IShopService
    {
        Task<IList<ShopResponseModel>> GetAllShopsAsync();
        Task<ShopResponseModel> GetShopByUserIdAsync(Guid userId);
        Task<bool> CreateShopAsync(string userId, CreateShopDto createShop);
        Task<bool> UpdateShopAsync(string userId, string id, CreateShopDto shop);
        Task<bool> DeleteShopAsync(string userId, string id);
        Task<decimal> CalculateShopAverageRatingAsync(string shopId);

    }
}
