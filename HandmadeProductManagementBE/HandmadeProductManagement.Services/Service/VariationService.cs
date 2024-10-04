using AutoMapper;
using FluentValidation;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.ModelViews.VariationModelViews;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Core.Base;
using Microsoft.EntityFrameworkCore;

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

        public async Task<IList<VariationDto>> GetByCategoryId(string id)
        {
            var variationRepository = _unitOfWork.GetRepository<Variation>();
            var variations = await variationRepository.Entities
                .Where(v => v.CategoryId == id && !v.DeletedTime.HasValue)
                .ToListAsync();

            return _mapper.Map<IList<VariationDto>>(variations);
        }

        // Get cancel reasons by page (only active records)
        public async Task<IList<VariationDto>> GetByPage(int page, int pageSize)
        {
            if (page <= 0)
            {
                throw new BaseException.BadRequestException("invalid_input", "Page must be greater than 0.");
            }

            if (pageSize <= 0)
            {
                throw new BaseException.BadRequestException("invalid_input", "Page size must be greater than 0.");
            }

            IQueryable<Variation> query = _unitOfWork.GetRepository<Variation>().Entities
                .Where(cr => !cr.DeletedTime.HasValue || cr.DeletedBy == null);

            var variations = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(variation => new VariationDto
                {
                    Id = variation.Id.ToString(),
                    Name = variation.Name,
                    CategoryId = variation.CategoryId,
                })
                .ToListAsync();

            var variationDto = _mapper.Map<IList<VariationDto>>(variations);
            return variationDto;
        }

        public async Task<bool> Create(VariationForCreationDto variationForCreation)
        {
            var validationResult = await _creationValidator.ValidateAsync(variationForCreation);
            if (!validationResult.IsValid)
            {
                throw new BaseException.BadRequestException("validation_failed", validationResult.Errors.Select(e => e.ErrorMessage).FirstOrDefault());
            }

            var variationEntity = _mapper.Map<Variation>(variationForCreation);

            variationEntity.CreatedBy = "currentUser";
            variationEntity.LastUpdatedBy = "currentUser";

            await _unitOfWork.GetRepository<Variation>().InsertAsync(variationEntity);
            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<bool> Update(string id, VariationForUpdateDto variationForUpdate)
        {
            var validationResult = await _updateValidator.ValidateAsync(variationForUpdate);
            if (!validationResult.IsValid)
            {
                throw new BaseException.BadRequestException("validation_failed", validationResult.Errors.Select(e => e.ErrorMessage).FirstOrDefault());
            }

            var repository = _unitOfWork.GetRepository<Variation>();
            var variation = await repository.GetByIdAsync(id);

            if (variation == null)
            {
                throw new BaseException.NotFoundException("variation_not_found", "Variation not found.");
            }

            _mapper.Map(variationForUpdate, variation);
            repository.Update(variation);
            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<bool> Delete(string id)
        {
            var repository = _unitOfWork.GetRepository<Variation>();
            var variation = await repository.GetByIdAsync(id);

            if (variation == null)
            {
                throw new BaseException.NotFoundException("variation_not_found", "Variation not found.");
            }

            variation.DeletedTime = DateTime.UtcNow;
            repository.Update(variation);
            await _unitOfWork.SaveAsync();

            return true;
        }
    }
}
