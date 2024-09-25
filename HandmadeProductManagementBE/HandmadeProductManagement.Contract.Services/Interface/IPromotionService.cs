using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.ModelViews.UserModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HandmadeProductManagement.ModelViews.PromotionModelViews;

namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface IPromotionService
    {
        Task<IEnumerable<PromotionDto>> GetAllPromotionAsync();
        Task<PromotionDto> GetPromotionByIdAsync(Guid id);
        Task DeletePromotionAsync(Guid id);
        Task UpdatePromotionAsync(Guid id, PromotionForUpdateDto promotionDto);
        Task CreatePromotionAsync(PromotionDto promotionDto);
    }
}
