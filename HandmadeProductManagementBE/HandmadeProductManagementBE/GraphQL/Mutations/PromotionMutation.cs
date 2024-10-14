using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.ModelViews.PromotionModelViews;

namespace HandmadeProductManagementAPI.GraphQL.Mutations;

public class PromotionMutation
{
    public async Task<bool> CreatePromotion([Service] IPromotionService promotionService, PromotionForCreationDto promotion, string userId) => await promotionService.Create(promotion, userId);

    public async Task<bool> UpdatePromotion([Service] IPromotionService promotionService, string id, PromotionForUpdateDto promotion, string userId) => await promotionService.Update(id, promotion, userId);
    
    public async Task<bool> SoftDeletePromotion([Service] IPromotionService promotionService, string id) => await promotionService.SoftDelete(id);
   
}