using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.ModelViews.PaymentModelViews;
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

        public async Task<bool> CreateShopAsync(CreateShopDto createShop)
        {
            ValidateShop(createShop);

            var userRepository = _unitOfWork.GetRepository<ApplicationUser>();
            var userExists = await userRepository.Entities.AnyAsync(
                u => u.Id.ToString() == createShop.UserId && !u.DeletedTime.HasValue);
            if (!userExists)
            {
                throw new BaseException.NotFoundException("user_not_found", "User not found.");
            }

            var repository = _unitOfWork.GetRepository<Shop>();

            var existingShop = await repository.Entities
                .FirstOrDefaultAsync(s => s.UserId.ToString() == createShop.UserId && !s.DeletedTime.HasValue);

            if (existingShop != null)
            {
                if (existingShop.DeletedTime == null)
                {
                    throw new BaseException.BadRequestException(
                        "user_active_shop", "User already has an active shop.");
                }
                else
                {
                    existingShop.Name = createShop.Name;
                    existingShop.Description = createShop.Description;
                    existingShop.Rating = 0;
                    existingShop.DeletedBy = null;
                    existingShop.DeletedTime = null;
                    existingShop.LastUpdatedBy = createShop.UserId.ToString();
                    existingShop.LastUpdatedTime = DateTime.UtcNow;

                    repository.Update(existingShop);
                    await _unitOfWork.SaveAsync();

                    return true;
                }
            }

            var shop = new Shop
            {
                Name = createShop.Name,
                Description = createShop.Description,
                Rating = 0,
                UserId = Guid.Parse(createShop.UserId)
            };

            await repository.InsertAsync(shop);
            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<bool> DeleteShopAsync(Guid userId, string id)
        {
            if (!Guid.TryParse(userId.ToString(), out _) || !Guid.TryParse(id, out _))
            {
                throw new BaseException.BadRequestException("invalid_format", "UserId or Id is not in the correct format. Ex: 123e4567-e89b-12d3-a456-426614174000.");
            }

            var userRepository = _unitOfWork.GetRepository<ApplicationUser>();
            var userExists = await userRepository.Entities.AnyAsync(u => u.Id == userId && !u.DeletedTime.HasValue);
            if (!userExists)
            {
                throw new BaseException.NotFoundException("user_not_found", "User not found.");
            }

            var repository = _unitOfWork.GetRepository<Shop>();
            var shop = await repository.Entities.FirstOrDefaultAsync(s => s.Id == id && !s.DeletedTime.HasValue);
            if (shop == null)
            {
                throw new BaseException.NotFoundException("shop_not_found", "Shop not found.");
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
                .Where(shop => !shop.DeletedTime.HasValue);
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
            var userExists = await userRepository.Entities.AnyAsync(u => u.Id == userId && !u.DeletedTime.HasValue);
            if (!userExists)
            {
                throw new BaseException.NotFoundException("user_not_found", "User not found.");
            }

            var repository = _unitOfWork.GetRepository<Shop>();
            var shop = await repository.Entities
                .FirstOrDefaultAsync(s => s.UserId == userId && !s.DeletedTime.HasValue);

            if (shop == null)
            {
                throw new BaseException.NotFoundException(
                    "shop_not_found", "Shop not found for the given user.");
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

        public async Task<bool> UpdateShopAsync(string id, CreateShopDto shop)
        {
            ValidateShop(shop);

            if (string.IsNullOrWhiteSpace(id) || !Guid.TryParse(id, out _))
            {
                throw new BaseException.BadRequestException("invalid_format", "Id is not in the correct format. " +
                    "Ex: 123e4567-e89b-12d3-a456-426614174000.");
            }

            var repository = _unitOfWork.GetRepository<Shop>();
            var existingShop = await repository.Entities
                .FirstOrDefaultAsync(s => s.Id == id && !s.DeletedTime.HasValue);

            if (existingShop == null)
            {
                throw new BaseException.NotFoundException(
                    "shop_not_found", "Shop not found.");
            }

            existingShop.Name = shop.Name;
            existingShop.Description = shop.Description;
            existingShop.LastUpdatedBy = shop.UserId.ToString();
            existingShop.LastUpdatedTime = DateTime.UtcNow;

            repository.Update(existingShop);
            await _unitOfWork.SaveAsync();

            return true;
        }

        private void ValidateShop(CreateShopDto shop)
        {
            if (string.IsNullOrWhiteSpace(shop.UserId))
            {
                throw new BaseException.BadRequestException("invalid_user_id", "Please input user id.");
            }

            if (!Guid.TryParse(shop.UserId, out _))
            {
                throw new BaseException.BadRequestException("invalid_user_id_format", "User ID format is invalid. Example: 123e4567-e89b-12d3-a456-426614174000.");
            }

            if (string.IsNullOrWhiteSpace(shop.Name))
            {
                throw new BaseException.BadRequestException("invalid_shop_name", "Please input shop name.");
            }

            if (Regex.IsMatch(shop.Name, @"[^a-zA-Z\s]"))
            {
                throw new BaseException.BadRequestException("invalid_shop_name_format", "Shop name can only contain letters and spaces.");
            }

            if (string.IsNullOrWhiteSpace(shop.Description))
            {
                throw new BaseException.BadRequestException("invalid_shop_description", "Please input shop description.");
            }

            if (Regex.IsMatch(shop.Description, @"[^a-zA-Z0-9\s]"))
            {
                throw new BaseException.BadRequestException("invalid_shop_description_format", "Shop description cannot contain special characters.");
            }

        }
    }
}