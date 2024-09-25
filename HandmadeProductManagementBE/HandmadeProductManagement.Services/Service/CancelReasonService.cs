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
            return await _unitOfWork.GetRepository<CancelReason>().Entities
                                     .FirstOrDefaultAsync(cr => cr.Id == id);
        }

        // Create a new cancel reason
        public async Task<CancelReason> Create(CancelReason cancelReason)
        {
            await _unitOfWork.GetRepository<CancelReason>().InsertAsync(cancelReason);
            await _unitOfWork.SaveAsync();
            return cancelReason;
        }

        // Update an existing cancel reason
        public async Task<CancelReason> Update(string id, CancelReason updatedCancelReason)
        {
            var existingCancelReason = await GetById(id);
            if (existingCancelReason == null)
                return null;

            existingCancelReason.Description = updatedCancelReason.Description;
            existingCancelReason.RefundRate = updatedCancelReason.RefundRate;

            _unitOfWork.GetRepository<CancelReason>().Update(existingCancelReason);
            await _unitOfWork.SaveAsync();
            return existingCancelReason;
        }

        // Delete a cancel reason by Id (string)
        public async Task<bool> Delete(string id)
        {
            var cancelReason = await GetById(id);
            if (cancelReason == null)
                return false; 
            // Kiểm tra trước khi xóa (nếu có các ràng buộc)
            try
            {
                _unitOfWork.GetRepository<CancelReason>().Delete(cancelReason.Id);
                await _unitOfWork.SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting CancelReason: {ex.Message}");
                return false;
            }
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

            _unitOfWork.GetRepository<CancelReason>().Update(cancelReason);
            await _unitOfWork.SaveAsync();
            return true;
        }
    }
}
