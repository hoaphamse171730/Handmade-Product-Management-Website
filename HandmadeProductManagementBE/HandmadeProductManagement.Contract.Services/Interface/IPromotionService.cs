using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.ModelViews.UserModelViews;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HandmadeProductManagement.ModelViews.PromotionModelViews;

namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface IPromotionService
    {
        Task<IList<PromotionDto>> GetAll();
        Task<PromotionDto> GetById(string id);
        Task<PromotionDto> Create(PromotionForCreationDto promotion);
        Task<PromotionDto> Update(string id, PromotionForUpdateDto promotion);
        Task<bool> SoftDelete(string id);
<<<<<<< HEAD
        Task<bool> UpdatePromotionStatusByRealtime(string id);

=======
        Task<bool> updatePromotionStatusByRealtime(string id);
>>>>>>> 9fa1a47015a52984048ae3d1b5e0f3e4fe2db44f
    }
}
