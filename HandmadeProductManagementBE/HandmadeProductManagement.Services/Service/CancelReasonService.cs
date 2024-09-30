using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace HandmadeProductManagement.Services.Service
{
    public class CancelReasonService : ICancelReasonService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CancelReasonService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Get all cancel reasons (only active records)
        public async Task<IList<CancelReason>> GetAll()
        {
            IQueryable<CancelReason> query = _unitOfWork.GetRepository<CancelReason>().Entities
                .Where(cr => cr.DeletedTime == null && cr.DeletedBy == null);

            return await query.ToListAsync();
        }

        // Get cancel reasons by page (only active records)
        public async Task<IList<CancelReason>> GetByPage(int page, int pageSize)
        {
            if (page <= 0)
                throw new ArgumentException("Page number must be greater than 0.");

            if (pageSize <= 0)
                throw new ArgumentException("Page size must be greater than 0.");

            IQueryable<CancelReason> query = _unitOfWork.GetRepository<CancelReason>().Entities
                .Where(cr => cr.DeletedTime == null && cr.DeletedBy == null);

            return await query
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();
        }

        // Get cancel reason by Id (only active records)
        public async Task<CancelReason> GetById(string id)
        {
            var cancelReason = await _unitOfWork.GetRepository<CancelReason>()
                .Entities
                .Where(cr => cr.Id == id && cr.DeletedTime == null && cr.DeletedBy == null)
                .FirstOrDefaultAsync();

            return cancelReason ?? throw new KeyNotFoundException("Cancel reason not found");
        }

        // Create a new cancel reason
        public async Task<CancelReason> Create(CancelReason cancelReason)
        {
            // Validate RefundRate is between 0 and 1
            if (cancelReason.RefundRate < 0 || cancelReason.RefundRate > 1)
            {
                throw new ArgumentException("RefundRate must be between 0 and 1.");
            }

            // Validate Description is not null or empty
            if (string.IsNullOrWhiteSpace(cancelReason.Description))
            {
                throw new ArgumentException("Description cannot be null or empty.");
            }

            // Set metadata
            cancelReason.CreatedBy = "currentUser"; // Update with actual user info
            cancelReason.LastUpdatedBy = "currentUser"; // Update with actual user info
            cancelReason.CreatedTime = DateTimeOffset.UtcNow;
            cancelReason.LastUpdatedTime = DateTimeOffset.UtcNow;

            await _unitOfWork.GetRepository<CancelReason>().InsertAsync(cancelReason);
            await _unitOfWork.SaveAsync();

            return cancelReason;
        }

        // Update an existing cancel reason
        public async Task<CancelReason> Update(string id, CancelReason updatedCancelReason)
        {
            var existingCancelReason = await GetById(id);
            if (existingCancelReason == null)
                throw new KeyNotFoundException("Cancel Reason not found");

            // Validate RefundRate is between 0 and 1
            if (updatedCancelReason.RefundRate < 0 || updatedCancelReason.RefundRate > 1)
            {
                throw new ArgumentException("RefundRate must be between 0 and 1.");
            }

            // Validate Description is not null or empty
            if (string.IsNullOrWhiteSpace(updatedCancelReason.Description))
            {
                throw new ArgumentException("Description cannot be null or empty.");
            }

            // Update fields
            existingCancelReason.Description = updatedCancelReason.Description;
            existingCancelReason.RefundRate = updatedCancelReason.RefundRate;
            existingCancelReason.LastUpdatedBy = "currentUser"; // Update with actual user info
            existingCancelReason.LastUpdatedTime = DateTimeOffset.UtcNow;

            await _unitOfWork.GetRepository<CancelReason>().UpdateAsync(existingCancelReason);
            await _unitOfWork.SaveAsync();
            return existingCancelReason;
        }

        // Soft delete 
        public async Task<bool> Delete(string id)
        {
            var cancelReason = await GetById(id);
            if (cancelReason == null)
                return false;

            // Set DeletedTime and DeletedBy
            cancelReason.DeletedTime = DateTimeOffset.UtcNow;
            cancelReason.DeletedBy = "currentUser"; // Update with actual user info

            await _unitOfWork.GetRepository<CancelReason>().UpdateAsync(cancelReason);
            await _unitOfWork.SaveAsync();
            return true;
        }
    }
}
