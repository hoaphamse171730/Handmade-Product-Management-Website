using AutoMapper;
using FluentValidation;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.ModelViews.VariationModelViews;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Core.Base;
using Microsoft.EntityFrameworkCore;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.ModelViews.VariationOptionModelViews;

namespace HandmadeProductManagement.Services.Service
{
    public class VariationService : IVariationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<VariationForCreationDto> _creationValidator;
        private readonly IValidator<VariationForUpdateDto> _updateValidator;

        public VariationService(IUnitOfWork unitOfWork, IMapper mapper, IValidator<VariationForCreationDto> creationValidator, IValidator<VariationForUpdateDto> updateValidator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _creationValidator = creationValidator;
            _updateValidator = updateValidator;
        }

        public async Task<IList<VariationWithOptionsDto>> GetAllVariationsAndOptionsForProductItems(string categoryId, string userId)
        {
            // Validate categoryId format
            if (!Guid.TryParse(categoryId, out var categoryGuid))
            {
                throw new BaseException.BadRequestException(
                    StatusCodeHelper.BadRequest.ToString(),
                    Constants.ErrorMessageInvalidGuidFormat
                );
            }

            // Retrieve all product items for the user that belong to the given categoryId
            var productItems = await _unitOfWork.GetRepository<ProductItem>().Entities
                .Where(pi => pi.Product!.CategoryId == categoryId && pi.Product.CreatedBy == userId)
                .Include(pi => pi.ProductConfigurations)  // Ensure ProductConfigurations are loaded
                .ThenInclude(pc => pc.VariationOption)  // Ensure VariationOption is loaded in ProductConfiguration
                .ToListAsync();

            // Check if productItems have ProductConfigurations with VariationOptions
            if (!productItems.Any(pi => pi.ProductConfigurations.Any(pc => pc.VariationOption != null)))
            {
                throw new BaseException.NotFoundException(
                    StatusCodeHelper.NotFound.ToString(),
                    "No valid product configurations with variation options found."
                );
            }

            // Get all the variation IDs used in the product items (via ProductConfiguration and VariationOption)
            var variationIdsUsedByUser = productItems
                .SelectMany(pi => pi.ProductConfigurations)  // Get all product configurations
                .Where(pc => pc.VariationOption != null)  // Ensure the VariationOption is not null
                .Select(pc => pc.VariationOption!.VariationId)  // Get the associated VariationId
                .Distinct()  // Ensure uniqueness
                .ToList();

            // Check if we have any variation IDs
            if (!variationIdsUsedByUser.Any())
            {
                throw new BaseException.NotFoundException(
                    StatusCodeHelper.NotFound.ToString(),
                    "No variations found for the provided product items."
                );
            }

            // Retrieve all variations that belong to the given categoryId and are used in the product items of the user
            var variations = await _unitOfWork.GetRepository<Variation>().Entities
                .Where(v => v.CategoryId == categoryId && variationIdsUsedByUser.Contains(v.Id) && (!v.DeletedTime.HasValue || v.DeletedBy == null))
                .ToListAsync();

            // Retrieve all variation options for those variations
            var variationOptionIds = variations.SelectMany(v => v.VariationOptions.Select(vo => vo.Id)).ToList();
            var variationOptions = await _unitOfWork.GetRepository<VariationOption>().Entities
                .Where(vo => variationOptionIds.Contains(vo.Id))
                .ToListAsync();

            // Map the retrieved data to the DTOs
            var result = variations.Select(v => new VariationWithOptionsDto
            {
                Id = v.Id,
                Name = v.Name,
                Options = v.VariationOptions
                    .Where(vo => variationOptions.Any(opt => opt.Id == vo.Id))
                    .Select(vo => new OptionsDto
                    {
                        Id = vo.Id,
                        Value = vo.Value
                    }).ToList()
            }).ToList();

            // Return the list of variations and their options
            return result;
        }


        public async Task<LatestVariationId> GetLatestVariationId(string categoryId, string userId)
        {
            // Validate categoryId format
            if (!Guid.TryParse(categoryId, out _))
            {
                throw new BaseException.BadRequestException(
                    StatusCodeHelper.BadRequest.ToString(),
                    Constants.ErrorMessageInvalidGuidFormat
                );
            }

            // Retrieve the latest variation created by the specified user in the given category
            var latestVariation = await _unitOfWork.GetRepository<Variation>().Entities
                .Where(v => v.CategoryId == categoryId &&
                            v.CreatedBy == userId &&
                            (!v.DeletedTime.HasValue || v.DeletedBy == null))
                .OrderByDescending(v => v.CreatedTime)
                .FirstOrDefaultAsync();

            return _mapper.Map<LatestVariationId>(latestVariation);
        }

        public async Task<IList<VariationDto>> GetByCategoryId(string id, string userId)
        {
            // Validate id format
            if (!Guid.TryParse(id, out var guidId))
            {
                throw new BaseException.BadRequestException(
                    StatusCodeHelper.BadRequest.ToString(),
                    Constants.ErrorMessageInvalidGuidFormat
                );
            }

            var variations = await _unitOfWork.GetRepository<Variation>().Entities
                .Where(v => v.CategoryId == id && (!v.DeletedTime.HasValue || v.DeletedBy == null) && v.CreatedBy == userId)
                .ToListAsync();

            return _mapper.Map<IList<VariationDto>>(variations);
        }

        public async Task<bool> Create(VariationForCreationDto variationForCreation, string userId)
        {
            // Validate id format
            if (!Guid.TryParse(variationForCreation.CategoryId, out var guidId))
            {
                throw new BaseException.BadRequestException(
                    StatusCodeHelper.BadRequest.ToString(),
                    Constants.ErrorMessageInvalidGuidFormat
                );
            }

            var validationResult = await _creationValidator.ValidateAsync(variationForCreation);
            if (!validationResult.IsValid)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), validationResult.Errors.Select(e => e.ErrorMessage).FirstOrDefault() ?? string.Empty);
            }

            // Check if the CategoryId exists in the Categories table
            var categoryExists = await _unitOfWork.GetRepository<Category>()
                .Entities.AnyAsync(c => c.Id == variationForCreation.CategoryId);

            if (!categoryExists)
            {
                throw new BaseException.NotFoundException(
                    StatusCodeHelper.NotFound.ToString(),
                    Constants.ErrorMessageCategoryNotFound
                );
            }

            var variationEntity = _mapper.Map<Variation>(variationForCreation);

            variationEntity.CreatedBy = userId;
            variationEntity.LastUpdatedBy = userId;

            await _unitOfWork.GetRepository<Variation>().InsertAsync(variationEntity);
            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<bool> Update(string id, VariationForUpdateDto variationForUpdate, string userId)
        {
            // Validate id format
            if (!Guid.TryParse(id, out var guidId))
            {
                throw new BaseException.BadRequestException(
                    StatusCodeHelper.BadRequest.ToString(),
                    Constants.ErrorMessageInvalidGuidFormat
                );
            }

            var validationResult = await _updateValidator.ValidateAsync(variationForUpdate);
            if (!validationResult.IsValid)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), validationResult.Errors.Select(e => e.ErrorMessage).FirstOrDefault() ?? string.Empty);
            }

            var repository = _unitOfWork.GetRepository<Variation>();
            var variation = await repository.Entities
                .FirstOrDefaultAsync(v => v.Id == id && (!v.DeletedTime.HasValue || v.DeletedBy == null))
                ?? throw new BaseException.NotFoundException(
                    StatusCodeHelper.NotFound.ToString(),
                    Constants.ErrorMessageVariationNotFound 
                );

            variation.LastUpdatedBy = userId;
            variation.LastUpdatedTime = DateTime.UtcNow;

            _mapper.Map(variationForUpdate, variation);
            repository.Update(variation);
            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<bool> Delete(string id, string userId)
        {
            // Validate id format
            if (!Guid.TryParse(id, out var guidId))
            {
                throw new BaseException.BadRequestException(
                    StatusCodeHelper.BadRequest.ToString(),
                    Constants.ErrorMessageInvalidGuidFormat
                );
            }

            var repository = _unitOfWork.GetRepository<Variation>();
            var variation = await repository.GetByIdAsync(id);

            if (variation == null || variation.DeletedTime.HasValue || variation.DeletedBy != null)
            {
                throw new BaseException.NotFoundException(
                    StatusCodeHelper.NotFound.ToString(),
                    Constants.ErrorMessageVariationNotFound
                );
            }

            variation.DeletedBy = userId;
            variation.DeletedTime = DateTime.UtcNow;

            repository.Update(variation);
            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<IList<Variation>> GetDeleted()
        {
            var deletedVariations = await _unitOfWork.GetRepository<Variation>().Entities
                .Select(variation => new Variation
                {
                    Id = variation.Id.ToString(),
                    Name = variation.Name,
                    CategoryId = variation.CategoryId,
                    DeletedBy = variation.DeletedBy,
                    DeletedTime = variation.DeletedTime,
                })
                .Where(v => v.DeletedTime.HasValue && v.DeletedBy != null)
                .ToListAsync();
            return _mapper.Map<IList<Variation>>(deletedVariations);
        }

        public async Task<bool> Recover(string id, string userId)
        {
            if (!Guid.TryParse(id, out var guidId))
            {
                throw new BaseException.BadRequestException(
                    StatusCodeHelper.BadRequest.ToString(),
                    Constants.ErrorMessageInvalidGuidFormat
                );
            }

            var repository = _unitOfWork.GetRepository<Variation>();
            var variation = await repository.Entities
                .FirstOrDefaultAsync(v => v.Id == id);

            if (variation == null)
            {
                throw new BaseException.NotFoundException(
                    StatusCodeHelper.NotFound.ToString(),
                    Constants.ErrorMessageVariationNotFound
                );
            }

            if (variation.DeletedTime.HasValue && variation.DeletedBy != null)
            {
                throw new BaseException.BadRequestException(
                    StatusCodeHelper.BadRequest.ToString(),
                    Constants.ErrorMessageVariationDeleted
                );
            }

            variation.DeletedBy = null;
            variation.DeletedTime = null;
            variation.LastUpdatedBy = userId;
            variation.LastUpdatedTime = DateTime.UtcNow;

            repository.Update(variation);
            await _unitOfWork.SaveAsync();

            return true;
        }
    }
}
