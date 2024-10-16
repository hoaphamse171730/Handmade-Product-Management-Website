using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.ModelViews.ProductDetailModelViews;
using HandmadeProductManagement.ModelViews.ProductModelViews;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using FluentValidation;
using HandmadeProductManagement.ModelViews.VariationCombinationModelViews;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Common;

namespace HandmadeProductManagement.Services.Service
{
    public class ProductService : IProductService
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<ProductForCreationDto> _creationValidator;
        private readonly IValidator<ProductForUpdateDto> _updateValidator;
        private readonly IValidator<VariationCombinationDto> _variationCombinationValidator;
        private readonly IPromotionService _promotionService;

        public ProductService(IUnitOfWork unitOfWork, IMapper mapper,
            IValidator<ProductForCreationDto> creationValidator, IValidator<ProductForUpdateDto> updateValidator, IValidator<VariationCombinationDto> variationCombinationValidator, IPromotionService promotionService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _creationValidator = creationValidator;
            _updateValidator = updateValidator;
            _variationCombinationValidator = variationCombinationValidator;
            _promotionService = promotionService;
        }

        public async Task<bool> Create(ProductForCreationDto productDto, string userId)
        {
            // Step 1: Validate the product creation DTO
            var validationResult = await _creationValidator.ValidateAsync(productDto);
            if (!validationResult.IsValid)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageValidationFailed);
            }

            // Step 2: Validate VariationCombinationDtos
            foreach (var variationCombination in productDto.VariationCombinations)
            {
                var variationCombinationValidationResult = await _variationCombinationValidator.ValidateAsync(variationCombination);
                if (!variationCombinationValidationResult.IsValid)
                {
                    throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), validationResult.Errors.First().ErrorMessage);
                }
            }

            _unitOfWork.BeginTransaction();

            try
            {
                // Step 3: Validate if the Category exists and if its ID is a valid GUID
                if (!Guid.TryParse(productDto.CategoryId, out _))
                {
                    throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidGuidFormat);
                }

                var categoryExists = await _unitOfWork.GetRepository<Category>().Entities
                    .AnyAsync(c => c.Id == productDto.CategoryId);
                if (!categoryExists)
                {
                    throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageCategoryNotFound);
                }

                // Step 4: Get the ShopId based on userId
                var shop = await _unitOfWork.GetRepository<Shop>().Entities
                    .FirstOrDefaultAsync(s => s.UserId == Guid.Parse(userId)) ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageShopNotFound);

                // Step 5: Map the product DTO to the Product entity
                var productEntity = _mapper.Map<Product>(productDto);
                productEntity.ShopId = shop.Id;
                productEntity.Status = Constants.ProductStatusAvailable;
                productEntity.CreatedBy = userId;
                productEntity.LastUpdatedBy = userId;

                // Step 6: Insert the product into the repository
                await _unitOfWork.GetRepository<Product>().InsertAsync(productEntity);
                await _unitOfWork.SaveAsync();

                // Step 7: Validate if each Variation exists and if its ID is a valid GUID
                foreach (var variation in productDto.Variations)
                {
                    if (!Guid.TryParse(variation.Id, out _))
                    {
                        throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidGuidFormat);
                    }

                    var variationExists = await _unitOfWork.GetRepository<Variation>().Entities
                        .AnyAsync(v => v.Id == variation.Id);
                    if (!variationExists)
                    {
                        throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageVariationNotFound);
                    }
                }

                // Validate if each VariationOption belongs to the correct Variation and if its ID is a valid GUID
                foreach (var variation in productDto.Variations)
                {
                    foreach (var variationOptionId in variation.VariationOptionIds)
                    {
                        if (!Guid.TryParse(variationOptionId, out _))
                        {
                            throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidGuidFormat);
                        }

                        // Validate if the VariationOption belongs to the correct Variation
                        var variationOptionExists = await _unitOfWork.GetRepository<VariationOption>().Entities
                            .AnyAsync(vo => vo.Id == variationOptionId && vo.VariationId == variation.Id);

                        if (!variationOptionExists)
                        {
                            throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(),
                                string.Format(Constants.ErrorMessageVariationOptionNotBelongToVariation, variationOptionId, variation.Id));
                        }
                    }
                }

                // Step 8: Validate if each VariationOption exists and if its ID is a valid GUID
                foreach (var variation in productDto.Variations)
                {
                    foreach (var variationOptionId in variation.VariationOptionIds)
                    {
                        if (!Guid.TryParse(variationOptionId, out _))
                        {
                            throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidGuidFormat);
                        }

                        var variationOptionExists = await _unitOfWork.GetRepository<VariationOption>().Entities
                            .AnyAsync(vo => vo.Id == variationOptionId);
                        if (!variationOptionExists)
                        {
                            throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageVariationOptionNotFound);
                        }
                    }
                }

                // Step 9: Handle the variations and collect VariationOptionIds per variation
                var variationOptionIdsPerVariation = new Dictionary<string, List<string>>();
                foreach (var variation in productDto.Variations)
                {
                    variationOptionIdsPerVariation[variation.Id] = variation.VariationOptionIds;
                }

                // Step 10: Generate all combinations of VariationOptionIds
                var variationCombinations = GetVariationOptionCombinations(variationOptionIdsPerVariation);

                // Step 11: Check if all required combinations exist in VariationCombinations
                var providedCombinations = productDto.VariationCombinations
                    .Select(vc => string.Join("-", vc.VariationOptionIds.OrderBy(id => id)))
                    .ToList();

                var validCombinations = variationCombinations
                    .Select(vc => string.Join("-", vc.VariationOptionIds.OrderBy(id => id)))
                    .ToList();

                // Step 12: Compare combinations
                if (!validCombinations.All(providedCombinations.Contains))
                {
                    throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageIncompleteCombinations);
                }

                // Step 13: Create ProductItems and ProductConfigurations for each VariationCombination
                await AddVariationOptionsToProduct(productEntity, productDto.VariationCombinations, userId);

                _unitOfWork.CommitTransaction();
                return true;
            }
            catch (Exception)
            {
                _unitOfWork.RollBack();
                throw;
            }
        }

        public async Task AddVariationOptionsToProduct(Product product, List<VariationCombinationDto> variationCombinations, string userId)
        {
            foreach (var combination in variationCombinations)
            {
                var productItem = new ProductItem
                {
                    ProductId = product.Id,
                    Price = combination.Price,
                    QuantityInStock = combination.QuantityInStock,
                    CreatedBy = userId,
                    LastUpdatedBy = userId
                };

                await _unitOfWork.GetRepository<ProductItem>().InsertAsync(productItem);
                await _unitOfWork.SaveAsync();

                foreach (var variationOptionId in combination.VariationOptionIds)
                {
                    var productConfiguration = new ProductConfiguration
                    {
                        ProductItemId = productItem.Id,
                        VariationOptionId = variationOptionId
                    };

                    await _unitOfWork.GetRepository<ProductConfiguration>().InsertAsync(productConfiguration);
                }

                await _unitOfWork.SaveAsync();
            }
        }

        // Method to generate all combinations of VariationOptionIds
        private List<VariationCombinationDto> GetVariationOptionCombinations(Dictionary<string, List<string>> variationOptionIdsPerVariation)
        {
            // Generate all possible combinations of variation options based on the IDs per variation
            var allCombinations = new List<VariationCombinationDto>();

            var lists = variationOptionIdsPerVariation.Values.ToList();

            var combinations = GetCombinations(lists);

            foreach (var combination in combinations)
            {
                allCombinations.Add(new VariationCombinationDto
                {
                    VariationOptionIds = combination
                });
            }

            return allCombinations;
        }

        // Method to generate all possible combinations of elements from multiple lists
        private IEnumerable<List<string>> GetCombinations(List<List<string>> lists)
        {
            IEnumerable<IEnumerable<string>> result = new List<List<string>> { new List<string>() };

            foreach (var list in lists)
            {
                result = from combination in result
                         from item in list
                         select combination.Concat(new[] { item });
            }

            return result.Select(c => c.ToList());
        }

        public async Task<IEnumerable<ProductSearchVM>> SearchProductsAsync(ProductSearchFilter searchFilter, int pageNumber, int pageSize)
        {
            if (pageNumber <= 0)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidPageNumber);
            }
            if (pageSize <= 0)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidPageSize);
            }

            // Validate CategoryId and ShopId datatype (Guid)
            if (!string.IsNullOrWhiteSpace(searchFilter.CategoryId) && !IsValidGuid(searchFilter.CategoryId))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidGuidFormat);
            }

            if (!string.IsNullOrWhiteSpace(searchFilter.ShopId) && !IsValidGuid(searchFilter.ShopId))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidGuidFormat);
            }

            // Validate MinRating limit (from 0 to 5)
            if (searchFilter.MinRating.HasValue && (searchFilter.MinRating < 0 || searchFilter.MinRating > 5))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageMinRatingOutOfRange);
            }

            var query = _unitOfWork.GetRepository<Product>().Entities
                                    .Include(p => p.ProductImages)
                                    .Include(p => p.ProductItems)
                                    .Where(p => !p.DeletedTime.HasValue || p.DeletedBy == null)
                                    .AsQueryable();

            // Apply Search Filters
            if (!string.IsNullOrWhiteSpace(searchFilter.Name))
            {
                query = query.Where(p => p.Name.Contains(searchFilter.Name));
            }

            if (!string.IsNullOrWhiteSpace(searchFilter.CategoryId))
            {
                query = query.Where(p => p.CategoryId == searchFilter.CategoryId);
            }

            if (!string.IsNullOrWhiteSpace(searchFilter.ShopId))
            {
                query = query.Where(p => p.ShopId == searchFilter.ShopId);
            }

            if (!string.IsNullOrWhiteSpace(searchFilter.Status))
            {
                query = query.Where(p => p.Status == searchFilter.Status);
            }

            if (searchFilter.MinRating.HasValue)
            {
                query = query.Where(p => p.Rating >= searchFilter.MinRating.Value);
            }

            // Sort Logic
            if (searchFilter.SortByPrice)
            {
                query = searchFilter.SortDescending
                    ? query.OrderByDescending(p => p.ProductItems.Min(pi => pi.Price))
                    : query.OrderBy(p => p.ProductItems.Min(pi => pi.Price));
            }
            else
            {
                query = searchFilter.SortDescending
                    ? query.OrderByDescending(p => p.Rating)
                    : query.OrderBy(p => p.Rating);
            }

            // Pagination: Apply Skip and Take
            var totalItems = await query.CountAsync();
            var paginatedQuery = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            // Now we group and project the result into ProductSearchVM
            var productSearchVMs = await paginatedQuery
                .Select(p => new ProductSearchVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    CategoryId = p.CategoryId,
                    ShopId = p.ShopId,
                    Rating = p.Rating,
                    Status = p.Status,
                    SoldCount = p.SoldCount,
                    ProductImageUrl = p.ProductImages.FirstOrDefault() != null ? p.ProductImages.FirstOrDefault().Url : string.Empty,
                    LowestPrice = p.ProductItems.Any() ? p.ProductItems.Min(pi => pi.Price) : 0
                })
                .ToListAsync();

            // Return the paginated result and total items count
            return productSearchVMs;
        }

        // Sort Function

        //public async Task<IEnumerable<ProductSearchVM>> SortProductsAsync(ProductSortFilter sortFilter)
        //{
        //    var query = _unitOfWork.GetRepository<Product>().Entities
        //        .Include(p => p.ProductItems)
        //        .Include(p => p.Reviews)
        //        .AsQueryable();

        //    // Sort by Price
        //    if (sortFilter.SortByPrice)
        //    {
        //        query = sortFilter.SortDescending
        //            ? query.OrderByDescending(p => p.ProductItems.Min(pi => pi.Price))
        //            : query.OrderBy(p => p.ProductItems.Min(pi => pi.Price));
        //    }

        //    // Sort by Rating
        //    else if (sortFilter.SortByRating)
        //    {
        //        query = sortFilter.SortDescending
        //            ? query.OrderByDescending(p => p.Rating)
        //            : query.OrderBy(p => p.Rating);
        //    }

        //    var products = await query.ToListAsync();

        //    var productSearchVMs = products.Select(p => new ProductSearchVM
        //    {
        //        Id = p.Id,
        //        Name = p.Name,
        //        Description = p.Description,
        //        CategoryId = p.CategoryId,
        //        ShopId = p.ShopId,
        //        Rating = p.Rating,
        //        Status = p.Status,
        //        SoldCount = p.SoldCount,
        //        Price = p.ProductItems.Any() ? p.ProductItems.Min(pi => pi.Price) : 0
        //    });
        //    if (productSearchVMs.IsNullOrEmpty())
        //    {
        //        throw new BaseException.NotFoundException("not_found", "Product Not Found");
        //    }

        //    return productSearchVMs;

        //}

        public async Task<IList<ProductOverviewDto>> GetProductsByUserByPage(string userId, int pageNumber, int pageSize)
        {
            if (pageNumber <= 0)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidPageNumber);
            }
            if (pageSize <= 0)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidPageSize);
            }

            if (!Guid.TryParse(userId, out var guidId))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidGuidFormat);
            }

            var shop = await _unitOfWork.GetRepository<Shop>().Entities
                 .FirstOrDefaultAsync(s => s.UserId == guidId && (s.DeletedBy == null || !s.DeletedTime.HasValue))
                 ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageShopNotFound);


            var productsQuery = _unitOfWork.GetRepository<Product>().Entities
                .Where(p => p.ShopId == shop.Id &&
                            (!p.DeletedTime.HasValue || p.DeletedBy == null))
                .Include(p => p.ProductImages)
                .Include(p => p.ProductItems)
                .AsQueryable();

            var totalItems = await productsQuery.CountAsync();
            var products = await productsQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var productsOverviewDto = products.Select(p => new ProductOverviewDto
            {
                Id = p.Id,
                Name = p.Name,
                CategoryId = p.CategoryId,
                ProductImageUrl = p.ProductImages.FirstOrDefault()?.Url ?? string.Empty,
                Rating = p.Rating,
                SoldCount = p.SoldCount,
                Status = p.Status,
                LowestPrice = p.ProductItems.Any() ? p.ProductItems.Min(pi => pi.Price) : 0
            }).ToList();

            return productsOverviewDto;
        }

        public async Task<ProductDto> GetById(string id)
        {
            var product = await _unitOfWork.GetRepository<Product>().Entities
                .FirstOrDefaultAsync(p => p.Id == id && (!p.DeletedTime.HasValue || p.DeletedBy == null))
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString() ,Constants.ErrorMessageProductNotFound);

            var productToReturn = _mapper.Map<ProductDto>(product);
            return productToReturn;
        }

        public async Task<bool> Update(string id, ProductForUpdateDto product, string userId)
        {
            var validationResult = await _updateValidator.ValidateAsync(product);
            if (!validationResult.IsValid)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageValidationFailed);
            }

            // Fetch product and check if it exists
            var productEntity = await _unitOfWork.GetRepository<Product>().Entities
                .FirstOrDefaultAsync(p => p.Id == id) ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageProductNotFound);

            // Check if the user has permission to update the product
            if (productEntity.CreatedBy != userId)
            {
                throw new BaseException.ForbiddenException(StatusCodeHelper.Forbidden.ToString(), Constants.ErrorMessageForbidden);
            }

            // Update fields if provided
            if (!string.IsNullOrWhiteSpace(product.Name))
            {
                productEntity.Name = product.Name;
            }

            if (!string.IsNullOrWhiteSpace(product.Description))
            {
                productEntity.Description = product.Description;
            }

            if (!string.IsNullOrWhiteSpace(product.CategoryId))
            {
                productEntity.CategoryId = product.CategoryId;
            }

            // Update metadata
            productEntity.LastUpdatedTime = DateTime.UtcNow;
            productEntity.LastUpdatedBy = userId;
            await _unitOfWork.GetRepository<Product>().UpdateAsync(productEntity);
            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<bool> SoftDelete(string id, string userId)
        {
            // Fetch product and check if it exists
            var productRepo = _unitOfWork.GetRepository<Product>();
            var productEntity = await productRepo.Entities
                .FirstOrDefaultAsync(p => p.Id == id)
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageProductNotFound);

            // Check if the user has permission to delete the product
            if (productEntity.CreatedBy != userId)
            {
                throw new BaseException.ForbiddenException(StatusCodeHelper.Forbidden.ToString(), Constants.ErrorMessageForbidden);
            }

            // Mark as soft deleted
            productEntity.DeletedTime = DateTime.UtcNow;
            productEntity.DeletedBy = userId;

            await productRepo.UpdateAsync(productEntity);
            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<IList<Product>> GetAllDeletedProducts(int pageNumber, int pageSize)
        {
            var deletedProductsQuery = _unitOfWork.GetRepository<Product>().Entities
                .Where(p => p.DeletedTime.HasValue || p.DeletedBy != null);

            var totalRecords = await deletedProductsQuery.CountAsync();

            var deletedProducts = await deletedProductsQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return deletedProducts;
        }

        public async Task<bool> RecoverProduct(string id, string userId)
        {
            var productRepo = _unitOfWork.GetRepository<Product>();
            var productEntity = await productRepo.Entities.FirstOrDefaultAsync(x => x.Id == id);

            // Check if the product exists and has been deleted
            if (productEntity == null || productEntity.DeletedBy == null || !productEntity.DeletedTime.HasValue)
                throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageProductNotFound);

            // Recover the product
            productEntity.DeletedTime = null;
            productEntity.DeletedBy = null;
            productEntity.LastUpdatedBy = userId;

            await productRepo.UpdateAsync(productEntity);
            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<bool> UpdateStatusProduct(string productId, bool isAvailable, string userId)
        {
            // Retrieve the product
            var productEntity = await _unitOfWork.GetRepository<Product>().Entities
                .FirstOrDefaultAsync(p => p.Id == productId && (!p.DeletedTime.HasValue || p.DeletedBy == null))
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageProductNotFound);

            // Check if the current user is the creator of the product
            if (productEntity.CreatedBy != userId)
            {
                throw new BaseException.ForbiddenException(StatusCodeHelper.Forbidden.ToString(), Constants.ErrorMessageForbidden);
            }

            var newStatus = isAvailable ? Constants.ProductStatusAvailable : Constants.ProductStatusUnavailable;

            // Check if the status is the same as the current status
            if (newStatus == productEntity.Status)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), string.Format(Constants.ErrorMessageProductAlreadyHasStatus, productEntity.Status));
            }

            productEntity.Status = newStatus;
            productEntity.LastUpdatedBy = userId;
            productEntity.LastUpdatedTime = DateTime.UtcNow;

            await _unitOfWork.SaveAsync();

            return true;
        }

        private bool IsValidGuid(string input) => Guid.TryParse(input, out _);

        public async Task<ProductDetailResponseModel> GetProductDetailsByIdAsync(string productId)
        {
            if (string.IsNullOrWhiteSpace(productId) || !IsValidGuid(productId))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidGuidFormat);
            }

            var product = await _unitOfWork.GetRepository<Product>().Entities
                .Include(p => p.Category)
                .ThenInclude(p => p.Promotion)
                .Include(p => p.Shop)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductItems)
                .ThenInclude(p => p.ProductConfigurations)
                .ThenInclude(p => p.VariationOption)
                .ThenInclude(v => v.Variation)
                .FirstOrDefaultAsync(p => p.Id == productId)
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageProductNotFound);

            var promotion = await _unitOfWork.GetRepository<Promotion>().Entities
                .FirstOrDefaultAsync(p => p.Categories.Any(c => c.Id == product.CategoryId) &&
                                          p.Status == Constants.PromotionStatusActive);

            var response = new ProductDetailResponseModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                CategoryId = product.CategoryId,
                CategoryName = product.Category.Name,
                ShopId = product.ShopId,
                ShopName = product.Shop.Name,
                Rating = product.Rating,
                Status = product.Status,
                SoldCount = product.SoldCount,
                ProductImageUrls = product.ProductImages.Select(pi => pi.Url).ToList(),
                ProductItems = product.ProductItems.Select(pi => new ProductItemDetailModel
                {
                    Id = pi.Id,
                    QuantityInStock = pi.QuantityInStock,
                    Price = pi.Price,
                    DiscountedPrice = promotion != null ? (int)(pi.Price * (1 - promotion.DiscountRate)) : null,
                    Configurations = pi.ProductConfigurations.Select(pc => new ProductConfigurationDetailModel
                    {
                        VariationName = pc.VariationOption.Variation.Name,
                        OptionName = pc.VariationOption.Value
                    }).ToList()
                }).ToList(),
                Promotion = promotion != null
                    ? new PromotionDetailModel
                    {
                        Id = promotion.Id,
                        Name = promotion.Name,
                        Description = promotion.Description,
                        DiscountRate = promotion.DiscountRate,
                        StartDate = promotion.StartDate,
                        EndDate = promotion.EndDate,
                        Status = promotion.Status
                    }
                    : null
            };
            return response;
        }
        public async Task<decimal> CalculateAverageRatingAsync(string productId)
        {
            if (string.IsNullOrWhiteSpace(productId))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidGuidFormat);
            }

            if (!IsValidGuid(productId))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidGuidFormat);
            }

            var product = await _unitOfWork.GetRepository<Product>().Entities
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(p => p.Id == productId)
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageProductNotFound);

            var activeReviews = product.Reviews.Where(r => r.DeletedTime == null).ToList();

            if (activeReviews == null || !activeReviews.Any())
            {
                return 0m;
            }

            decimal averageRating = Math.Round((decimal)product.Reviews.Average(r => r.Rating), 1);

            // Update the product's rating
            product.Rating = averageRating;
            await _unitOfWork.GetRepository<Product>().UpdateAsync(product);
            await _unitOfWork.SaveAsync();

            return averageRating;
        }

        public async Task UpdateProductSoldCountAsync(string orderId)
        {
            var order = await _unitOfWork.GetRepository<Order>().Entities
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.ProductItem)
                .ThenInclude(pi => pi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId)
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageOrderNotFound);

            if (order.Status != Constants.OrderStatusShipped)
            {
                return; // Only update soldCount when the order status is "Shipped"
            }

            foreach (var orderDetail in order.OrderDetails)
            {
                var product = orderDetail.ProductItem.Product;
                product.SoldCount += orderDetail.ProductQuantity;
                await _unitOfWork.GetRepository<Product>().UpdateAsync(product);
            }

            await _unitOfWork.SaveAsync();
        }

    }
}

