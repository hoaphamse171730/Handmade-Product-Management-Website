using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.ModelViews.ShopModelViews;
using HandmadeProductManagement.Repositories.Entity;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

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
            ValidateShop(createShop);

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
                if (existingShop.DeletedTime == null)
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
            if (!Guid.TryParse(userId.ToString(), out _) || !Guid.TryParse(id, out _))
            {
                throw new BaseException.BadRequestException("invalid_format", "UserId or Id is not in the correct format. Ex: 123e4567-e89b-12d3-a456-426614174000.");
            }

            var userRepository = _unitOfWork.GetRepository<ApplicationUser>();
            var userExists = await userRepository.Entities.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                throw new BaseException.ErrorException(404, "user_not_found", "User not found.");
            }

            var repository = _unitOfWork.GetRepository<Shop>();
            var shop = await repository.Entities.FirstOrDefaultAsync(s => s.Id == id);
            if (shop == null)
            {
                throw new BaseException.ErrorException(404, "shop_not_found", "Shop not found.");
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
                .Where(shop => shop.DeletedTime == null);
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
                .FirstOrDefaultAsync(s => s.UserId == userId && s.DeletedTime == null);

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
            ValidateShop(shop);

            if (string.IsNullOrWhiteSpace(id) || !Guid.TryParse(id, out _))
            {
                throw new BaseException.BadRequestException("invalid_format", "Id is not in the correct format. " +
                    "Ex: 123e4567-e89b-12d3-a456-426614174000.");
            }

            var repository = _unitOfWork.GetRepository<Shop>();
            var existingShop = await repository.Entities
                .FirstOrDefaultAsync(s => s.Id == id && s.DeletedTime == null);

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

        private void ValidateShop(CreateShopDto shop)
        {
            if (string.IsNullOrEmpty(shop.Name) || string.IsNullOrEmpty(shop.Description))
            {
                throw new BaseException.BadRequestException("invalid_input", "Name and Description cannot be null or empty.");
            }

            if (!Regex.IsMatch(shop.Name, @"^[a-zA-Z\s]+$"))
            {
                throw new BaseException.BadRequestException("invalid_name", "Name cannot contain numbers or special characters.");
            }

            if (!Regex.IsMatch(shop.Description, @"^[a-zA-Z0-9\s]+$"))
            {
                throw new BaseException.BadRequestException("invalid_description", "Description cannot contain special characters.");
            }

            if (shop.Rating < 0 || shop.Rating > 5)
            {
                throw new BaseException.BadRequestException("invalid_rating", "Rating must be between 0 and 5.");
            }

            if (!Guid.TryParse(shop.UserId.ToString(), out _))
            {
                throw new BaseException.BadRequestException("invalid_format", "UserId is not in the correct format. Ex: 123e4567-e89b-12d3-a456-426614174000.");
            }
        }
    }
}