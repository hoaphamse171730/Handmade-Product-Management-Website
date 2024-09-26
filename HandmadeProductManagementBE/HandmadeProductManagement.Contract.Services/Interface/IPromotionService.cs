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
        Task<IList<Promotion>> GetAll();
        Task<Promotion> GetById(string id);
        Task<Promotion> Create(Promotion promotion);
        Task<Promotion> Update(string id, Promotion promotion);
        Task<bool> Delete(string id);
    }
}
