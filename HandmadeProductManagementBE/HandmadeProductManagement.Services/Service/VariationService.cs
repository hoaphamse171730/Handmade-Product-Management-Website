﻿using AutoMapper;
using FluentValidation;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.ModelViews.VariationModelViews;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Core.Base;
using Microsoft.EntityFrameworkCore;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Common;

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

        public async Task<IList<VariationDto>> GetByCategoryId(string id)
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
                .Where(v => v.CategoryId == id && (!v.DeletedTime.HasValue || v.DeletedBy == null))
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
