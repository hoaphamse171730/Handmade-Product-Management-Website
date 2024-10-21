using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.ModelViews.PromotionModelViews;

namespace HandmadeProductManagementAPI.GraphQL.Queries;

public class PromotionQuery
{
    public async Task<IList<PromotionDto>> GetPromotions([Service] IPromotionService promotionService, int pageNumber = 1, int pageSize = 10)
    {
        return await promotionService.GetAll(pageNumber, pageSize);
    }

    public async Task<PromotionDto> GetPromotionById([Service] IPromotionService promotionService, string id)
    {
        return await promotionService.GetById(id);
    }

    public async Task<IList<PromotionDto>> GetExpiredPromotions([Service] IPromotionService promotionService, int pageNumber = 1, int pageSize = 10)
    {
        return await promotionService.GetExpiredPromotions(pageNumber, pageSize);
    }
}