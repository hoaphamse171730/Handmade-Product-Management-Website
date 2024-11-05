using HandmadeProductManagement.ModelViews.PromotionModelViews;
using HandmadeProductManagement.ModelViews.CategoryModelViews;
using HandmadeProductManagement.Core.Utils;

namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface ICategoryService
    {
        Task<IList<CategoryDto>> GetAll();
        Task<IList<CategoryDtoWithDetail>> GetAllWithDetailByPageAsync(int pageNumber, int pageSize);
        Task<CategoryDto> GetById(string id);
        Task<IList<CategoryDto>> GetAllDeleted();
        Task<bool> Create(CategoryForCreationDto category);
        Task<CategoryDto> Update(string id, CategoryForUpdateDto category);
        Task<bool> SoftDelete(string id);

        Task<CategoryDto> UpdatePromotion(string id, CategoryForUpdatePromotion category);

        Task<bool> RestoreCategory(string id, string userId);

    }
}
