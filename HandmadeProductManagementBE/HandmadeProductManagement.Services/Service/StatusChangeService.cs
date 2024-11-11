using AutoMapper;
using FluentValidation;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.StatusChangeModelViews;
using HandmadeProductManagement.Repositories.Entity;
using Microsoft.EntityFrameworkCore;

namespace HandmadeProductManagement.Services.Service
{
    public class StatusChangeService : IStatusChangeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<StatusChangeForCreationDto> _creationValidator;
        private readonly IValidator<StatusChangeForUpdateDto> _updateValidator;

        public StatusChangeService(IUnitOfWork unitOfWork, IMapper mapper, IValidator<StatusChangeForCreationDto> creationValidator, IValidator<StatusChangeForUpdateDto> updateValidator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _creationValidator = creationValidator;
            _updateValidator = updateValidator;
        }

        // Get status changes by OrderId (only active records)
        public async Task<IList<StatusChangeDto>> GetByPage(int page, int pageSize, bool sortAsc)
        {
            if (page <= 0)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidPageNumber);
            }

            if (pageSize <= 0)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidPageSize);
            }

            IQueryable<StatusChange> query = _unitOfWork.GetRepository<StatusChange>().Entities
                .Where(cr => !cr.DeletedTime.HasValue || cr.DeletedBy == null);

            // Sort by ChangeTime
            query = sortAsc
                ? query.OrderBy(statusChange => statusChange.ChangeTime)
                : query.OrderByDescending(statusChange => statusChange.ChangeTime);

            var statusChanges = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(statusChange => new StatusChangeDto
                {
                    Id = statusChange.Id.ToString(),
                    OrderId = statusChange.OrderId,
                    Status = statusChange.Status,
                    ChangeTime = statusChange.ChangeTime
                })
                .ToListAsync();

            return _mapper.Map<IList<StatusChangeDto>>(statusChanges);
        }

        public async Task<IList<StatusChangeDto>> GetByOrderId(string orderId, bool sortAsc)
        {
            // Validate id format
            if (string.IsNullOrWhiteSpace(orderId))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageEmptyId);
            }

            if (!Guid.TryParse(orderId, out var guidId))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidGuidFormat);
            }

            IQueryable<StatusChange> query = _unitOfWork.GetRepository<StatusChange>().Entities
                .Where(sc => sc.OrderId == orderId && (!sc.DeletedTime.HasValue || sc.DeletedBy == null));

            // Sort by ChangeTime
            query = sortAsc
                ? query.OrderBy(statusChange => statusChange.ChangeTime)
                : query.OrderByDescending(statusChange => statusChange.ChangeTime);

            var statusChanges = await query.ToListAsync();

            return _mapper.Map<IList<StatusChangeDto>>(statusChanges);
        }


        // Create a new status change
        public async Task<bool> Create(StatusChangeForCreationDto createStatusChange, string userId)
        {
            // Validate id format
            if (!Guid.TryParse(createStatusChange.OrderId, out var orderGuidId))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidGuidFormat);
            }

            // Validate
            var validationResult = await _creationValidator.ValidateAsync(createStatusChange);
            if (!validationResult.IsValid)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), validationResult.Errors.Select(e => e.ErrorMessage).FirstOrDefault() ?? string.Empty);
            }

            // Check if OrderId exists
            var order = await _unitOfWork.GetRepository<Order>().Entities
                .FirstOrDefaultAsync(o => o.Id == createStatusChange.OrderId && (!o.DeletedTime.HasValue || o.DeletedBy == null))
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageOrderNotFound);

            var statusChangeEntity = _mapper.Map<StatusChange>(createStatusChange);
            statusChangeEntity.ChangeTime = DateTime.UtcNow.AddHours(7);

            // Set metadata
            statusChangeEntity.CreatedBy = userId;
            statusChangeEntity.LastUpdatedBy = userId;

            await _unitOfWork.GetRepository<StatusChange>().InsertAsync(statusChangeEntity);
            await _unitOfWork.SaveAsync();

            return true;
        }
    }
}
