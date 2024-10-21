using HandmadeProductManagement.ModelViews.PromotionModelViews;

namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface IPromotionService
    {
        Task<IList<PromotionDto>> GetAll(int pageNumber, int pageSize);
        Task<PromotionDto> GetById(string id);
        Task<IList<PromotionDto>> GetExpiredPromotions(int pageNumber, int pageSize);
        Task<bool> Create(PromotionForCreationDto promotion, string userId);
        Task<bool> Update(string id, PromotionForUpdateDto promotion, string userId);
        Task<bool> SoftDelete(string id);
        Task<bool> UpdatePromotionStatusByRealtime(string id);
        Task<bool> RecoverDeletedPromotionAsync(string id, Guid userId);
    }
}
