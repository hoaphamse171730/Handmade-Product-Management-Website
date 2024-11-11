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
using Microsoft.IdentityModel.Tokens;
using System.Text.RegularExpressions;

namespace HandmadeProductManagement.Services.Service
{
    public class ShopService : IShopService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;

        public ShopService(IUnitOfWork unitOfWork, IUserService userService)
        {
            _unitOfWork = unitOfWork;
            _userService = userService;
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
        public async Task<bool> HaveShopAsync(Guid userId)
        {
            var userRepository = _unitOfWork.GetRepository<ApplicationUser>();
            var userExists = await userRepository.Entities
                .AnyAsync(u => u.Id == userId && !u.DeletedTime.HasValue);

            if (!userExists)
            {
                return false;
            }

            var shopRepository = _unitOfWork.GetRepository<Shop>();

            var hasShop = await shopRepository.Entities
                .AnyAsync(s => s.UserId == userId && !s.DeletedTime.HasValue);

            return hasShop;
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

                if (Regex.IsMatch(shop.Name, @"[^a-zA-Z0-9À-ỹáàảãạắằẳẵặéèẻẽẹíìỉĩịóòỏõọốồổỗộúùủũụưáàảãạắằẳẵặýỳỷỹỵđĐ\s]"))
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

            if (Regex.IsMatch(shop.Name, @"[^a-zA-Z0-9À-ỹáàảãạắằẳẵặéèẻẽẹíìỉĩịóòỏõọốồổỗộúùủũụưáàảãạắằẳẵặýỳỷỹỵđĐ\s]"))
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
        public async Task<ShopDto> AdminGetShopByIdAsync(string shopId)
        {
            if (string.IsNullOrEmpty(shopId))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidGuidFormat);
            }

            var repository = _unitOfWork.GetRepository<Shop>();
            var productRepository = _unitOfWork.GetRepository<Product>();

            var shop = await repository.Entities
                .Where(s => s.Id.ToString() == shopId && !s.DeletedTime.HasValue)
                .Select(shop => new ShopDto
                {
                    Id = shop.Id.ToString(),
                    Name = shop.Name,
                    Description = shop.Description!,
                    Rating = shop.Rating,
                    userId = shop.UserId.ToString(),
                })
                .FirstOrDefaultAsync();

            if (shop == null)
            {
                throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageShopNotFound);
            }

            // Fetch TotalSales if there are shipped orders related to the shop
            var shippedOrders = await _unitOfWork.GetRepository<Order>()
                .Entities
                .Where(order => order.Status == Constants.OrderStatusShipped && order.OrderDetails.Any(od => od!.ProductItem!.Product!.ShopId == shop.Id))
                .ToListAsync();

            shop.TotalSales = shippedOrders.Sum(order => order.TotalPrice);

            // Fetch the owner's name using the UserService
            var user = await _userService.GetById(shop.userId);
            shop.ownerName = user.UserName!;

            return shop;
        }

        public async Task<bool> DeleteShopByIdAsync(string id, string userId)
        {
            var shopRepo = _unitOfWork.GetRepository<Shop>();
            var shop = await shopRepo.Entities.FirstOrDefaultAsync(p => p.Id == id && p.DeletedTime == null);

            if (shop is null)
                throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(),
                    Constants.ErrorMessageShopNotFound);

            shop.LastUpdatedBy = userId;
            shop.DeletedTime = DateTime.UtcNow;

            await shopRepo.UpdateAsync(shop);
            await _unitOfWork.SaveAsync();
            return true;

        }

        public async Task<IList<ShopDto>> GetShopListByAdmin(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidPageNumber);
            }
            if (pageSize <= 0)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidPageSize);
            }
            var query = _unitOfWork.GetRepository<Shop>()
            .Entities
            .Where(shop => !shop.DeletedTime.HasValue)
            .AsQueryable();

            // Pagination: Apply Skip and Take
            var paginatedQuery = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);


            var shops = await paginatedQuery
                .Select(shop => new ShopDto
                {
                    Id = shop.Id.ToString(),
                    Name = shop.Name,
                    Description = shop.Description!,
                    Rating = shop.Rating,
                    userId = shop.UserId.ToString(),
                })
                .ToListAsync();

            // Fetch orders grouped by ShopId and calculate TotalSales for each shop
            var shippedOrders = await _unitOfWork.GetRepository<Order>()
                .Entities
                .Where(order => order.Status == Constants.OrderStatusShipped)
                .GroupBy(order => order.OrderDetails
                    .Select(od => od!.ProductItem!.Product!.ShopId).FirstOrDefault())
                .Select(group => new
                {
                    ShopId = group.Key,
                    TotalSales = group.Sum(order => order.TotalPrice)
                })
                .ToListAsync();

            var totalSalesByShopId = shippedOrders.ToDictionary(x => x.ShopId, x => x.TotalSales);

            foreach (var shop in shops)
            {
                // Set TotalSales if available, otherwise 0
                if (totalSalesByShopId.TryGetValue(shop.Id, out var totalSales))
                {
                    shop.TotalSales = totalSales;
                }
                else
                {
                    shop.TotalSales = 0;
                }

                // Fetch the owner's name using the UserService
                var user = await _userService.GetById(shop.userId);
                shop.ownerName = user.UserName!;
            }

            return shops;
        }
        public async Task<IList<ShopDto>> GetDeletedShops(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidPageNumber);
            }
            if (pageSize <= 0)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidPageSize);
            }
            var query = _unitOfWork.GetRepository<Shop>()
            .Entities
            .Where(shop => shop.DeletedTime.HasValue)
            .AsQueryable();

            // Pagination: Apply Skip and Take
            var paginatedQuery = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);


            var shops = await paginatedQuery
                .Select(shop => new ShopDto
                {
                    Id = shop.Id.ToString(),
                    Name = shop.Name,
                    Description = shop.Description!,
                    Rating = shop.Rating,
                    userId = shop.UserId.ToString(),
                })
                .ToListAsync();

            // Fetch orders grouped by ShopId and calculate TotalSales for each shop
            var shippedOrders = await _unitOfWork.GetRepository<Order>()
                .Entities
                .Where(order => order.Status == Constants.OrderStatusShipped)
                .GroupBy(order => order.OrderDetails
                    .Select(od => od!.ProductItem!.Product!.ShopId).FirstOrDefault())
                .Select(group => new
                {
                    ShopId = group.Key,
                    TotalSales = group.Sum(order => order.TotalPrice)
                })
                .ToListAsync();

            var totalSalesByShopId = shippedOrders.ToDictionary(x => x.ShopId, x => x.TotalSales);

            foreach (var shop in shops)
            {
                // Set TotalSales if available, otherwise 0
                if (totalSalesByShopId.TryGetValue(shop.Id, out var totalSales))
                {
                    shop.TotalSales = totalSales;
                }
                else
                {
                    shop.TotalSales = 0;
                }

                // Fetch the owner's name using the UserService
                var user = await _userService.GetById(shop.userId);
                shop.ownerName = user.UserName!;
            }

            return shops;
        }
        public async Task<bool> RecoverDeletedShopAsync(string shopId, string userId)
        {
            var shopRepository = _unitOfWork.GetRepository<Shop>();
            var shop = await shopRepository.Entities
                .FirstOrDefaultAsync(s => s.Id.ToString() == shopId && s.DeletedTime.HasValue)
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageShopNotFound);

            shop.DeletedBy = userId; 
            shop.DeletedTime = null;

            shopRepository.Update(shop);
            await _unitOfWork.SaveAsync();

            return true;
        }
    }
}