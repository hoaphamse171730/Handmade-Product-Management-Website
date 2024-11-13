using AutoMapper;
using FluentValidation;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Utils;
using HandmadeProductManagement.ModelViews.OrderModelViews;
using HandmadeProductManagement.ModelViews.ProductItemModelViews;
using HandmadeProductManagement.ModelViews.VariationCombinationModelViews;
using HandmadeProductManagement.ModelViews.VariationModelViews;
using HandmadeProductManagement.ModelViews.VariationOptionModelViews;
using Microsoft.EntityFrameworkCore;

namespace HandmadeProductManagement.Services.Service
{
    public class ProductItemService : IProductItemService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<ProductItemForCreationDto> _creationValidator;
        private readonly IValidator<ProductItemForUpdateDto> _updateValidator;
        private readonly IProductService _productService;

        public ProductItemService(IUnitOfWork unitOfWork, IMapper mapper, IValidator<ProductItemForCreationDto> creationValidator, IValidator<ProductItemForUpdateDto> updateValidator, IProductService productService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _creationValidator = creationValidator;
            _updateValidator = updateValidator;
            _productService = productService;
        }

        public async Task<bool> AddVariationOptionsToProduct(string productId, List<VariationCombinationDto> variationCombinations, string userId)
        {
            // Step 1: Get the product entity
            var product = await _unitOfWork.GetRepository<Product>().Entities
                .FirstOrDefaultAsync(p => p.Id.ToString() == productId && (!p.DeletedTime.HasValue || p.DeletedBy == null))
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageProductNotFound);

            if (product.CreatedBy != userId)
            {
                throw new BaseException.ForbiddenException(StatusCodeHelper.Forbidden.ToString(), Constants.ErrorMessageForbidden);
            }

            // Step 2: Get all existing variation options for this product
            var existingVariations = await GetAllVariationsWithOptionsForProduct(productId);

            // Step 3: Collect all variation option IDs from the provided combinations
            var allVariationOptionIds = variationCombinations.SelectMany(vc => vc.VariationOptionIds).Distinct().ToList();

            // Step 4: Validate if all provided variation option IDs exist in the database
            var variationOptionExists = await _unitOfWork.GetRepository<VariationOption>().Entities
                .Where(vo => allVariationOptionIds.Contains(vo.Id))
                .Select(vo => vo.Id)
                .ToListAsync();

            if (variationOptionExists.Count != allVariationOptionIds.Count)
            {
                throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageVariationOptionNotFound);
            }

            // Step 5: Check if each provided variation option already exists in the product
            var existingProductConfigurations = await _unitOfWork.GetRepository<ProductConfiguration>().Entities
                .Where(pc => pc.ProductItem != null && pc.ProductItem.ProductId.ToString() == productId && allVariationOptionIds.Contains(pc.VariationOptionId))
                .Select(pc => pc.VariationOptionId)
                .ToListAsync();

            // Step 6: Filter out any variation options that are already associated with the product
            var newVariationCombinations = variationCombinations
                .Where(vc => !vc.VariationOptionIds.All(id => existingProductConfigurations.Contains(id)))
                .ToList();

            if (!newVariationCombinations.Any())
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageDuplicateCombination);
            }

            // Step 7: Validate the new combinations and ensure they are valid
            foreach (var combination in newVariationCombinations)
            {
                var variationOptionIds = combination.VariationOptionIds.Distinct().ToList();

                // Step 7.1: Ensure that no two variation options in the combination have the same variation name
                var variationNames = await _unitOfWork.GetRepository<VariationOption>().Entities
                    .Where(vo => variationOptionIds.Contains(vo.Id))
                    .Select(vo => vo.Variation!.Name.ToLower()) // Convert to lowercase for case-insensitive comparison
                    .Distinct()
                    .ToListAsync();

                if (variationOptionIds.Count != variationNames.Count)
                {
                    throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidCombination);
                }

                // Step 7.2: Ensure that new options complete all other existing options by unique variation names
                foreach (var existingVariation in existingVariations)
                {
                    // Convert the existing variation name to lowercase for case-insensitive comparison
                    var existingVariationName = existingVariation.Name.ToLower();

                    // Skip if the existing variation is already covered by the combination
                    if (variationNames.Contains(existingVariationName)) continue;

                    var missingVariationOptions = existingVariation.Options?.Select(opt => opt.Id).ToList();
                    if (!variationOptionIds.Any(id => missingVariationOptions!.Contains(id)))
                    {
                        throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageIncompleteCombinations);
                    }
                }

                // Step 8: Check if the combination already exists in the product configurations
                var existingProductItem = await _unitOfWork.GetRepository<ProductItem>().Entities
                    .Where(pi => pi.ProductId == product.Id)
                    .Include(pi => pi.ProductConfigurations)
                    .Where(pi => pi.ProductConfigurations.All(pc => variationOptionIds.Contains(pc.VariationOptionId)) &&
                                 pi.ProductConfigurations.Count == variationOptionIds.Count)
                    .FirstOrDefaultAsync();

                if (existingProductItem != null)
                {
                    throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageDuplicateCombination);
                }
            }

            // Step 9: Remove old ProductItems that no longer fit the new combinations
            var productItemsToRemove = await _unitOfWork.GetRepository<ProductItem>().Entities
                .Where(pi => pi.ProductId == product.Id) // Lọc theo ProductId
                .ToListAsync(); // Chuyển tất cả các ProductItem sang client-side

            var productItemsToDelete = productItemsToRemove
                .Where(pi => !newVariationCombinations.Any(vc =>
                    // Kiểm tra nếu tất cả VariationOptionIds trong sự kết hợp mới có thể được tìm thấy trong ProductConfigurations của ProductItem
                    vc.VariationOptionIds.Intersect(
                        pi.ProductConfigurations.Select(pc => pc.VariationOptionId)
                    ).Count() == vc.VariationOptionIds.Count // Đảm bảo rằng tất cả các VariationOptionIds đều có trong ProductConfiguration
                ))
                .ToList();

            // Xóa các ProductItem không hợp lệ
            foreach (var itemToRemove in productItemsToDelete)
            {
                _unitOfWork.GetRepository<ProductItem>().Delete(itemToRemove.Id);
            }


            // Step 10: Call the ProductService to add the variation options for new combinations
            await _productService.AddVariationOptionsToProduct(product, newVariationCombinations, userId);

            return true;
        }

        public async Task<List<VariationWithOptionsDto>> GetAllVariationsWithOptionsForProduct(string productId)
        {
            var productConfigurations = await _unitOfWork.GetRepository<ProductConfiguration>().Entities
                .Include(pc => pc.VariationOption)
                .ThenInclude(vo => vo!.Variation)
                .Where(pc => pc.ProductItem != null && pc.ProductItem.ProductId.ToString() == productId)

                .ToListAsync();

            // Group by VariationId to collect all VariationOptions for each Variation
            var variationsWithOptions = productConfigurations
                .Where(pc => pc.VariationOption != null && pc.VariationOption.Variation != null)
                .GroupBy(pc => pc.VariationOption!.VariationId)
                .Select(g => new VariationWithOptionsDto
                {
                    Id = g.Key,
                    Name = g.First().VariationOption!.Variation!.Name,
                    Options = g.Select(pc => new OptionsDto
                    {
                        Id = pc.VariationOption!.Id,
                        Name = pc.VariationOption.Value
                    }).ToList()
                })
                .ToList();

            return variationsWithOptions;
        }

        // Get product item by productId
        public async Task<ProductItemDto> GetByProductId(string productId)
        {
            // Validate id format
            if (!Guid.TryParse(productId, out var guidProductId))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidGuidFormat);
            }

            var productItemEntity = await _unitOfWork.GetRepository<ProductItem>().Entities
                .FirstOrDefaultAsync(pi => pi.ProductId == productId && (!pi.DeletedTime.HasValue || pi.DeletedBy == null))
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageProductItemNotFound);

            var productItemDto = _mapper.Map<ProductItemDto>(productItemEntity);
            return productItemDto;
        }

        // Create a new product item
        public async Task<bool> Create(ProductItemForCreationDto productItemDto, string userId)
        {
            // Validate
            var validationResult = await _creationValidator.ValidateAsync(productItemDto);
            if (!validationResult.IsValid)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), validationResult.Errors.Select(e => e.ErrorMessage).FirstOrDefault() ?? string.Empty);
            }

            var productItemEntity = _mapper.Map<ProductItem>(productItemDto);

            productItemEntity.CreatedBy = userId;
            productItemEntity.LastUpdatedBy = userId;

            await _unitOfWork.GetRepository<ProductItem>().InsertAsync(productItemEntity);
            await _unitOfWork.SaveAsync();

            return true;
        }

        // Update an existing product item partially (PATCH)
        public async Task<bool> Update(string id, ProductItemForUpdateDto productItemDto, string userId)
        {
            // Validate id format
            if (!Guid.TryParse(id, out var guidId))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidGuidFormat);
            }

            var validationResult = await _updateValidator.ValidateAsync(productItemDto);
            if (!validationResult.IsValid)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), validationResult.Errors.Select(e => e.ErrorMessage).FirstOrDefault() ?? string.Empty);
            }

            var productItemEntity = await _unitOfWork.GetRepository<ProductItem>().Entities
                .FirstOrDefaultAsync(p => p.Id == id && (!p.DeletedTime.HasValue || p.DeletedBy == null)) ?? throw new BaseException.NotFoundException("not_found", "Product Item Not Found");

            // Get the associated product to check the shop ownership
            var productRepo = _unitOfWork.GetRepository<Product>();
            var productEntity = await productRepo.Entities
                .FirstOrDefaultAsync(p => p.Id == productItemEntity.ProductId)
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageProductNotFound);

            if (productItemEntity == null)
            {
                throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageProductItemNotFound);
            }

            if (productItemEntity.CreatedBy != userId)
            {
                throw new BaseException.ForbiddenException(StatusCodeHelper.Forbidden.ToString(), Constants.ErrorMessageForbidden);
            }

            if (productItemDto.Price.HasValue)
            {
                productItemEntity.Price = productItemDto.Price.Value;
            }

            if (productItemDto.QuantityInStock.HasValue)
            {
                productItemEntity.QuantityInStock = productItemDto.QuantityInStock.Value;
            }

            // Cập nhật thông tin metadata
            productItemEntity.LastUpdatedTime = DateTime.UtcNow;
            productItemEntity.LastUpdatedBy = userId;

            await _unitOfWork.GetRepository<ProductItem>().UpdateAsync(productItemEntity);
            await _unitOfWork.SaveAsync();

            return true;
        }

        // Soft delete a product item
        public async Task<bool> Delete(string id, string userId)
        {
            // Validate id format
            if (!Guid.TryParse(id, out var guidId))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidGuidFormat);
            }

            var productItemRepo = _unitOfWork.GetRepository<ProductItem>();
            var productItemEntity = await productItemRepo.Entities.FirstOrDefaultAsync(p => p.Id == id);

            if (productItemEntity == null || productItemEntity.DeletedTime.HasValue || productItemEntity.DeletedBy != null)
            {
                throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageProductItemNotFound);
            }

            var productRepo = _unitOfWork.GetRepository<Product>();
            var productEntity = await productRepo.Entities
                .FirstOrDefaultAsync(p => p.Id == productItemEntity.ProductId)
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageProductNotFound);

            if (productItemEntity.CreatedBy != userId)
            {
                throw new BaseException.ForbiddenException(StatusCodeHelper.Forbidden.ToString(), Constants.ErrorMessageForbidden);
            }

            productItemEntity.DeletedTime = DateTime.UtcNow;
            productItemEntity.DeletedBy = userId;

            await productItemRepo.UpdateAsync(productItemEntity);
            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<bool> Restore(string id, string userId)
        {
            // Validate id format
            if (!Guid.TryParse(id, out var guidId))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidGuidFormat);
            }

            var productItemRepo = _unitOfWork.GetRepository<ProductItem>();
            var productItemEntity = await productItemRepo.Entities.FirstOrDefaultAsync(p => p.Id == id);

            if (productItemEntity == null)
            {
                throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageProductItemNotFound);
            }

            if (!productItemEntity.DeletedTime.HasValue)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageProductItemAlreadyActive);
            }

            var productRepo = _unitOfWork.GetRepository<Product>();
            var productEntity = await productRepo.Entities
                .FirstOrDefaultAsync(p => p.Id == productItemEntity.ProductId)
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageProductNotFound);

            if (productItemEntity.CreatedBy != userId)
            {
                throw new BaseException.ForbiddenException(StatusCodeHelper.Forbidden.ToString(), Constants.ErrorMessageForbidden);
            }

            productItemEntity.DeletedTime = null;
            productItemEntity.DeletedBy = null;
            productItemEntity.LastUpdatedBy = userId;
            productItemEntity.LastUpdatedTime = DateTime.UtcNow;

            await productItemRepo.UpdateAsync(productItemEntity);
            await _unitOfWork.SaveAsync();

            return true;

        }

        public async Task<List<ProductItemDto>> GetAllDeletedAsync()
        {
            var deletedProductItems = await _unitOfWork.GetRepository<ProductItem>().Entities
                .Where(p => p.DeletedTime.HasValue)
                .ToListAsync();

            var productItemDtos = _mapper.Map<List<ProductItemDto>>(deletedProductItems);

            return productItemDtos;
        }
    }
}
