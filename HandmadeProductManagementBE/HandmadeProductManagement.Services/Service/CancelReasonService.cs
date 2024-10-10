using AutoMapper;
using FluentValidation;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
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
                .Where(cr => !cr.DeletedTime.HasValue || cr.DeletedBy == null);

            var cancelReasons = await query
                .Select(cancelReason => new CancelReasonDto
                {
                    Id = cancelReason.Id.ToString(),
                    Description = cancelReason.Description,
                    RefundRate = cancelReason.RefundRate,
                })
                .ToListAsync();

            if (!cancelReasons.Any())
            {
                throw new BaseException.NotFoundException("not_found", "There is no cancel reasons.");
            }

            var cancelReasonDto = _mapper.Map<IList<CancelReasonDto>>(cancelReasons);
            return cancelReasonDto;
        }

        // Create a new cancel reason
        public async Task<bool> Create(CancelReasonForCreationDto cancelReason, string userId)
        {
            // Validate
            var validationResult = await _creationValidator.ValidateAsync(cancelReason);
            if (!validationResult.IsValid)
            {
                throw new BaseException.BadRequestException("validation_failed", validationResult.Errors.Select(e => e.ErrorMessage).FirstOrDefault());
            }

            var cancelReasonEntity = _mapper.Map<CancelReason>(cancelReason);

            // Set metadata
            cancelReasonEntity.CreatedBy = userId;
            cancelReasonEntity.LastUpdatedBy = userId;

            await _unitOfWork.GetRepository<CancelReason>().InsertAsync(cancelReasonEntity);
            await _unitOfWork.SaveAsync();

            return true;
        }

        // Update an existing cancel reason
        public async Task<bool> Update(string id, CancelReasonForUpdateDto cancelReason, string userId)
        {
            // Validate id format
            if (!Guid.TryParse(id, out var guidId))
            {
                throw new BaseException.BadRequestException("invalid_input", "ID is not in a valid GUID format.");
            }

            var validationResult = await _updateValidator.ValidateAsync(cancelReason);
            if (!validationResult.IsValid)
            {
                throw new BaseException.BadRequestException("validation_failed", validationResult.Errors.Select(e => e.ErrorMessage).FirstOrDefault());
            }

            var cancelReasonEntity = await _unitOfWork.GetRepository<CancelReason>().Entities
                .FirstOrDefaultAsync(p => p.Id == id && (!p.DeletedTime.HasValue || p.DeletedBy == null));
            if (cancelReasonEntity == null)
            {
                throw new BaseException.NotFoundException("not_found", "Cancel Reason not found");
            }
            _mapper.Map(cancelReason, cancelReasonEntity);

            cancelReasonEntity.LastUpdatedTime = DateTime.UtcNow;
            cancelReasonEntity.LastUpdatedBy = userId;

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
                throw new BaseException.BadRequestException("invalid_input", "ID is not in a valid GUID format.");
            }

            var cancelReasonRepo = _unitOfWork.GetRepository<CancelReason>();
            var cancelReasonEntity = await cancelReasonRepo.Entities.FirstOrDefaultAsync(x => x.Id == id);
            if (cancelReasonEntity == null || cancelReasonEntity.DeletedTime.HasValue || cancelReasonEntity.DeletedBy != null)
            {
                throw new BaseException.NotFoundException("not_found", "Cancel Reason not found");
            }
            cancelReasonEntity.DeletedTime = DateTime.UtcNow;
            cancelReasonEntity.DeletedBy = userId;

            await cancelReasonRepo.UpdateAsync(cancelReasonEntity);
            await _unitOfWork.SaveAsync();
            return true;
        }

        // Get all soft-deleted cancel reasons
        public async Task<IList<CancelReason>> GetDeletedCancelReasons()
        {
            var query = _unitOfWork.GetRepository<CancelReason>().Entities
                .Where(cr => cr.DeletedTime.HasValue && cr.DeletedBy != null);

            var cancelReasons = await query
                .Select(cancelReason => new CancelReason
                {
                    Id = cancelReason.Id.ToString(),
                    Description = cancelReason.Description,
                    RefundRate = cancelReason.RefundRate,
                    DeletedBy = cancelReason.DeletedBy,
                    DeletedTime = cancelReason.DeletedTime,
                })
                .ToListAsync();

            if (!cancelReasons.Any())
            {
                throw new BaseException.NotFoundException("not_found", "There is no cancel reasons.");
            }

            return cancelReasons;
        }

        // Patch reverse delete (restore a soft-deleted cancel reason)
        public async Task<bool> PatchReverseDelete(string id, string userId)
        {
            // Validate id format
            if (!Guid.TryParse(id, out var guidId))
            {
                throw new BaseException.BadRequestException("invalid_input", "ID is not in a valid GUID format.");
            }

            var cancelReasonRepo = _unitOfWork.GetRepository<CancelReason>();
            var cancelReasonEntity = await cancelReasonRepo.Entities
                .FirstOrDefaultAsync(cr => cr.Id == id && cr.DeletedTime.HasValue && cr.DeletedBy != null);

            if (cancelReasonEntity == null)
            {
                throw new BaseException.NotFoundException("not_found", "Cancel Reason not found or not deleted.");
            }

            cancelReasonEntity.DeletedTime = null;
            cancelReasonEntity.DeletedBy = null;

            cancelReasonEntity.LastUpdatedTime = DateTime.UtcNow;
            cancelReasonEntity.LastUpdatedBy = userId;

            await cancelReasonRepo.UpdateAsync(cancelReasonEntity);
            await _unitOfWork.SaveAsync();

            return true;
        }
    }
}
