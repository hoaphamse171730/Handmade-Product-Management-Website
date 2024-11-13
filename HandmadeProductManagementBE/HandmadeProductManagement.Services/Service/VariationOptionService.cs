using AutoMapper;
using FluentValidation;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.VariationOptionModelViews;
using Microsoft.EntityFrameworkCore;
using System;

namespace HandmadeProductManagement.Services.Service
{
    public class VariationOptionService : IVariationOptionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<VariationOptionForCreationDto> _creationValidator;
        private readonly IValidator<VariationOptionForUpdateDto> _updateValidator;

        public VariationOptionService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<VariationOptionForCreationDto> creationValidator,
            IValidator<VariationOptionForUpdateDto> updateValidator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _creationValidator = creationValidator;
            _updateValidator = updateValidator;
        }

        public async Task<LatestVariationOptionId> GetLatestVariationOptionId(string variationId, string userId)
        {
            // Validate variationId format
            if (!Guid.TryParse(variationId, out var guid))
            {
                throw new BaseException.BadRequestException(
                    StatusCodeHelper.BadRequest.ToString(),
                    Constants.ErrorMessageInvalidGuidFormat
                );
            }

            // Retrieve the latest variation option for the specified variation and user
            var latestOption = await _unitOfWork.GetRepository<VariationOption>().Entities
                .Where(vo => vo.VariationId == variationId &&
                             vo.CreatedBy == userId &&
                             (!vo.DeletedTime.HasValue || vo.DeletedBy == null))
                .OrderByDescending(vo => vo.CreatedTime)
                .FirstOrDefaultAsync();

            return _mapper.Map<LatestVariationOptionId>(latestOption);
        }

        public async Task<IList<VariationOptionDto>> GetByVariationId(string variationId, string userId)
        {
            // Validate id format
            if (!Guid.TryParse(variationId, out var guidId))
            {
                throw new BaseException.BadRequestException(
                    StatusCodeHelper.BadRequest.ToString(),
                    Constants.ErrorMessageInvalidGuidFormat
                );
            }

            if (string.IsNullOrWhiteSpace(variationId))
            {
                throw new BaseException.BadRequestException(
                    StatusCodeHelper.BadRequest.ToString(),
                    Constants.ErrorMessageEmptyId
                );
            }

            var options = await _unitOfWork.GetRepository<VariationOption>().Entities
                .Where(vo => vo.VariationId == variationId && (!vo.DeletedTime.HasValue || vo.DeletedBy == null) && vo.CreatedBy == userId)
                .ToListAsync();

            return _mapper.Map<IList<VariationOptionDto>>(options);
        }

        public async Task<bool> Create(VariationOptionForCreationDto option, string userId)
        {
            // Validate if the VariationId is a valid GUID
            if (!Guid.TryParse(option.VariationId, out var guidId))
            {
                throw new BaseException.BadRequestException(
                    StatusCodeHelper.BadRequest.ToString(),
                    Constants.ErrorMessageInvalidGuidFormat
                );
            }

            var validationResult = await _creationValidator.ValidateAsync(option);
            if (!validationResult.IsValid)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), validationResult.Errors.Select(e => e.ErrorMessage).FirstOrDefault() ?? string.Empty);
            }

            // Check if the VariationId exists
            var variationExists = await _unitOfWork.GetRepository<Variation>()
                .Entities.AnyAsync(v => v.Id == option.VariationId);
            if (!variationExists)
            {
                throw new BaseException.NotFoundException(
                    StatusCodeHelper.NotFound.ToString(),
                    Constants.ErrorMessageVariationNotFound
                );
            }

            var optionEntity = _mapper.Map<VariationOption>(option);

            optionEntity.CreatedBy = userId;
            optionEntity.LastUpdatedBy = userId;

            await _unitOfWork.GetRepository<VariationOption>().InsertAsync(optionEntity);
            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<bool> Update(string id, VariationOptionForUpdateDto option, string userId)
        {
            // Validate id format
            if (!Guid.TryParse(id, out var guidId))
            {
                throw new BaseException.BadRequestException(
                    StatusCodeHelper.BadRequest.ToString(),
                    Constants.ErrorMessageInvalidGuidFormat
                );
            }

            // Validate the input data
            var validationResult = await _updateValidator.ValidateAsync(option);
            if (!validationResult.IsValid)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), validationResult.Errors.Select(e => e.ErrorMessage).FirstOrDefault() ?? string.Empty);
            }

            var existingOption = await _unitOfWork.GetRepository<VariationOption>().Entities
                .FirstOrDefaultAsync(vo => vo.Id == id && (!vo.DeletedTime.HasValue || vo.DeletedBy == null))
                ?? throw new BaseException.NotFoundException(
                    StatusCodeHelper.NotFound.ToString(),
                    Constants.ErrorMessageVariationOptionNotFound
                );

            if (existingOption.CreatedBy != userId)
            {
                throw new BaseException.ForbiddenException(
                    StatusCodeHelper.Forbidden.ToString(),
                    Constants.ErrorMessageForbidden
                );
            }

            existingOption.LastUpdatedBy = userId;
            existingOption.LastUpdatedTime = DateTime.UtcNow;

            _mapper.Map(option, existingOption);
            await _unitOfWork.GetRepository<VariationOption>().UpdateAsync(existingOption);
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

            var option = await _unitOfWork.GetRepository<VariationOption>().Entities
                .FirstOrDefaultAsync(vo => vo.Id == id);

            if (option == null || option.DeletedTime.HasValue || option.DeletedBy != null)
            {
                throw new BaseException.NotFoundException(
                    StatusCodeHelper.NotFound.ToString(),
                    Constants.ErrorMessageVariationOptionNotFound
                );
            }

            if (option.CreatedBy != userId)
            {
                throw new BaseException.ForbiddenException(
                    StatusCodeHelper.Forbidden.ToString(),
                    Constants.ErrorMessageForbidden
                );
            }

            // Perform the soft delete
            option.DeletedTime = DateTime.UtcNow;
            option.DeletedBy = userId;

            await _unitOfWork.GetRepository<VariationOption>().UpdateAsync(option);
            await _unitOfWork.SaveAsync();

            return true;
        }

    }
}
