using HandmadeProductManagement.ModelViews.PromotionModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HandmadeProductManagement.ModelViews.CategoryModelViews;

namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface ICategoryService
    {
        Task<IList<CategoryDto>> GetAll();
        Task<CategoryDto> GetById(string id);
        Task<bool> Create(CategoryForCreationDto category);
        Task<bool> Update(string id, CategoryForUpdateDto category);
        Task<bool> SoftDelete(string id);

        Task<CategoryDto> UpdatePromotion(string id, CategoryForUpdatePromotion category);
    }
}
