using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Utils;
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

        public async Task<bool> DeleteShopAsync(string userId)
        {
            var userRepository = _unitOfWork.GetRepository<ApplicationUser>();
            var userExists = await userRepository.Entities.AnyAsync(u => u.Id.ToString() == userId && !u.DeletedTime.HasValue);
            if (!userExists)
            {
                throw new BaseException.NotFoundException("user_not_found", "User not found.");
            }

            var shopRepository = _unitOfWork.GetRepository<Shop>();
            var shop = await shopRepository.Entities
                .FirstOrDefaultAsync(s => s.UserId.ToString() == userId && !s.DeletedTime.HasValue);

            if (shop == null)
            {
                throw new BaseException.NotFoundException("shop_not_found", "Shop not found.");
            }

            shop.DeletedBy = userId;
            shop.DeletedTime = DateTime.UtcNow;

            shopRepository.Update(shop);
            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<PaginatedList<ShopResponseModel>> GetShopsByPageAsync(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0)
            {
                throw new BaseException.BadRequestException("invalid_input", "Page must be greater than 0.");
            }

            if (pageSize <= 0)
            {
                throw new BaseException.BadRequestException("invalid_input", "Page size must be greater than 0.");
            }

            var repository = _unitOfWork.GetRepository<Shop>();
            var query = repository.Entities.Where(shop => !shop.DeletedTime.HasValue);

            var totalItems = await query.CountAsync();
            var shops = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(shop => new ShopResponseModel
                {
                    Id = shop.Id.ToString(),
                    Name = shop.Name,
                    Description = shop.Description,
                    Rating = shop.Rating,
                    UserId = shop.UserId
                })
                .ToListAsync();

            return new PaginatedList<ShopResponseModel>(shops, totalItems, pageNumber, pageSize);
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

        public async Task<bool> UpdateShopAsync(string userId, CreateShopDto shop)
        {
            var repository = _unitOfWork.GetRepository<Shop>();
            var existingShop = await repository.Entities
                .FirstOrDefaultAsync(s => s.UserId.ToString() == userId && !s.DeletedTime.HasValue);

            if (shop == null)
            {
                throw new BaseException.NotFoundException("shop_not_found", "Shop not found.");
            }

            if (!string.IsNullOrWhiteSpace(shop.Name))
            {
                if (string.IsNullOrWhiteSpace(shop.Name))
                {
                    throw new BaseException.BadRequestException("invalid_shop_name", "Please input shop name.");
                }

                if (Regex.IsMatch(shop.Name, @"[^a-zA-Z\s]"))
                {
                    throw new BaseException.BadRequestException("invalid_shop_name_format", "Shop name can only contain letters and spaces.");
                }

                existingShop.Name = shop.Name;
            }

            if (!string.IsNullOrWhiteSpace(shop.Description))
            {
                if (string.IsNullOrWhiteSpace(shop.Description))
                {
                    throw new BaseException.BadRequestException("invalid_shop_description", "Please input shop description.");
                }

                if (Regex.IsMatch(shop.Description, @"[^a-zA-Z0-9\s]"))
                {
                    throw new BaseException.BadRequestException("invalid_shop_description_format", "Shop description cannot contain special characters.");
                }

                existingShop.Description = shop.Description;
            }

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

            var activeProducts = shop.Products.Where(p => p.DeletedTime == null).ToList();

            if (activeProducts == null || !activeProducts.Any())
            {
                return 0m;
            }

            var productRatings = activeProducts.Select(p => p.Rating).Where(r => r > 0).ToList();

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