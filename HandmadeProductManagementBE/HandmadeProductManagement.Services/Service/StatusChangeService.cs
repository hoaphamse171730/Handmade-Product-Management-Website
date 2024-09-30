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
            IQueryable<StatusChange> query = _unitOfWork.GetRepository<StatusChange>().Entities
                .Where(sc => sc.DeletedTime == null && sc.DeletedBy == null);

            return await query.ToListAsync();
        }

        // Get cancel reasons by page with validation
        public async Task<IList<StatusChange>> GetByPage(int page, int pageSize)
        {
            if (page <= 0)
                throw new ArgumentException("Page number must be greater than 0.");

            if (pageSize <= 0)
                throw new ArgumentException("Page size must be greater than 0.");

            IQueryable<StatusChange> query = _unitOfWork.GetRepository<StatusChange>().Entities
                .Where(sc => sc.DeletedTime == null && sc.DeletedBy == null);

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        // Get status changes by OrderId (only active records)
        public async Task<IList<StatusChange>> GetByOrderId(string orderId)
        {
            IQueryable<StatusChange> query = _unitOfWork.GetRepository<StatusChange>()
                .Entities
                .Where(sc => sc.OrderId == orderId && sc.DeletedTime == null && sc.DeletedBy == null);

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
            var statusChange = await _unitOfWork.GetRepository<StatusChange>().Entities
                .Where(sc => sc.Id == id && sc.DeletedTime == null && sc.DeletedBy == null)
                .FirstOrDefaultAsync();

            return statusChange ?? throw new KeyNotFoundException("Status change not found");
        }

        // Create a new status change
        public async Task<StatusChange> Create(StatusChange statusChange)
        {
            // Check if OrderId exists
            var orderExists = await _unitOfWork.GetRepository<Order>().GetByIdAsync(statusChange.OrderId);
            if (orderExists == null)
            {
                throw new ArgumentException("OrderId does not exist.");
            }

            // Validate ChangeTime, Status, and OrderId
            if (statusChange.ChangeTime == default)
                throw new ArgumentException("ChangeTime cannot be null or default.");
            if (string.IsNullOrWhiteSpace(statusChange.Status))
                throw new ArgumentException("Status cannot be null or empty.");
            if (string.IsNullOrWhiteSpace(statusChange.OrderId))
                throw new ArgumentException("OrderId cannot be null or empty.");

            // Set the audit fields
            statusChange.CreatedBy = "currentUser";
            statusChange.CreatedTime = DateTimeOffset.UtcNow;

            await _unitOfWork.GetRepository<StatusChange>().InsertAsync(statusChange);
            await _unitOfWork.SaveAsync();
            return statusChange;
        }


        // Update an existing status change
        public async Task<StatusChange> Update(string id, StatusChange updatedStatusChange)
        {
            // Check if OrderId exists
            var orderExists = await _unitOfWork.GetRepository<Order>().GetByIdAsync(updatedStatusChange.OrderId);
            if (orderExists == null)
            {
                throw new ArgumentException("OrderId does not exist.");
            }

            var existingStatusChange = await GetById(id);
            if (existingStatusChange == null)
                throw new KeyNotFoundException("Status change not found");

            // Validate ChangeTime, Status, and OrderId
            if (updatedStatusChange.ChangeTime == default)
                throw new ArgumentException("ChangeTime cannot be null or default.");
            if (string.IsNullOrWhiteSpace(updatedStatusChange.Status))
                throw new ArgumentException("Status cannot be null or empty.");
            if (string.IsNullOrWhiteSpace(updatedStatusChange.OrderId))
                throw new ArgumentException("OrderId cannot be null or empty.");

            // Update the fields
            existingStatusChange.ChangeTime = updatedStatusChange.ChangeTime;
            existingStatusChange.Status = updatedStatusChange.Status;
            existingStatusChange.OrderId = updatedStatusChange.OrderId;

            // Set the audit fields
            existingStatusChange.LastUpdatedBy = "currentUser";
            existingStatusChange.LastUpdatedTime = DateTimeOffset.UtcNow;

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
