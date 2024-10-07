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
            // Validate id format
            if (!Guid.TryParse(id, out var guidId))
            {
                throw new BaseException.BadRequestException("invalid_input", "ID is not in a valid GUID format.");
            }

            var variationRepository = _unitOfWork.GetRepository<Variation>();

            var variations = await variationRepository.Entities
                .Where(v => v.CategoryId == id && (!v.DeletedTime.HasValue || v.DeletedBy == null))
                .ToListAsync();

            if (variations.Count == 0)
            {
                throw new BaseException.NotFoundException("not_found", "No variations found for the specified category.");
            }

            return _mapper.Map<IList<VariationDto>>(variations);
        }



        // Get variations by page (only active records)
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
                .Where(v => !v.DeletedTime.HasValue || v.DeletedBy == null);

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

            if (variations == null || !variations.Any())
            {
                throw new BaseException.NotFoundException("not_found", "No variations found for the specified page.");
            }

            return _mapper.Map<IList<VariationDto>>(variations);
        }

        public async Task<bool> Create(VariationForCreationDto variationForCreation)
        {
            // Validate id format
            if (!Guid.TryParse(variationForCreation.CategoryId, out var guidId))
            {
                throw new BaseException.BadRequestException("invalid_input", "ID is not in a valid GUID format.");
            }

            var validationResult = await _creationValidator.ValidateAsync(variationForCreation);
            if (!validationResult.IsValid)
            {
                throw new BaseException.BadRequestException("validation_failed", validationResult.Errors.Select(e => e.ErrorMessage).FirstOrDefault());
            }

            // Check if the CategoryId exists in the Categories table
            var categoryExists = await _unitOfWork.GetRepository<Category>()
                .Entities.AnyAsync(c => c.Id == variationForCreation.CategoryId);

            if (!categoryExists)
            {
                throw new BaseException.NotFoundException("category_not_found", "Category does not exist.");
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
            // Validate id format
            if (!Guid.TryParse(id, out var guidId))
            {
                throw new BaseException.BadRequestException("invalid_input", "ID is not in a valid GUID format.");
            }

            var validationResult = await _updateValidator.ValidateAsync(variationForUpdate);
            if (!validationResult.IsValid)
            {
                throw new BaseException.BadRequestException("validation_failed", validationResult.Errors.Select(e => e.ErrorMessage).FirstOrDefault());
            }

            var repository = _unitOfWork.GetRepository<Variation>();
            var variation = await repository.Entities
                .FirstOrDefaultAsync(v => v.Id == id && (!v.DeletedTime.HasValue || v.DeletedBy == null));

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
            // Validate id format
            if (!Guid.TryParse(id, out var guidId))
            {
                throw new BaseException.BadRequestException("invalid_input", "ID is not in a valid GUID format.");
            }

            var repository = _unitOfWork.GetRepository<Variation>();
            var variation = await repository.GetByIdAsync(id);

            if (variation == null || variation.DeletedTime.HasValue || variation.DeletedBy != null)
            {
                throw new BaseException.NotFoundException("not_found", "Variation not found");
            }

            variation.DeletedBy = "currentUser";
            variation.DeletedTime = DateTime.UtcNow;
            repository.Update(variation);
            await _unitOfWork.SaveAsync();

            return true;
        }
    }
}
