using HandmadeProductManagement.Core.Utils;
using HandmadeProductManagement.ModelViews.ShopModelViews;

namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface IShopService
    {
        Task<ShopResponseModel> GetShopByIdAsync(string shopId);
        Task<ShopResponseModel> GetShopByUserIdAsync(Guid userId);
        Task<bool> HaveShopAsync(Guid userId);

        Task<bool> CreateShopAsync(string userId, CreateShopDto createShop);
        Task<bool> UpdateShopAsync(string userId, CreateShopDto shop);
        Task<bool> DeleteShopAsync(string userId);
        Task<decimal> CalculateShopAverageRatingAsync(string shopId);
        Task<IList<ShopResponseModel>> GetAllShopsAsync();
        Task<IList<ShopDto>> GetShopListByAdmin(int pageNumber, int pageSize);
        Task<bool> DeleteShopByIdAsync(string id, string userId);
        Task<IList<ShopDto>> GetDeletedShops(int pageNumber, int pageSize);
        Task<bool> RecoverDeletedShopAsync(string shopId, string userId);
        Task<ShopDto> AdminGetShopByIdAsync(string shopId);
    }
}
