using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
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

        // Get all status changes
        public async Task<IList<StatusChange>> GetAll()
        {
            IQueryable<StatusChange> query = _unitOfWork.GetRepository<StatusChange>().Entities;
            return await query.ToListAsync();
        }

        // Get cancel reasons by page with validation
        public async Task<IList<StatusChange>> GetByPage(int page, int pageSize)
        {
            if (page <= 0)
                throw new ArgumentException("Page number must be greater than 0.");

            if (pageSize <= 0)
                throw new ArgumentException("Page size must be greater than 0.");

            IQueryable<StatusChange> query = _unitOfWork.GetRepository<StatusChange>().Entities;

            return await query
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();
        }

        // Get status changes by OrderId
        public async Task<IList<StatusChange>> GetByOrderId(string orderId)
        {
            // Validate OrderId is not null or empty
            //if (string.IsNullOrWhiteSpace(orderId))
            //{
            //    throw new ArgumentException("OrderId cannot be null or empty.");
            //}

            IQueryable<StatusChange> query = _unitOfWork.GetRepository<StatusChange>()
                                                        .Entities
                                                        .Where(sc => sc.OrderId == orderId);

            var statusChanges = await query.ToListAsync();

            if (statusChanges == null || !statusChanges.Any())
            {
                throw new KeyNotFoundException("No status changes found for the given OrderId.");
            }

            return statusChanges;
        }


        // Get status change by Id
        public async Task<StatusChange> GetById(string id)
        {
            var statusChange = await _unitOfWork.GetRepository<StatusChange>().GetByIdAsync(id);
            return statusChange ?? throw new KeyNotFoundException("Status change not found");
        }

        // Create a new status change
        public async Task<StatusChange> Create(StatusChange statusChange)
        {
            // Check if OrderId exist
            var orderExists = await _unitOfWork.GetRepository<Order>().GetByIdAsync(statusChange.OrderId);
            if (orderExists == null)
            {
                throw new ArgumentException("OrderId does not exist.");
            }

            // Validate ChangeTime
            if (statusChange.ChangeTime == default)
            {
                throw new ArgumentException("ChangeTime cannot be null or default.");
            }

            // Validate Status
            if (string.IsNullOrWhiteSpace(statusChange.Status))
            {
                throw new ArgumentException("Status cannot be null or empty.");
            }

            // Validate OrderId
            if (string.IsNullOrWhiteSpace(statusChange.OrderId))
            {
                throw new ArgumentException("OrderId cannot be null or empty.");
            }

            await _unitOfWork.GetRepository<StatusChange>().InsertAsync(statusChange);
            await _unitOfWork.SaveAsync();
            return statusChange;
        }

        // Update an existing status change
        public async Task<StatusChange> Update(string id, StatusChange updatedStatusChange)
        {
            // Check if OrderId exist
            var orderExists = await _unitOfWork.GetRepository<Order>().GetByIdAsync(updatedStatusChange.OrderId);
            if (orderExists == null)
            {
                throw new ArgumentException("OrderId does not exist.");
            }

            var existingStatusChange = await GetById(id);
            if (existingStatusChange == null)
                throw new KeyNotFoundException("Cancel Reason not found");

            // Validate ChangeTime
            if (updatedStatusChange.ChangeTime == default)
            {
                throw new ArgumentException("ChangeTime cannot be null or default.");
            }

            // Validate Status
            if (string.IsNullOrWhiteSpace(updatedStatusChange.Status))
            {
                throw new ArgumentException("Status cannot be null or empty.");
            }

            // Validate OrderId
            if (string.IsNullOrWhiteSpace(updatedStatusChange.OrderId))
            {
                throw new ArgumentException("OrderId cannot be null or empty.");
            }

            existingStatusChange.ChangeTime = updatedStatusChange.ChangeTime;
            existingStatusChange.Status = updatedStatusChange.Status;
            existingStatusChange.OrderId = updatedStatusChange.OrderId;

            _unitOfWork.GetRepository<StatusChange>().Update(existingStatusChange);
            await _unitOfWork.SaveAsync();
            return existingStatusChange;
        }

        //  Soft delete
        public async Task<bool> Delete(string id)
        {
            var statusChange = await GetById(id);
            if (statusChange == null)
                return false;

            // Set DeletedTime to current time and update the DeletedBy field
            statusChange.DeletedTime = DateTimeOffset.UtcNow;
            statusChange.DeletedBy = "currentUser";

            await _unitOfWork.GetRepository<StatusChange>().UpdateAsync(statusChange);
            await _unitOfWork.SaveAsync();
            return true;
        }
    }
}
