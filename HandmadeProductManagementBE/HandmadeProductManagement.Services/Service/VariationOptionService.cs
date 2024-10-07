using AutoMapper;
using FluentValidation;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.ModelViews.VariationOptionModelViews;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

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

        public async Task<IList<VariationOptionDto>> GetByPage(int page, int pageSize)
        {
            if (page <= 0 || pageSize <= 0)
            {
                throw new BaseException.BadRequestException("invalid_input", "Page and PageSize must be greater than 0.");
            }

            IQueryable<VariationOption> query = _unitOfWork.GetRepository<VariationOption>().Entities
                .Where(cr => !cr.DeletedTime.HasValue || cr.DeletedBy == null);

            var variationOptions = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(variationOption => new VariationOptionDto
                {
                    Id = variationOption.Id.ToString(),
                    Value = variationOption.Value,
                    VariationId = variationOption.VariationId,
                })
                .ToListAsync();

            if (variationOptions == null || !variationOptions.Any())
            {
                throw new BaseException.NotFoundException("not_found", "No variation options found for the specified page.");
            }

            return _mapper.Map<IList<VariationOptionDto>>(variationOptions);
        }


        public async Task<IList<VariationOptionDto>> GetByVariationId(string variationId)
        {
            // Validate id format
            if (!Guid.TryParse(variationId, out var guidId))
            {
                throw new BaseException.BadRequestException("invalid_input", "ID is not in a valid GUID format.");
            }

            if (string.IsNullOrWhiteSpace(variationId))
            {
                throw new BaseException.BadRequestException("invalid_input", "Variation ID cannot be null or empty.");
            }

            var options = await _unitOfWork.GetRepository<VariationOption>().Entities
                .Where(vo => vo.VariationId == variationId && (!vo.DeletedTime.HasValue || vo.DeletedBy == null))
                .ToListAsync();

            // Check if the list is empty, throw not found exception
            if (options == null || options.Count == 0)
            {
                throw new BaseException.NotFoundException("not_found", "No variation options found for the specified variation.");
            }

            return _mapper.Map<IList<VariationOptionDto>>(options);
        }


        public async Task<bool> Create(VariationOptionForCreationDto option, string username)
        {
            // Validate id format
            if (!Guid.TryParse(option.VariationId, out var guidId))
            {
                throw new BaseException.BadRequestException("invalid_input", "ID is not in a valid GUID format.");
            }

            var validationResult = await _creationValidator.ValidateAsync(option);
            if (!validationResult.IsValid)
            {
                throw new BaseException.BadRequestException("validation_failed", validationResult.Errors.Select(e => e.ErrorMessage).FirstOrDefault());
            }

            // Check if VariationId exists
            var variationExists = await _unitOfWork.GetRepository<Variation>().Entities
                .AnyAsync(v => v.Id == option.VariationId);
            if (!variationExists)
            {
                throw new BaseException.NotFoundException("not_found", "Variation not found.");
            }

            var optionEntity = _mapper.Map<VariationOption>(option);

            // Set metadata
            optionEntity.CreatedBy = username;
            optionEntity.LastUpdatedBy = username;

            await _unitOfWork.GetRepository<VariationOption>().InsertAsync(optionEntity);
            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<bool> Update(string id, VariationOptionForUpdateDto option, string username)
        {
            // Validate id format
            if (!Guid.TryParse(id, out var guidId))
            {
                throw new BaseException.BadRequestException("invalid_input", "ID is not in a valid GUID format.");
            }

            var validationResult = await _updateValidator.ValidateAsync(option);
            if (!validationResult.IsValid)
            {
                throw new BaseException.BadRequestException("validation_failed", validationResult.Errors.Select(e => e.ErrorMessage).FirstOrDefault());
            }

            var existingOption = await _unitOfWork.GetRepository<VariationOption>().Entities
                .FirstOrDefaultAsync(vo => vo.Id == id && (!vo.DeletedTime.HasValue || vo.DeletedBy == null));

            if (existingOption == null)
            {
                throw new BaseException.NotFoundException("not_found", "Variation Option not found.");
            }

            existingOption.LastUpdatedBy = username;

            _mapper.Map(option, existingOption);
            await _unitOfWork.GetRepository<VariationOption>().UpdateAsync(existingOption);
            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<bool> Delete(string id, string username)
        {
            // Validate id format
            if (!Guid.TryParse(id, out var guidId))
            {
                throw new BaseException.BadRequestException("invalid_input", "ID is not in a valid GUID format.");
            }

            var option = await _unitOfWork.GetRepository<VariationOption>().Entities
                .FirstOrDefaultAsync(vo => vo.Id == id);
            if (option == null || option.DeletedTime.HasValue || option.DeletedBy != null)
            {
                throw new BaseException.NotFoundException("not_found", "Variation Option not found.");
            }

            option.DeletedTime = DateTime.UtcNow;
            option.DeletedBy = username;

            await _unitOfWork.GetRepository<VariationOption>().UpdateAsync(option);
            await _unitOfWork.SaveAsync();

            return true;
        }
    }
}
