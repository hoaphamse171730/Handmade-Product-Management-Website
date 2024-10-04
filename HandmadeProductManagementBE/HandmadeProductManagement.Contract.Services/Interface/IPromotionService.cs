using HandmadeProductManagement.ModelViews.PromotionModelViews;

namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface IPromotionService
    {
        Task<IList<PromotionDto>> GetAll();
        Task<PromotionDto> GetById(string id);
        Task<PromotionDto> Create(PromotionForCreationDto promotion);
        Task<PromotionDto> Update(string id, PromotionForUpdateDto promotion);
        Task<bool> Delete(string id);
        Task<bool> SoftDelete(string id);
        Task<bool> UpdatePromotionStatusByRealtime(string id);
    }
}
