using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.ModelViews.StatusChangeModelViews;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace HandmadeProductManagement.Services.Service
{
    public class StatusChangeService : IStatusChangeService
    {
        private readonly IUnitOfWork _unitOfWork;

        public StatusChangeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Get all status changes (only active records)
        public async Task<IList<StatusChangeResponseModel>> GetAll()
        {
            IQueryable<StatusChange> query = _unitOfWork.GetRepository<StatusChange>().Entities
                .Where(sc => !sc.DeletedTime.HasValue || sc.DeletedBy == null);

            var result = await query.Select(statusChange => new StatusChangeResponseModel
            {
                Id = statusChange.Id.ToString(),
                OrderId = statusChange.OrderId,
                Status = statusChange.Status,
                ChangeTime = statusChange.ChangeTime
            }).ToListAsync();

            return result;
        }

        // Get status changes by page (only active records)
        public async Task<IList<StatusChangeResponseModel>> GetByPage(int page, int pageSize)
        {
            if (page <= 0)
                throw new BaseException.BadRequestException("invalid_input","Page must be greater than 0.");

            if (pageSize <= 0)
                throw new BaseException.BadRequestException("invalid_input", "Page size must be greater than 0.");

            IQueryable<StatusChange> query = _unitOfWork.GetRepository<StatusChange>().Entities
                .Where(sc => !sc.DeletedTime.HasValue || sc.DeletedBy == null);

            var result = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(statusChange => new StatusChangeResponseModel
                {
                    Id = statusChange.Id.ToString(),
                    OrderId = statusChange.OrderId,
                    Status = statusChange.Status,
                    ChangeTime = statusChange.ChangeTime
                })
                .ToListAsync();

            return result;
        }

        // Get status changes by OrderId (only active records)
        public async Task<IList<StatusChangeResponseModel>> GetByOrderId(string orderId)
        {
            IQueryable<StatusChange> query = _unitOfWork.GetRepository<StatusChange>().Entities
                .Where(sc => sc.OrderId == orderId && (!sc.DeletedTime.HasValue || sc.DeletedBy == null));

            var statusChanges = await query.Select(statusChange => new StatusChangeResponseModel
            {
                Id = statusChange.Id.ToString(),
                OrderId = statusChange.OrderId,
                Status = statusChange.Status,
                ChangeTime = statusChange.ChangeTime
            }).ToListAsync();

            if (!statusChanges.Any())
            {
                throw new KeyNotFoundException("No status changes found for the given OrderId.");
            }

            return statusChanges;
        }

        // Create a new status change
        public async Task<StatusChangeResponseModel> Create(CreateStatusChangeDto createStatusChange)
        {
            // Check if OrderId exists
            var orderExists = await _unitOfWork.GetRepository<Order>().GetByIdAsync(createStatusChange.OrderId);
            if (orderExists == null)
            {
                throw new BaseException.BadRequestException("non_exist_orderId","OrderId does not exist.");
            }

            var statusChange = new StatusChange
            {
                OrderId = createStatusChange.OrderId,
                Status = createStatusChange.Status,
                ChangeTime = createStatusChange.ChangeTime
            };

            // Set metadata
            statusChange.CreatedBy = "currentUser"; // Update with actual user info
            statusChange.LastUpdatedBy = "currentUser"; // Update with actual user info

            await _unitOfWork.GetRepository<StatusChange>().InsertAsync(statusChange);
            await _unitOfWork.SaveAsync();

            return new StatusChangeResponseModel
            {
                Id = statusChange.Id.ToString(),
                OrderId = statusChange.OrderId,
                Status = statusChange.Status,
                ChangeTime = statusChange.ChangeTime
            };
        }

        // Update an existing status change
        public async Task<StatusChangeResponseModel> Update(string id, CreateStatusChangeDto updatedStatusChange)
        {
            var existingStatusChange = await _unitOfWork.GetRepository<StatusChange>().GetByIdAsync(id);
            if (existingStatusChange == null)
                throw new KeyNotFoundException("Status change not found");

            // Check if OrderId exists
            var orderExists = await _unitOfWork.GetRepository<Order>().GetByIdAsync(updatedStatusChange.OrderId);
            if (orderExists == null)
            {
                throw new BaseException.BadRequestException("non_exist_orderId", "OrderId does not exist.");
            }

            existingStatusChange.OrderId = updatedStatusChange.OrderId;
            existingStatusChange.Status = updatedStatusChange.Status;
            existingStatusChange.ChangeTime = updatedStatusChange.ChangeTime;
            existingStatusChange.LastUpdatedBy = "currentUser"; // Update with actual user info
            existingStatusChange.LastUpdatedTime = DateTimeOffset.UtcNow;

            await _unitOfWork.GetRepository<StatusChange>().UpdateAsync(existingStatusChange);
            await _unitOfWork.SaveAsync();

            return new StatusChangeResponseModel
            {
                Id = existingStatusChange.Id.ToString(),
                OrderId = existingStatusChange.OrderId,
                Status = existingStatusChange.Status,
                ChangeTime = existingStatusChange.ChangeTime
            };
        }

        // Soft delete status change
        public async Task<bool> Delete(string id)
        {
            var statusChange = await _unitOfWork.GetRepository<StatusChange>().GetByIdAsync(id);
            if (statusChange == null)
            {

            }

            statusChange.DeletedTime = DateTimeOffset.UtcNow;
            statusChange.DeletedBy = "currentUser"; // Update with actual user info

            await _unitOfWork.GetRepository<StatusChange>().UpdateAsync(statusChange);
            await _unitOfWork.SaveAsync();
            return true;
        }
    }
}
