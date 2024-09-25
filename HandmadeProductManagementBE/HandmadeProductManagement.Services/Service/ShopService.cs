using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.ModelViews.ShopModelViews;
using HandmadeProductManagement.Repositories.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandmadeProductManagement.Services.Service
{
    public class ShopService : IShopService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ShopService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ShopResponseModel> CreateShopAsync(CreateShopDto createShop)
        {
            var userRepository = _unitOfWork.GetRepository<ApplicationUser>();
            var userExists = await userRepository.Entities.AnyAsync(u => u.Id == createShop.UserId);
            if (!userExists)
            {
                throw new BaseException.ErrorException(404, "user_not_found", "User not found.");
            }

            var repository = _unitOfWork.GetRepository<Shop>();

            var existingShop = await repository.Entities
                .FirstOrDefaultAsync(s => s.UserId == createShop.UserId);

            if (existingShop != null)
            {
                if (existingShop.DeletedBy == null)
                {
                    throw new BaseException.ErrorException(
                        400, "user_active_shop", "User already has an active shop.");
                }
                else
                {
                    existingShop.Name = createShop.Name;
                    existingShop.Description = createShop.Description;
                    existingShop.Rating = createShop.Rating;
                    existingShop.DeletedBy = null;
                    existingShop.DeletedTime = null;
                    existingShop.LastUpdatedBy = createShop.UserId.ToString();
                    existingShop.LastUpdatedTime = DateTime.UtcNow;

                    repository.Update(existingShop);
                    await _unitOfWork.SaveAsync();

                    return new ShopResponseModel
                    {
                        Id = existingShop.Id.ToString(),
                        Name = existingShop.Name,
                        Description = existingShop.Description,
                        Rating = existingShop.Rating,
                        UserId = existingShop.UserId
                    };
                }
            }

            var shop = new Shop
            {
                Id = Guid.NewGuid().ToString(),
                Name = createShop.Name,
                Description = createShop.Description,
                Rating = createShop.Rating,
                UserId = createShop.UserId
            };

            shop.CreatedBy = shop.UserId.ToString();
            shop.CreatedTime = DateTime.UtcNow;
            shop.LastUpdatedBy = shop.UserId.ToString();
            shop.LastUpdatedTime = DateTime.UtcNow;

            await repository.InsertAsync(shop);
            await _unitOfWork.SaveAsync();

            return new ShopResponseModel
            {
                Id = shop.Id.ToString(),
                Name = shop.Name,
                Description = shop.Description,
                Rating = shop.Rating,
                UserId = shop.UserId
            };
        }

        public async Task<bool> DeleteShopAsync(Guid userId, string id)
        {
            var repository = _unitOfWork.GetRepository<Shop>();
            var shop = await repository.Entities.FirstOrDefaultAsync(s => s.Id == id);
            if (shop == null)
            {
                return false;
            }

            shop.DeletedBy = userId.ToString();
            shop.DeletedTime = DateTime.UtcNow;

            repository.Update(shop);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<IList<ShopResponseModel>> GetAllShopsAsync()
        {
            IQueryable<Shop> query = _unitOfWork.GetRepository<Shop>().Entities
                .Where(shop => shop.DeletedBy == null);
            var result = await query.Select(shop => new ShopResponseModel
            {
                Id = shop.Id.ToString(),
                Name = shop.Name,
                Description = shop.Description,
                Rating = shop.Rating,
                UserId = shop.UserId
            }).ToListAsync();

            return result;
        }

        public async Task<ShopResponseModel> GetShopByUserIdAsync(Guid userId)
        {
            var userRepository = _unitOfWork.GetRepository<ApplicationUser>();
            var userExists = await userRepository.Entities.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                throw new BaseException.ErrorException(404, "user_not_found", "User not found.");
            }

            var repository = _unitOfWork.GetRepository<Shop>();
            var shop = await repository.Entities
                .FirstOrDefaultAsync(s => s.UserId == userId && s.DeletedBy == null);

            if (shop == null)
            {
                throw new BaseException.ErrorException(
                    404, "shop_not_found", "Shop not found for the given user.");
            }

            return new ShopResponseModel
            {
                Id = shop.Id.ToString(),
                Name = shop.Name,
                Description = shop.Description,
                Rating = shop.Rating,
                UserId = shop.UserId
            };
        }

        public async Task<ShopResponseModel> UpdateShopAsync(string id, CreateShopDto shop)
        {
            var repository = _unitOfWork.GetRepository<Shop>();
            var existingShop = await repository.Entities
                .FirstOrDefaultAsync(s => s.Id == id && s.DeletedBy == null);

            if (existingShop == null)
            {
                throw new BaseException.ErrorException(
                    404, "shop_not_found", "Shop not found.");
            }

            existingShop.Name = shop.Name;
            existingShop.Description = shop.Description;
            existingShop.Rating = shop.Rating;
            existingShop.LastUpdatedBy = shop.UserId.ToString();
            existingShop.LastUpdatedTime = DateTime.UtcNow;

            repository.Update(existingShop);
            await _unitOfWork.SaveAsync();

            return new ShopResponseModel
            {
                Id = existingShop.Id.ToString(),
                Name = existingShop.Name,
                Description = existingShop.Description,
                Rating = existingShop.Rating,
                UserId = existingShop.UserId
            };
        }
    }
}
