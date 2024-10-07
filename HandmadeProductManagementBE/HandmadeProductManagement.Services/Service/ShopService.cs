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

        public async Task<bool> CreateShopAsync(string userId, CreateShopDto createShop)
        {
            ValidateShop(createShop);

            var userRepository = _unitOfWork.GetRepository<ApplicationUser>();
            var userExists = await userRepository.Entities.AnyAsync(
                u => u.Id.ToString() == userId && !u.DeletedTime.HasValue);
            if (!userExists)
            {
                throw new BaseException.NotFoundException("user_not_found", "User not found.");
            }

            var repository = _unitOfWork.GetRepository<Shop>();

            var existingShop = await repository.Entities
                .FirstOrDefaultAsync(s => s.UserId.ToString() == userId && !s.DeletedTime.HasValue);

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
                    existingShop.LastUpdatedBy = userId;
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
                UserId = Guid.Parse(userId),
                CreatedBy = userId,
                LastUpdatedBy = userId
            };

            await repository.InsertAsync(shop);
            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<bool> DeleteShopAsync(string userId, string id)
        {
            if (!Guid.TryParse(userId.ToString(), out _) || !Guid.TryParse(id, out _))
            {
                throw new BaseException.BadRequestException("invalid_format", "UserId or Id is not in the correct format. Ex: 123e4567-e89b-12d3-a456-426614174000.");
            }

            var userRepository = _unitOfWork.GetRepository<ApplicationUser>();
            var userExists = await userRepository.Entities.AnyAsync(u => u.Id.ToString() == userId && !u.DeletedTime.HasValue);
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

            shop.DeletedBy = userId;
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

        public async Task<bool> UpdateShopAsync(string userId, string id, CreateShopDto shop)
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
            existingShop.LastUpdatedBy = userId;
            existingShop.LastUpdatedTime = DateTime.UtcNow;

            repository.Update(existingShop);
            await _unitOfWork.SaveAsync();

            return true;
        }

        private void ValidateShop(CreateShopDto shop)
        {
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

        public async Task<decimal> CalculateShopAverageRatingAsync(string shopId)
        {
            if (string.IsNullOrEmpty(shopId))
            {
                throw new BaseException.BadRequestException("invalid_shop_id", "Shop ID cannot be null or empty.");
            }

            if (!Guid.TryParse(shopId, out _))
            {
                throw new BaseException.BadRequestException("invalid_shop_id", "Shop ID is not a valid GUID.");
            }

            var shop = await _unitOfWork.GetRepository<Shop>().Entities
                                        .Include(s => s.Products)
                                        .FirstOrDefaultAsync(s => s.Id == shopId);

            if (shop == null)
            {
                throw new BaseException.NotFoundException("shop_not_found", "Shop not found.");
            }

            if (shop.Products == null || !shop.Products.Any())
            {
                return 0m; // Return 0 as decimal if there are no products
            }

            var productRepository = _unitOfWork.GetRepository<Product>();
            var productRatings = await productRepository.Entities
                .Where(p => p.ShopId == shopId && !p.DeletedTime.HasValue)
                .Select(p => p.Rating)
                .ToListAsync();

            if (!productRatings.Any())
            {
                return 0m;
            }

            decimal averageRating = Math.Round(productRatings.Average(), 1);

            // Update the shop's rating
            shop.Rating = averageRating;
            await _unitOfWork.GetRepository<Shop>().UpdateAsync(shop);
            await _unitOfWork.SaveAsync();

            return averageRating;
        }
    }
}