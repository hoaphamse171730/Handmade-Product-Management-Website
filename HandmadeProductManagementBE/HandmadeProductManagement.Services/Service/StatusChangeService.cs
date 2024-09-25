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

        // Get status changes by OrderId
        public async Task<IList<StatusChange>> GetByOrderId(string orderId)
        {
            IQueryable<StatusChange> query = _unitOfWork.GetRepository<StatusChange>()
                                                        .Entities
                                                        .Where(sc => sc.OrderId == orderId);
            return await query.ToListAsync();
        }

        // Get status change by Id
        public async Task<StatusChange> GetById(string id)
        {
            return await _unitOfWork.GetRepository<StatusChange>().Entities
                                     .FirstOrDefaultAsync(sc => sc.Id == id);
        }

        // Create a new status change
        public async Task<StatusChange> Create(StatusChange statusChange)
        {
            await _unitOfWork.GetRepository<StatusChange>().InsertAsync(statusChange);
            await _unitOfWork.SaveAsync();
            return statusChange;
        }

        // Update an existing status change
        public async Task<StatusChange> Update(string id, StatusChange updatedStatusChange)
        {
            var existingStatusChange = await GetById(id);
            if (existingStatusChange == null)
                return null;

            existingStatusChange.ChangeTime = updatedStatusChange.ChangeTime;
            existingStatusChange.Status = updatedStatusChange.Status;
            existingStatusChange.OrderId = updatedStatusChange.OrderId;

            _unitOfWork.GetRepository<StatusChange>().Update(existingStatusChange);
            await _unitOfWork.SaveAsync();
            return existingStatusChange;
        }

        // Delete a status change by Id
        public async Task<bool> Delete(string id)
        {
            var statusChange = await GetById(id);
            if (statusChange == null)
                return false;

            try
            {
                _unitOfWork.GetRepository<StatusChange>().Delete(statusChange.Id);
                await _unitOfWork.SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting StatusChange: {ex.Message}");
                return false;
            }
        }
    }
}
