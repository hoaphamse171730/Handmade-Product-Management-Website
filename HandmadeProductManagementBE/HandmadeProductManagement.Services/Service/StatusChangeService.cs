using AutoMapper;
using FluentValidation;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.ModelViews.StatusChangeModelViews;
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

        // Get status changes by page (only active records)
        public async Task<IList<StatusChangeDto>> GetByPage(int page, int pageSize)
        {
            if (page <= 0)
            {
                throw new BaseException.BadRequestException("invalid_input", "Page must be greater than 0.");
            }

            if (pageSize <= 0)
            {
                throw new BaseException.BadRequestException("invalid_input", "Page size must be greater than 0.");
            }

            IQueryable<StatusChange> query = _unitOfWork.GetRepository<StatusChange>().Entities
                .Where(cr => !cr.DeletedTime.HasValue || cr.DeletedBy == null);

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

            if (statusChanges == null || !statusChanges.Any())
            {
                throw new BaseException.NotFoundException("not_found", "No status changes found for the specified page.");
            }

            return _mapper.Map<IList<StatusChangeDto>>(statusChanges);
        }

        // Get status changes by OrderId (only active records)
        public async Task<IList<StatusChangeDto>> GetByOrderId(string orderId)
        {
            // Validate id format
            if (!Guid.TryParse(orderId, out var guidId))
            {
                throw new BaseException.BadRequestException("invalid_input", "ID is not in a valid GUID format.");
            }

            if (string.IsNullOrWhiteSpace(orderId))
            {
                throw new BaseException.BadRequestException("invalid_input", "Order ID cannot be null or empty.");
            }

            IQueryable<StatusChange> query = _unitOfWork.GetRepository<StatusChange>().Entities
                .Where(sc => sc.OrderId == orderId && (!sc.DeletedTime.HasValue || sc.DeletedBy == null));

            var statusChanges = await query.ToListAsync();

            if (!statusChanges.Any())
            {
                throw new BaseException.NotFoundException("not_found", "No status changes found for the given OrderId.");
            }

            return _mapper.Map<IList<StatusChangeDto>>(statusChanges);
        }

        // Create a new status change
        public async Task<bool> Create(StatusChangeForCreationDto createStatusChange, string username)
        {
            // Validate id format
            if (!Guid.TryParse(createStatusChange.OrderId, out var orderGuidId))
            {
                throw new BaseException.BadRequestException("invalid_input", "ID is not in a valid GUID format.");
            }
            // Validate
            var validationResult = await _creationValidator.ValidateAsync(createStatusChange);

            if (!validationResult.IsValid)
            {
                throw new BaseException.BadRequestException("validation_failed", validationResult.Errors.Select(e => e.ErrorMessage).FirstOrDefault());
            }

            // Check if OrderId exists
            var order = await _unitOfWork.GetRepository<Order>().Entities
                .FirstOrDefaultAsync(o => o.Id == createStatusChange.OrderId && (!o.DeletedTime.HasValue || o.DeletedBy == null));

            if (order == null)
            {
                throw new BaseException.NotFoundException("order_not_found", "Order not found.");
            }

            // Validate Status Flow
            //var currentStatus = order.Status;

            //if (!IsValidStatusTransition(currentStatus, createStatusChange.Status))
            //{
            //    throw new BaseException.BadRequestException("invalid_status_transition", $"Cannot transition from {order.Status} to {createStatusChange.Status}.");
            //}

            var statusChangeEntity = _mapper.Map<StatusChange>(createStatusChange);

            statusChangeEntity.ChangeTime = DateTime.UtcNow;

            // Set metadata
            statusChangeEntity.CreatedBy = username; 
            statusChangeEntity.LastUpdatedBy = username;

            await _unitOfWork.GetRepository<StatusChange>().InsertAsync(statusChangeEntity);
            await _unitOfWork.SaveAsync();

            return true;
        }

        // Update an existing status change
        //public async Task<bool> Update(string id, StatusChangeForUpdateDto updatedStatusChange, string username)
        //{
        //    // Validate id format
        //    if (!Guid.TryParse(id, out var guidId))
        //    {
        //        throw new BaseException.BadRequestException("invalid_input", "ID is not in a valid GUID format.");
        //    }

        //    var validationResult = await _updateValidator.ValidateAsync(updatedStatusChange);
        //    if (!validationResult.IsValid)
        //    {
        //        throw new BaseException.BadRequestException("validation_failed", validationResult.Errors.Select(e => e.ErrorMessage).FirstOrDefault());
        //    }

        //    var statusChangeEntity = await _unitOfWork.GetRepository<StatusChange>().Entities
        //        .FirstOrDefaultAsync(p => p.Id == id && (!p.DeletedTime.HasValue || p.DeletedBy == null));

        //    if (statusChangeEntity == null)
        //    {
        //        throw new BaseException.NotFoundException("not_found", "Status Change not found");
        //    }

        //    // Define valid statuses within the method
        //    var validStatuses = new List<string>
        //    {
        //        "Pending",
        //        "Canceled",
        //        "Awaiting Payment",
        //        "Processing",
        //        "Delivering",
        //        "Shipped",
        //        "Delivery Failed",
        //        "On Hold",
        //        "Delivering Retry",
        //        "Refund Requested",
        //        "Refund Denied",
        //        "Refund Approve",
        //        "Returning",
        //        "Return Failed",
        //        "Returned",
        //        "Refunded",
        //        "Closed"
        //    };

        //    // Check if the new status is valid
        //    if (!validStatuses.Contains(updatedStatusChange.Status))
        //    {
        //        throw new BaseException.BadRequestException("invalid_status", $"The status '{updatedStatusChange.Status}' is not valid.");
        //    }

        //    _mapper.Map(updatedStatusChange, statusChangeEntity);

        //    statusChangeEntity.ChangeTime = DateTime.UtcNow;

        //    statusChangeEntity.LastUpdatedTime = DateTime.UtcNow;
        //    statusChangeEntity.LastUpdatedBy = username;

        //    await _unitOfWork.GetRepository<StatusChange>().UpdateAsync(statusChangeEntity);
        //    await _unitOfWork.SaveAsync();

        //    return true;
        //}

        // Soft delete status change
        //public async Task<bool> Delete(string id, string username)
        //{
        //    // Validate id format
        //    if (!Guid.TryParse(id, out var guidId))
        //    {
        //        throw new BaseException.BadRequestException("invalid_input", "ID is not in a valid GUID format.");
        //    }

        //    var statusChangeRepo = _unitOfWork.GetRepository<StatusChange>();
        //    var statusChangeEntity = await statusChangeRepo.Entities.FirstOrDefaultAsync(x => x.Id == id);
        //    if (statusChangeEntity == null || statusChangeEntity.DeletedTime.HasValue || statusChangeEntity.DeletedBy != null)
        //    {
        //        throw new BaseException.NotFoundException("not_found", "Status Change not found");
        //    }

        //    statusChangeEntity.DeletedTime = DateTime.UtcNow;
        //    statusChangeEntity.DeletedBy = username;

        //    await statusChangeRepo.UpdateAsync(statusChangeEntity);
        //    await _unitOfWork.SaveAsync();

        //    return true;
        //}

        //private bool IsValidStatusTransition(string currentStatus, string newStatus)
        //{
        //    var validStatusTransitions = new Dictionary<string, List<string>>
        //    {
        //        { "Pending", new List<string> { "Canceled", "Awaiting Payment" } },
        //        { "Awaiting Payment", new List<string> { "Canceled", "Processing" } },
        //        { "Processing", new List<string> { "Delivering" } },
        //        { "Delivering", new List<string> { "Shipped", "Delivery Failed" } },
        //        { "Delivery Failed", new List<string> { "On Hold" } },
        //        { "On Hold", new List<string> { "Delivering Retry", "Refund Requested" } },
        //        { "Refund Requested", new List<string> { "Refund Denied", "Refund Approve" } },
        //        { "Refund Approve", new List<string> { "Returning" } },
        //        { "Returning", new List<string> { "Return Failed", "Returned" } },
        //        { "Return Failed", new List<string> { "On Hold" } },
        //        { "Returned", new List<string> { "Refunded" } },
        //        { "Refunded", new List<string> { "Closed" } },
        //        { "Canceled", new List<string> { "Closed" } },
        //        { "Delivering Retry", new List<string> { "Delivering" } }
        //    };

        //    return validStatusTransitions.TryGetValue(currentStatus, out var validNextStatuses) && validNextStatuses.Contains(newStatus);
        //}
    }
}
