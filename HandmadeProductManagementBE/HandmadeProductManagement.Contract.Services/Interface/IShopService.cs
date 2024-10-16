﻿using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Utils;
using HandmadeProductManagement.ModelViews.ShopModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface IShopService
    {
        Task<PaginatedList<ShopResponseModel>> GetShopsByPageAsync(int pageNumber, int pageSize);
        Task<ShopResponseModel> GetShopByUserIdAsync(Guid userId);
        Task<bool> CreateShopAsync(string userId, CreateShopDto createShop);
        Task<bool> UpdateShopAsync(string userId, CreateShopDto shop);
        Task<bool> DeleteShopAsync(string userId);
        Task<decimal> CalculateShopAverageRatingAsync(string shopId);

    }
}
