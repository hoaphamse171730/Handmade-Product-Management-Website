using AutoMapper;
using FluentValidation;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.CancelReasonModelViews;
using Microsoft.EntityFrameworkCore;

namespace HandmadeProductManagement.Services.Service
{
    public class CancelReasonService : ICancelReasonService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<CancelReasonForCreationDto> _creationValidator;
        private readonly IValidator<CancelReasonForUpdateDto> _updateValidator;

        public CancelReasonService(IUnitOfWork unitOfWork, IMapper mapper, IValidator<CancelReasonForCreationDto> creationValidator, IValidator<CancelReasonForUpdateDto> updateValidator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _creationValidator = creationValidator;
            _updateValidator = updateValidator;
        }

        // Get all cancel reasons (only active records)
        public async Task<IList<CancelReasonDto>> GetAll()
        {
            IQueryable<CancelReason> query = _unitOfWork.GetRepository<CancelReason>().Entities
                .Where(cr => !cr.DeletedTime.HasValue ||( cr.DeletedBy == null || !cr.DeletedTime.HasValue))
                .OrderByDescending(cr => cr.CreatedTime); // Sort by CreatedBy in descending order

            var cancelReasons = await query
                .Select(cancelReason => new CancelReasonDto
                {
                    Id = cancelReason.Id.ToString(),
                    Description = cancelReason.Description,
                    RefundRate = cancelReason.RefundRate,
                })
                .ToListAsync();

            return cancelReasons;
        }

        // Create a new cancel reason
        public async Task<bool> Create(CancelReasonForCreationDto cancelReason, string userId)
        {
            // Validate
            var validationResult = await _creationValidator.ValidateAsync(cancelReason);
            if (!validationResult.IsValid)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), validationResult.Errors.First().ErrorMessage);
            }

            var cancelReasonEntity = _mapper.Map<CancelReason>(cancelReason);

            // Set metadata
            cancelReasonEntity.CreatedBy = userId;
            cancelReasonEntity.LastUpdatedBy = userId;

            await _unitOfWork.GetRepository<CancelReason>().InsertAsync(cancelReasonEntity);
            await _unitOfWork.SaveAsync();

            return true;
        }

        // Update an existing cancel reason partially (PATCH)
        public async Task<bool> Update(string id, CancelReasonForUpdateDto cancelReason, string userId)
        {
            // Validate id format
            if (!Guid.TryParse(id, out var guidId))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidGuidFormat);
            }

            var validationResult = await _updateValidator.ValidateAsync(cancelReason);
            if (!validationResult.IsValid)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), validationResult.Errors.First().ErrorMessage);
            }

            // Find the entity to be updated
            var cancelReasonEntity = await _unitOfWork.GetRepository<CancelReason>().Entities
                .FirstOrDefaultAsync(p => p.Id == id && (!p.DeletedTime.HasValue || p.DeletedBy == null))
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageCancelReasonNotFound);

            // Map only updated properties, keeping old values for null or unchanged fields
            if (!string.IsNullOrWhiteSpace(cancelReason.Description))
            {
                cancelReasonEntity.Description = cancelReason.Description;
            }

            if (cancelReason.RefundRate.HasValue)
            {
                cancelReasonEntity.RefundRate = cancelReason.RefundRate.Value;
            }

            // Update metadata (last updated time, updated by user)
            cancelReasonEntity.LastUpdatedTime = DateTime.UtcNow;
            cancelReasonEntity.LastUpdatedBy = userId;

            // Update the entity in the repository
            await _unitOfWork.GetRepository<CancelReason>().UpdateAsync(cancelReasonEntity);
            await _unitOfWork.SaveAsync();

            return true;
        }

        // Soft delete 
        public async Task<bool> Delete(string id, string userId)
        {
            // Validate id format
            if (!Guid.TryParse(id, out var guidId))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidGuidFormat);
            }

            var cancelReasonRepo = _unitOfWork.GetRepository<CancelReason>();
            var cancelReasonEntity = await cancelReasonRepo.Entities.FirstOrDefaultAsync(x => x.Id == id);
            if (cancelReasonEntity == null || cancelReasonEntity.DeletedTime.HasValue || cancelReasonEntity.DeletedBy != null)
            {
                throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageCancelReasonNotFound);
            }

            // Mark the entity as deleted
            cancelReasonEntity.DeletedTime = DateTime.UtcNow;
            cancelReasonEntity.DeletedBy = userId;

            await cancelReasonRepo.UpdateAsync(cancelReasonEntity);
            await _unitOfWork.SaveAsync();
            return true;
        }

        // Get all soft-deleted cancel reasons
        public async Task<IList<CancelReasonDeletedDto>> GetDeletedCancelReasons()
        {
            var query = _unitOfWork.GetRepository<CancelReason>().Entities
                .Where(cr => cr.DeletedTime.HasValue && cr.DeletedBy != null);

            var cancelReasons = await query
                .Select(cancelReason => new CancelReasonDeletedDto
                {
                    Id = cancelReason.Id.ToString(),
                    Description = cancelReason.Description,
                    RefundRate = cancelReason.RefundRate,
                    DeletedBy = cancelReason.DeletedBy,
                    DeletedTime = cancelReason.DeletedTime,
                })
                .ToListAsync();
            return cancelReasons;
        }

        // Patch reverse delete (restore a soft-deleted cancel reason)
        public async Task<bool> PatchReverseDelete(string id, string userId)
        {
            // Validate id format
            if (!Guid.TryParse(id, out var guidId))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidGuidFormat);
            }

            var cancelReasonRepo = _unitOfWork.GetRepository<CancelReason>();
            var cancelReasonEntity = await cancelReasonRepo.Entities
                .FirstOrDefaultAsync(cr => cr.Id == id) ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageCancelReasonNotFound);
            
            if (!cancelReasonEntity.DeletedTime.HasValue && cancelReasonEntity.DeletedBy == null)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageCancelReasonDeleted);
            }

            // Restore the soft-deleted entity
            cancelReasonEntity.DeletedTime = null;
            cancelReasonEntity.DeletedBy = null;

            // Update metadata
            cancelReasonEntity.LastUpdatedTime = DateTime.UtcNow;
            cancelReasonEntity.LastUpdatedBy = userId;

            await cancelReasonRepo.UpdateAsync(cancelReasonEntity);
            await _unitOfWork.SaveAsync();

            return true;
        }

        // Get cancel reason by description
        public async Task<CancelReasonDto> GetByDescription(string description)
        {
            var cancelReasonEntity = await _unitOfWork.GetRepository<CancelReason>().Entities
                .FirstOrDefaultAsync(cr => cr.Description == description && (!cr.DeletedTime.HasValue || cr.DeletedBy == null))
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageCancelReasonNotFound);

            var cancelReasonDto = new CancelReasonDto
            {
                Id = cancelReasonEntity.Id.ToString(),
                Description = cancelReasonEntity.Description,
                RefundRate = cancelReasonEntity.RefundRate
            };

            return cancelReasonDto;
        }

    }
}
