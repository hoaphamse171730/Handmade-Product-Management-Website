﻿using HandmadeProductManagement.ModelViews.VariationModelViews;

namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface IVariationService
    {
        Task<IList<VariationDto>> GetByCategoryId(string id);
        Task<IList<VariationDto>> GetByPage(int page, int pageSize);
        Task<bool> Create(VariationForCreationDto cancelReason);
        Task<bool> Update(string id, VariationForUpdateDto cancelReason);
        Task<bool> Delete(string id);
    }
}