using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.ModelViews.ProductDetailModelViews;
using HandmadeProductManagement.ModelViews.ProductModelViews;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using FluentValidation;
using Microsoft.IdentityModel.Tokens;
using HandmadeProductManagement.ModelViews.VariationCombinationModelViews;
using System.Linq;

namespace HandmadeProductManagement.Services.Service
{
    public class ProductService : IProductService
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<ProductForCreationDto> _creationValidator;
        private readonly IValidator<ProductForUpdateDto> _updateValidator;
        private readonly IValidator<VariationCombinationDto> _variationCombinationValidator;

        public ProductService(IUnitOfWork unitOfWork, IMapper mapper,
            IValidator<ProductForCreationDto> creationValidator, IValidator<ProductForUpdateDto> updateValidator, IValidator<VariationCombinationDto> variationCombinationValidator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _creationValidator = creationValidator;
            _updateValidator = updateValidator;
            _variationCombinationValidator = variationCombinationValidator;
        }

        public async Task<bool> Create(ProductForCreationDto productDto, string userId)
        {
            // Step 1: Validate the product creation DTO
            var validationResult = await _creationValidator.ValidateAsync(productDto);
            if (!validationResult.IsValid)
            {
                throw new BaseException.BadRequestException("validation_failed", validationResult.Errors.Select(e => e.ErrorMessage).FirstOrDefault());
            }

            // Step 2: Validate VariationCombinationDtos
            foreach (var variationCombination in productDto.VariationCombinations)
            {
                var variationCombinationValidationResult = await _variationCombinationValidator.ValidateAsync(variationCombination);
                if (!variationCombinationValidationResult.IsValid)
                {
                    throw new BaseException.BadRequestException("validation_failed", variationCombinationValidationResult.Errors.Select(e => e.ErrorMessage).FirstOrDefault());
                }
            }

            _unitOfWork.BeginTransaction();

            try
            {
                // Step 3: Validate if the Category exists and if its ID is a valid GUID
                if (!Guid.TryParse(productDto.CategoryId, out _))
                {
                    throw new BaseException.BadRequestException("invalid_category_id", "Category ID must be a valid GUID.");
                }

                var categoryRepository = _unitOfWork.GetRepository<Category>();
                var categoryExists = await categoryRepository.Entities
                    .AnyAsync(c => c.Id == productDto.CategoryId);
                if (!categoryExists)
                {
                    throw new BaseException.NotFoundException("category_not_found", $"Category not found.");
                }

                // Step 4: Validate if the Shop exists and if its ID is a valid GUID
                if (!Guid.TryParse(productDto.ShopId, out _))
                {
                    throw new BaseException.BadRequestException("invalid_shop_id", "Shop ID must be a valid GUID.");
                }

                var shopRepository = _unitOfWork.GetRepository<Shop>();
                var shopExists = await shopRepository.Entities
                    .AnyAsync(s => s.Id == productDto.ShopId);
                if (!shopExists)
                {
                    throw new BaseException.NotFoundException("shop_not_found", $"Shop not found.");
                }

                // Step 5: Map the product DTO to the Product entity
                var productEntity = _mapper.Map<Product>(productDto);
                productEntity.Status = "Available";
                productEntity.CreatedBy = userId;
                productEntity.LastUpdatedBy = userId;

                // Step 6: Insert the product into the repository
                await _unitOfWork.GetRepository<Product>().InsertAsync(productEntity);
                await _unitOfWork.SaveAsync();

                // Step 7: Validate if each Variation exists and if its ID is a valid GUID
                var variationRepository = _unitOfWork.GetRepository<Variation>();

                foreach (var variation in productDto.Variations)
                {
                    if (!Guid.TryParse(variation.Id, out _))
                    {
                        throw new BaseException.BadRequestException("invalid_variation_id", $"Variation ID must be a valid GUID.");
                    }

                    var variationExists = await variationRepository.Entities
                        .AnyAsync(v => v.Id == variation.Id);
                    if (!variationExists)
                    {
                        throw new BaseException.NotFoundException("variation_not_found", $"Variation not found.");
                    }
                }

                // Step 8: Validate if each VariationOption exists and if its ID is a valid GUID
                var variationOptionRepository = _unitOfWork.GetRepository<VariationOption>();

                foreach (var variation in productDto.Variations)
                {
                    foreach (var variationOptionId in variation.VariationOptionIds)
                    {
                        if (!Guid.TryParse(variationOptionId, out _))
                        {
                            throw new BaseException.BadRequestException("invalid_variation_option_id", $"Variation Option ID must be a valid GUID.");
                        }

                        var variationOptionExists = await variationOptionRepository.Entities
                            .AnyAsync(vo => vo.Id == variationOptionId);
                        if (!variationOptionExists)
                        {
                            throw new BaseException.NotFoundException("variation_option_not_found", $"Variation Option with ID not found.");
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
                    throw new BaseException.BadRequestException("incomplete_combinations", "Some required variation combinations are missing.");
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

                // Insert ProductItem
                await _unitOfWork.GetRepository<ProductItem>().InsertAsync(productItem);
                await _unitOfWork.SaveAsync();

                // Create ProductConfiguration for each VariationOption in the combination
                foreach (var variationOptionId in combination.VariationOptionIds)
                {
                    var productConfiguration = new ProductConfiguration
                    {
                        ProductItemId = productItem.Id,
                        VariationOptionId = variationOptionId
                    };

                    // Insert ProductConfiguration
                    await _unitOfWork.GetRepository<ProductConfiguration>().InsertAsync(productConfiguration);
                }

                // Save ProductConfiguration changes
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

        public async Task<IEnumerable<ProductSearchVM>> SearchProductsAsync(ProductSearchFilter searchModel)
        {
            // Validate CategoryId and ShopId datatype (Guid)
            if (!string.IsNullOrWhiteSpace(searchModel.CategoryId) && !IsValidGuid(searchModel.CategoryId))
            {
                throw new BaseException.BadRequestException("bad_request", "Invalid Category Id");
            }

            if (!string.IsNullOrWhiteSpace(searchModel.ShopId) && !IsValidGuid(searchModel.ShopId))
            {
                throw new BaseException.BadRequestException("bad_request", "Invalid Shop ID");
            }

            // Validate MinRating limit (from 0 to 5)
            if (searchModel.MinRating.HasValue && (searchModel.MinRating < 0 || searchModel.MinRating > 5))
            {
                throw new BaseException.BadRequestException("bad_request", "MinRating must be between 0 and 5.");
            }

            // Check if CategoryId exists
            if (!string.IsNullOrWhiteSpace(searchModel.CategoryId))
            {
                var categoryExists = await _unitOfWork.GetRepository<Category>().Entities
                    .AnyAsync(c => c.Id == searchModel.CategoryId);
                if (!categoryExists)
                {
                    throw new BaseException.NotFoundException("not_found", "Category Not Found");
                }
            }

            // Check if ShopId exists
            if (!string.IsNullOrWhiteSpace(searchModel.ShopId))
            {
                var shopExists = await _unitOfWork.GetRepository<Shop>().Entities
                    .AnyAsync(s => s.Id == searchModel.ShopId);
                if (!shopExists)
                {
                    throw new BaseException.NotFoundException("not_found", "Shop Not Found");
                }
            }

            var query = _unitOfWork.GetRepository<Product>().Entities.AsQueryable();

            // Apply server-side filters
            if (!string.IsNullOrEmpty(searchModel.Name))
            {
                //Skip normalization on the server-side 
                query = query.Where(p => p.Name != null);  // Placeholder to avoid mixing EF logic - if you delete this sentence, search go wrong 
            }

            if (!string.IsNullOrWhiteSpace(searchModel.CategoryId))
            {
                query = query.Where(p => p.CategoryId == searchModel.CategoryId);
            }

            if (!string.IsNullOrWhiteSpace(searchModel.ShopId))
            {
                query = query.Where(p => p.ShopId == searchModel.ShopId);
            }

            if (!string.IsNullOrWhiteSpace(searchModel.Status))
            {
                query = query.Where(p => p.Status == searchModel.Status);
            }

            if (searchModel.MinRating.HasValue)
            {
                query = query.Where(p => p.Rating >= searchModel.MinRating.Value);
            }


            // Sort Logic
            if (searchModel.SortByPrice)
            {
                query = searchModel.SortDescending
                    ? query.OrderByDescending(p => p.ProductItems.Min(pi => pi.Price))
                    : query.OrderBy(p => p.ProductItems.Min(pi => pi.Price));
            }

            else
            {
                query = searchModel.SortDescending
                    ? query.OrderByDescending(p => p.Rating)
                    : query.OrderBy(p => p.Rating);
            }



            var productSearchVMs = await query
                .GroupBy(p => new
                {
                    p.Id,
                    p.Name,
                    p.Description,
                    p.CategoryId,
                    p.ShopId,
                    p.Rating,
                    p.Status,
                    p.SoldCount
                })
                .Select(g => new ProductSearchVM
                {
                    Id = g.Key.Id,
                    Name = g.Key.Name,
                    Description = g.Key.Description,
                    CategoryId = g.Key.CategoryId,
                    ShopId = g.Key.ShopId,
                    Rating = g.Key.Rating,
                    Status = g.Key.Status,
                    SoldCount = g.Key.SoldCount,
                    // Avoid duplicates
                    Price = g.SelectMany(p => p.ProductItems).Any()
                        ? g.SelectMany(p => p.ProductItems).Min(pi => pi.Price)
                        : 0
                }).ToListAsync();

            // Search Normalized name - using Client-Side filter
            if (!string.IsNullOrEmpty(searchModel.Name))
            {
                var normalizedSearchName = NormalizeString(searchModel.Name);
                productSearchVMs = productSearchVMs.Where(p => NormalizeString(p.Name).Contains(normalizedSearchName)).ToList();
            }

            // Sort by price or rating after filtering
            productSearchVMs = searchModel.SortByPrice
                ? productSearchVMs.OrderBy(pr => searchModel.SortDescending ? -pr.Price : pr.Price).ToList()
                : productSearchVMs.OrderBy(pr => searchModel.SortDescending ? -pr.Rating : pr.Rating).ToList();


            if (productSearchVMs.IsNullOrEmpty())
            {
                throw new BaseException.NotFoundException("not_found", "Product Not Found");
            }

            return productSearchVMs;

        }


        // Sort Function

        public async Task<IEnumerable<ProductSearchVM>> SortProductsAsync(ProductSortFilter sortFilter)
        {
            var query = _unitOfWork.GetRepository<Product>().Entities
                .Include(p => p.ProductItems)
                .Include(p => p.Reviews)
                .AsQueryable();

            // Sort by Price
            if (sortFilter.SortByPrice)
            {
                query = sortFilter.SortDescending
                    ? query.OrderByDescending(p => p.ProductItems.Min(pi => pi.Price))
                    : query.OrderBy(p => p.ProductItems.Min(pi => pi.Price));
            }

            // Sort by Rating
            else if (sortFilter.SortByRating)
            {
                query = sortFilter.SortDescending
                    ? query.OrderByDescending(p => p.Rating)
                    : query.OrderBy(p => p.Rating);
            }

            var products = await query.ToListAsync();

            var productSearchVMs = products.Select(p => new ProductSearchVM
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                CategoryId = p.CategoryId,
                ShopId = p.ShopId,
                Rating = p.Rating,
                Status = p.Status,
                SoldCount = p.SoldCount,
                Price = p.ProductItems.Any() ? p.ProductItems.Min(pi => pi.Price) : 0
            });
            if (productSearchVMs.IsNullOrEmpty())
            {
                throw new BaseException.NotFoundException("not_found", "Product Not Found");
            }

            return productSearchVMs;

        }

        public async Task<IList<ProductDto>> GetAll()
        {
            var productRepo = _unitOfWork.GetRepository<Product>();
            var products = await productRepo.Entities
                .ToListAsync();
            var productsDto = _mapper.Map<IList<ProductDto>>(products);
            return productsDto;
        }

        public async Task<ProductDto> GetById(string id)
        {
            var product = await _unitOfWork.GetRepository<Product>().Entities
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                throw new KeyNotFoundException("Product not found");

            var productToReturn = _mapper.Map<ProductDto>(product);
            return productToReturn;

        }

        public async Task<bool> Update(string id, ProductForUpdateDto product, string userId)
        {
            var result = _updateValidator.ValidateAsync(product);
            if (!result.Result.IsValid)
                throw new ValidationException(result.Result.Errors);
            var productEntity = await _unitOfWork.GetRepository<Product>().Entities
                .FirstOrDefaultAsync(p => p.Id == id);
            if (productEntity == null)
                throw new KeyNotFoundException("Product not found");
            _mapper.Map(product, productEntity);

            productEntity.LastUpdatedTime = DateTime.UtcNow;
            productEntity.LastUpdatedBy = userId;

            await _unitOfWork.GetRepository<Product>().UpdateAsync(productEntity);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> SoftDelete(string id, string userId)
        {
            var productRepo = _unitOfWork.GetRepository<Product>();
            var productEntity = await productRepo.Entities.FirstOrDefaultAsync(x => x.Id == id.ToString());
            if (productEntity == null)
                throw new KeyNotFoundException("Product not found");
            productEntity.DeletedTime = DateTime.UtcNow;
            productEntity.DeletedBy = userId;
            await productRepo.UpdateAsync(productEntity);
            await _unitOfWork.SaveAsync();
            return true;
        }

        private bool IsValidGuid(string input) => Guid.TryParse(input, out _);

        public async Task<ProductDetailResponseModel> GetProductDetailsByIdAsync(string productId)
        {
            if (string.IsNullOrEmpty(productId) || !IsValidGuid(productId))
            {
                throw new BaseException.BadRequestException("invalid_product_id", "Product ID is invalid or empty.");
            }

            var product = await _unitOfWork.GetRepository<Product>().Entities
                .Include(p => p.Category)
                .ThenInclude(p => p.Promotion)
                .Include(p => p.Shop)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductItems)
                .ThenInclude(p => p.ProductConfiguration)
                .ThenInclude(p => p.VariationOption)
                .ThenInclude(v => v.Variation)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
            {
                throw new BaseException.NotFoundException("product_not_found", "Product not found.");
            }

            var promotion = await _unitOfWork.GetRepository<Promotion>().Entities
                .FirstOrDefaultAsync(p => p.Categories.Any(c => c.Id == product.CategoryId) &&
                                          p.StartDate <= DateTime.UtcNow &&
                                          p.EndDate >= DateTime.UtcNow);

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
                    DiscountedPrice = promotion != null ? (int)(pi.Price * (1 - promotion.DiscountRate/100)) : null,
                    Configurations = pi.ProductConfiguration.Select(pc => new ProductConfigurationDetailModel
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
            if (string.IsNullOrEmpty(productId))
            {
                throw new BaseException.BadRequestException("invalid_product_id",
                    "Product ID cannot be null or empty.");
            }

            if (!IsValidGuid(productId))
            {
                throw new BaseException.BadRequestException("invalid_product_id",
                    "Product ID is not a valid GUID.");
            }

            var product = await _unitOfWork.GetRepository<Product>().Entities
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
            {
                throw new BaseException.NotFoundException("product_not_found", "Product not found.");
            }

            if (product.Reviews == null || !product.Reviews.Any())
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

        private string NormalizeString(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            return input.ToLower().Trim().Replace("-", "");
        }

    }
}

