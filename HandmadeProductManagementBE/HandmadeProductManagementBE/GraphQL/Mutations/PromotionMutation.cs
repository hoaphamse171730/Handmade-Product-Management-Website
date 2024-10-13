using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.ModelViews.PromotionModelViews;

namespace HandmadeProductManagementAPI.GraphQL.Mutations;

public class PromotionMutation
{
    public async Task<bool> CreatePromotion([Service] IPromotionService promotionService, PromotionForCreationDto promotion)
    {
        return await promotionService.Create(promotion);
    }

    public async Task<bool> UpdatePromotion([Service] IPromotionService promotionService, string id, PromotionForUpdateDto promotion)
    {
        return await promotionService.Update(id, promotion);
    }

    public async Task<bool> SoftDeletePromotion([Service] IPromotionService promotionService, string id)
    {
        return await promotionService.SoftDelete(id);
    }
}