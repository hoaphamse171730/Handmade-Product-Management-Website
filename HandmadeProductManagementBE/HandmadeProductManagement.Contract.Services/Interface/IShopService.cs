using HandmadeProductManagement.Core.Utils;
using HandmadeProductManagement.ModelViews.ShopModelViews;

namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface IShopService
    {
        Task<PaginatedList<ShopResponseModel>> GetShopsByPageAsync(int pageNumber, int pageSize);
        Task<ShopResponseModel> GetShopByUserIdAsync(Guid userId);
        Task<bool> CreateShopAsync(string userId, CreateShopDto createShop);
        Task<bool> UpdateShopAsync(string userId, CreateShopDto shop);
        Task<bool> DeleteShopAsync(string userId);
        Task<decimal> CalculateShopAverageRatingAsync(string shopId);
        Task<IList<ShopResponseModel>> GetAllShopsAsync();

    }
}
