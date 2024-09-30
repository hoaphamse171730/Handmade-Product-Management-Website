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

        // Get all cancel reasons
        public async Task<IList<CancelReason>> GetAll()
        {
            IQueryable<CancelReason> query = _unitOfWork.GetRepository<CancelReason>().Entities;
            return await query.ToListAsync();
        }

        // Get cancel reason by Id (string)
        public async Task<CancelReason> GetById(string id)
        {
            var cancelReason = await _unitOfWork.GetRepository<CancelReason>().GetByIdAsync(id);
            return cancelReason ?? throw new KeyNotFoundException("Cancel reason not found");
        }

        // Create a new cancel reason
        public async Task<CancelReason> Create(CancelReason cancelReason)
        {
            // Validate if RefundRate is float
            float refundRate;
            if (!float.TryParse(cancelReason.RefundRate.ToString(), out refundRate))
            {
                throw new ArgumentException("RefundRate must be a valid float value.");
            }

            // Validate RefundRate between 0 and 1
            if (refundRate < 0 || refundRate > 1)
            {
                throw new ArgumentException("RefundRate must be between 0 and 1.");
            }

            // Validate Description null
            if (cancelReason.Description == null || cancelReason.Description.Trim().Length == 0)
            {
                throw new ArgumentException("Description cannot be null or empty.");
            }

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

            // Validate if RefundRate is float
            float refundRate;
            if (!float.TryParse(updatedCancelReason.RefundRate.ToString(), out refundRate))
            {
                throw new ArgumentException("RefundRate must be a valid float value.");
            }

            // Validate RefundRate between 0 and 1
            if (refundRate < 0 || refundRate > 1)
            {
                throw new ArgumentException("RefundRate must be between 0 and 1.");
            }

            // Validate Description null
            if (updatedCancelReason.Description == null || updatedCancelReason.Description.Trim().Length == 0)
            {
                throw new ArgumentException("Description cannot be null or empty.");
            }

            existingCancelReason.Description = updatedCancelReason.Description;
            existingCancelReason.RefundRate = updatedCancelReason.RefundRate;

            await _unitOfWork.GetRepository<CancelReason>().UpdateAsync(existingCancelReason);
            await _unitOfWork.SaveAsync();
            return existingCancelReason;
        }

        // Delete a cancel reason by Id (string)
        public async Task<bool> Delete(string id)
        {
            var cacncelReasonRepo = _unitOfWork.GetRepository<CancelReason>();
            var cancelReason = await cacncelReasonRepo.GetByIdAsync(id);
            if (cancelReason == null)
                return false;
            await cacncelReasonRepo.DeleteAsync(cancelReason.Id);
            await _unitOfWork.SaveAsync();
            return true;
        }

        // Soft delete 
        public async Task<bool> SoftDelete(string id)
        {
            var cancelReason = await GetById(id);
            if (cancelReason == null)
                return false;

            // Set DeletedTime to current time and update the DeletedBy field
            cancelReason.DeletedTime = DateTimeOffset.UtcNow;
            cancelReason.DeletedBy = "currentUser";

            await _unitOfWork.GetRepository<CancelReason>().UpdateAsync(cancelReason);
            await _unitOfWork.SaveAsync();
            return true;
        }
    }
}
