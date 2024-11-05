using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
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
                throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageUserNotFound);
            }

            var repository = _unitOfWork.GetRepository<Shop>();

            var existingShop = await repository.Entities
                .FirstOrDefaultAsync(s => s.UserId.ToString() == userId);

            if (existingShop != null)
            {
                if (existingShop.DeletedTime == null)
                {
                    throw new BaseException.BadRequestException(
                        StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageUserActiveShop);
                }
                else
                {
                    existingShop.Name = createShop.Name;
                    existingShop.Description = createShop.Description;
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
                throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageUserNotFound);
            }

            var shopRepository = _unitOfWork.GetRepository<Shop>();
            var shop = await shopRepository.Entities
                .FirstOrDefaultAsync(s => s.UserId.ToString() == userId && !s.DeletedTime.HasValue)
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageShopNotFound);

            shop.DeletedBy = userId;
            shop.DeletedTime = DateTime.UtcNow;

            shopRepository.Update(shop);
            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<ShopResponseModel> GetShopByIdAsync(string shopId)
        {
            if (string.IsNullOrEmpty(shopId))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidGuidFormat);
            }

            var repository = _unitOfWork.GetRepository<Shop>();
            var productRepository = _unitOfWork.GetRepository<Product>();

            var shop = await repository.Entities
                .Where(s => s.Id.ToString() == shopId && !s.DeletedTime.HasValue)
                .Select(shop => new ShopResponseModel
                {
                    Id = shop.Id.ToString(),
                    Name = shop.Name,
                    Description = shop.Description,
                    Rating = shop.Rating,
                    CreatedTime = shop.CreatedTime,
                    UserId = shop.UserId,
                    ProductCount = productRepository.Entities.Count(p => p.ShopId == shop.Id && (!p.DeletedTime.HasValue || p.DeletedBy == null))
                })
                .FirstOrDefaultAsync();

            if (shop == null)
            {
                throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageShopNotFound);
            }

            return shop;
        }

        public async Task<ShopResponseModel> GetShopByUserIdAsync(Guid userId)
        {
            var userRepository = _unitOfWork.GetRepository<ApplicationUser>();
            var userExists = await userRepository.Entities.AnyAsync(u => u.Id == userId && !u.DeletedTime.HasValue);
            if (!userExists)
            {
                throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageUserNotFound);
            }

            var repository = _unitOfWork.GetRepository<Shop>();
            var productRepository = _unitOfWork.GetRepository<Product>();

            var shop = await repository.Entities
                .FirstOrDefaultAsync(s => s.UserId == userId && !s.DeletedTime.HasValue)
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageShopNotFoundForUser);

            var productCount = await productRepository.Entities.CountAsync(p => p.ShopId == shop.Id && (!p.DeletedTime.HasValue || p.DeletedBy == null));

            return new ShopResponseModel
            {
                Id = shop.Id.ToString(),
                Name = shop.Name,
                Description = shop.Description,
                Rating = shop.Rating,
                CreatedTime = shop.CreatedTime,
                UserId = shop.UserId,
                ProductCount = productCount
            };
        }

        public async Task<bool> UpdateShopAsync(string userId, CreateShopDto shop)
        {
            var repository = _unitOfWork.GetRepository<Shop>();
            var existingShop = await repository.Entities
                .FirstOrDefaultAsync(s => s.UserId.ToString() == userId && !s.DeletedTime.HasValue);

            if (shop == null || existingShop == null) 
            {
                throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageShopNotFound);
            }

            if (!string.IsNullOrWhiteSpace(shop.Name))
            {
                if (string.IsNullOrWhiteSpace(shop.Name))
                {
                    throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidShopName);
                }

                if (Regex.IsMatch(shop.Name, @"[^a-zA-Z0-9\s]"))
                {
                    throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidShopNameFormat);
                }

                existingShop.Name = shop.Name;
            }

            if (!string.IsNullOrWhiteSpace(shop.Description))
            {
                if (string.IsNullOrWhiteSpace(shop.Description))
                {
                    throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidShopDescription);
                }

                if (Regex.IsMatch(shop.Description, @"[^a-zA-Z0-9\s]"))
                {
                    throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidShopDescriptionFormat);
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
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidShopName);
            }

            if (Regex.IsMatch(shop.Name, @"[^a-zA-Z0-9\s]"))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidShopNameFormat);
            }

            if (string.IsNullOrWhiteSpace(shop.Description))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidShopDescription);
            }

            if (Regex.IsMatch(shop.Description, @"[^a-zA-Z0-9\s]"))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidShopDescriptionFormat);
            }
        }

        public async Task<decimal> CalculateShopAverageRatingAsync(string shopId)
        {
            if (string.IsNullOrWhiteSpace(shopId))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageEmptyId);
            }

            if (!Guid.TryParse(shopId, out _))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidGuidFormat);
            }

            var shop = await _unitOfWork.GetRepository<Shop>().Entities
                                        .Include(s => s.Products)
                                        .FirstOrDefaultAsync(s => s.Id == shopId) ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageShopNotFound);

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

        public async Task<IList<ShopResponseModel>> GetAllShopsAsync()
        {
            var shops = await _unitOfWork.GetRepository<Shop>()
                .Entities
                .Where(shop => !shop.DeletedTime.HasValue)
                .Select(shop => new ShopResponseModel
                {
                    Id = shop.Id.ToString(),
                    Name = shop.Name
                })
                .ToListAsync();

            return shops;
        }
    }
}