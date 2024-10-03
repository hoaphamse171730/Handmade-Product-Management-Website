using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.ProductDetailModelViews;
using HandmadeProductManagement.ModelViews.ProductModelViews;
using HandmadeProductManagement.ModelViews.ReviewModelViews;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using HandmadeProductManagement.ModelViews.PromotionModelViews;
using HandmadeProductManagement.Core.Utils;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace HandmadeProductManagement.Services.Service
{
    public class ProductService : IProductService
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<ProductForCreationDto> _creationValidator;
        private readonly IValidator<ProductForUpdateDto> _updateValidator;

        public ProductService(IUnitOfWork unitOfWork, IMapper mapper, IValidator<ProductForCreationDto> creationValidator, IValidator<ProductForUpdateDto> updateValidator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _creationValidator = creationValidator;
            _updateValidator = updateValidator;
        }

        public async Task<IEnumerable<ProductSearchVM>> SearchProductsAsync(ProductSearchFilter searchModel)
        {
            // Validate CategoryId and ShopId datatype (Guid)
            if (!string.IsNullOrWhiteSpace(searchModel.CategoryId) && !IsValidGuid(searchModel.CategoryId))
            {
                throw new BaseException.BadRequestException("bad_request","Invalid Category Id");
            }

            if (!string.IsNullOrWhiteSpace(searchModel.ShopId) && !IsValidGuid(searchModel.ShopId))
            {
                throw new BaseException.BadRequestException("bad_request","Invalid Shop ID");
            }

            // Validate MinRating limit (from 0 to 5)
            if (searchModel.MinRating.HasValue && (searchModel.MinRating < 0 || searchModel.MinRating > 5))
            {
                throw new BaseException.BadRequestException("bad_request", "MinRating must be between 0 and 5.");
            }


            var query = _unitOfWork.GetRepository<Product>().Entities.AsQueryable();

            // Apply Search Filters
            if (!string.IsNullOrEmpty(searchModel.Name))
            {
                query = query.Where(p => p.Name.Contains(searchModel.Name));
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
                    Price = g.SelectMany(p => p.ProductItems).Any() ? g.SelectMany(p => p.ProductItems).Min(pi => pi.Price) : 0
                }).OrderBy(pr => searchModel.SortByPrice
                    ? (searchModel.SortDescending ? (decimal)-pr.Price : (decimal)pr.Price) // Sort by price ascending or descending
                    : (searchModel.SortDescending ? (decimal) -pr.Rating : (decimal)pr.Rating)) // Sort by rating ascending or descending
                .ToListAsync();

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



        public async Task<ProductDto> Create(ProductForCreationDto product)
        {
            var result = _creationValidator.ValidateAsync(product);
            if (!result.Result.IsValid)
                throw new ValidationException(result.Result.Errors);
            var productEntity = _mapper.Map<Product>(product);
            productEntity.CreatedTime = DateTime.UtcNow;
            await _unitOfWork.GetRepository<Product>().InsertAsync(productEntity);
            await _unitOfWork.SaveAsync();
            var productToReturn = _mapper.Map<ProductDto>(productEntity);
            return productToReturn;

        }

        public async Task<ProductDto> Update(string id, ProductForUpdateDto product)
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
            await _unitOfWork.GetRepository<Product>().UpdateAsync(productEntity);
            await _unitOfWork.SaveAsync();
            var productToReturn = _mapper.Map<ProductDto>(productEntity);
            return productToReturn;
        }

        public async Task<bool> Delete(string id)
        {
            var productRepo = _unitOfWork.GetRepository<Product>();
            var productEntity = await productRepo.Entities.FirstOrDefaultAsync(x => x.Id == id);
            if (productEntity == null)
                throw new KeyNotFoundException("Product not found");
            productEntity.DeletedTime = DateTime.UtcNow;
            await productRepo.DeleteAsync(id);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> SoftDelete(string id)
        {
            var productRepo = _unitOfWork.GetRepository<Product>();
            var productEntity = await productRepo.Entities.FirstOrDefaultAsync(x => x.Id == id.ToString());
            if (productEntity == null)
                throw new KeyNotFoundException("Product not found");
            productEntity.DeletedTime = DateTime.UtcNow;
            await productRepo.UpdateAsync(productEntity);
            await _unitOfWork.SaveAsync();
            return true;
        }

        private bool IsValidGuid(string input)
        {
            return Guid.TryParse(input, out _);
        }

        public async Task<ProductDetailResponseModel> GetProductDetailsByIdAsync(string productId)
        {
            if (string.IsNullOrEmpty(productId) || !IsValidGuid(productId))
            {
                throw new BaseException.BadRequestException("invalid_product_id", "Product ID is invalid or empty.");
            }

            var product = await _unitOfWork.GetRepository<Product>().Entities
                .Include(p => p.Category)
                .Include(p => p.Shop)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductItems)
                    .ThenInclude(pi => pi.ProductConfiguration)
                        .ThenInclude(pc => pc.VariationOption)
                            .ThenInclude(vo => vo.Variation)
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
                    DiscountedPrice = promotion != null ? (int)(pi.Price * (1 - promotion.DiscountRate)) : (int?)null,
                    Configurations = pi.ProductConfiguration.Select(pc => new ProductConfigurationDetailModel
                    {
                        VariationName = pc.VariationOption.Variation.Name,
                        OptionName = pc.VariationOption.Value
                    }).ToList()
                }).ToList(),
                Promotion = promotion != null ? new PromotionDetailModel
                {
                    Id = promotion.Id,
                    Name = promotion.Name,
                    Description = promotion.Description,
                    DiscountRate = promotion.DiscountRate,
                    StartDate = promotion.StartDate,
                    EndDate = promotion.EndDate,
                    Status = promotion.Status
                } : null
            };

            return response;
        }

        public async Task<decimal> CalculateAverageRatingAsync(string productId)
        {
            if (string.IsNullOrEmpty(productId))
            {
                throw new BaseException.BadRequestException("invalid_product_id", "Product ID cannot be null or empty.");
            }

            if (!IsValidGuid(productId))
            {
                throw new BaseException.BadRequestException("invalid_product_id", "Product ID is not a valid GUID.");
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

            decimal averageRating = Math.Round((decimal) product.Reviews.Average(r => r.Rating),1);

            // Update the product's rating
            product.Rating = averageRating;
            await _unitOfWork.GetRepository<Product>().UpdateAsync(product);
            await _unitOfWork.SaveAsync();

            return averageRating;
        }
    }
}
