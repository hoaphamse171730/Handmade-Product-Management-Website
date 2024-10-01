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
        public async Task<IList<CancelReasonResponseModel>> GetAll()
        {
            IQueryable<CancelReason> query = _unitOfWork.GetRepository<CancelReason>().Entities
                .Where(cr => !cr.DeletedTime.HasValue || cr.DeletedBy == null);

            var result = await query.Select(cancelReason => new CancelReasonResponseModel
            {
                Id = cancelReason.Id.ToString(),
                Description = cancelReason.Description, 
                RefundRate = cancelReason.RefundRate,
            }).ToListAsync();
            return result;
        }

        // Get cancel reasons by page (only active records)
        public async Task<IList<CancelReasonResponseModel>> GetByPage(int page, int pageSize)
        {
            if (page <= 0)
                throw new BaseException.BadRequestException("invalid_input", "Page must be greater than 0.");

            if (pageSize <= 0)
                throw new BaseException.BadRequestException("invalid_input", "Page size must be greater than 0.");

            IQueryable<CancelReason> query = _unitOfWork.GetRepository<CancelReason>().Entities
                .Where(cr => !cr.DeletedTime.HasValue || cr.DeletedBy == null);

            var result = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(cancelReason => new CancelReasonResponseModel
                {
                    Id = cancelReason.Id.ToString(),
                    Description = cancelReason.Description,
                    RefundRate = cancelReason.RefundRate,
                })
                .ToListAsync();

            return result;
        }

        // Create a new cancel reason
        public async Task<CancelReasonResponseModel> Create(CancelReasonForCreationDto cancelReason)
        {
            // Validate
            var result = _creationValidator.ValidateAsync(cancelReason);
            if (!result.Result.IsValid)
            {
                throw new ValidationException(result.Result.Errors);
            }

            var cancelReasonEntity = _mapper.Map<CancelReason>(cancelReason);

            // Set metadata
            cancelReason.CreatedBy = "currentUser"; // Update with actual user info
            cancelReason.LastUpdatedBy = "currentUser"; // Update with actual user info

            await _unitOfWork.GetRepository<CancelReason>().InsertAsync(cancelReasonEntity);
            await _unitOfWork.SaveAsync();

            var cancelReasonToReturn = _mapper.Map<CancelReasonResponseModel>(cancelReasonEntity);

            return cancelReasonToReturn;
        }

        // Update an existing cancel reason
        public async Task<CancelReasonResponseModel> Update(string id, CancelReasonForCreationDto updatedCancelReason)
        {
            var existingCancelReason = await _unitOfWork.GetRepository<CancelReason>().GetByIdAsync(id);

            if (existingCancelReason == null)
                throw new KeyNotFoundException("Cancel Reason not found");

            // Validate RefundRate is between 0 and 1
            if (updatedCancelReason.RefundRate < 0 || updatedCancelReason.RefundRate > 1)
            {
                throw new BaseException.BadRequestException("out_of_ranged_input", "RefundRate must be between 0 and 1.");
            }

            // Validate Description is not null or empty
            if (string.IsNullOrWhiteSpace(updatedCancelReason.Description))
            {
                throw new BaseException.BadRequestException("missing_required_field", "Description cannot be null or empty.");
            }

            // Update fields
            existingCancelReason.Description = updatedCancelReason.Description;
            existingCancelReason.RefundRate = updatedCancelReason.RefundRate;
            existingCancelReason.LastUpdatedBy = "currentUser"; // Update with actual user info
            existingCancelReason.LastUpdatedTime = DateTimeOffset.UtcNow;

            await _unitOfWork.GetRepository<CancelReason>().UpdateAsync(existingCancelReason);
            await _unitOfWork.SaveAsync();

            return new CancelReasonResponseModel 
            { 
                Id = existingCancelReason.Id.ToString(),
                Description = existingCancelReason.Description,
                RefundRate = existingCancelReason.RefundRate
            };
        }

        // Soft delete 
        public async Task<bool> Delete(string id)
        {
            var cancelReason = await _unitOfWork.GetRepository<CancelReason>().GetByIdAsync(id);
            if (cancelReason == null)
            {

            }

            // Set DeletedTime and DeletedBy
            cancelReason.DeletedTime = DateTimeOffset.UtcNow;
            cancelReason.DeletedBy = "currentUser"; // Update with actual user info

            await _unitOfWork.GetRepository<CancelReason>().UpdateAsync(cancelReason);
            await _unitOfWork.SaveAsync();
            return true;
        }
    }
}
