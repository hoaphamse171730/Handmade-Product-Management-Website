using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.ModelViews.OrderDetailModelViews;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace HandmadeProductManagement.Services.Service
{
    public class OrderDetailService : IOrderDetailService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public OrderDetailService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IList<OrderDetailDto>> GetAll()
        {
            var orderDetails = await _unitOfWork.GetRepository<OrderDetail>().Entities
                      .Where(od => !od.DeletedTime.HasValue && od.DeletedBy == null)
                      .ToListAsync();
            var orderDetailsDto = _mapper.Map<IList<OrderDetailDto>>(orderDetails);
            return orderDetailsDto;
        }

        public async Task<OrderDetailDto> GetById(string id)
        {
            var orderDetail = await _unitOfWork.GetRepository<OrderDetail>().Entities
                .FirstOrDefaultAsync(p => p.Id == id && !p.DeletedTime.HasValue && p.DeletedBy == null);

            if (orderDetail == null)
                throw new KeyNotFoundException("Order Detail not found");

            var orderDetailDto = _mapper.Map<OrderDetailDto>(orderDetail);
            return orderDetailDto;
        }

        public async Task<OrderDetailDto> Create(OrderDetailForCreationDto orderDetailForCreation)
        {
            var orderDetailEntity = _mapper.Map<OrderDetail>(orderDetailForCreation);
            await _unitOfWork.GetRepository<OrderDetail>().InsertAsync(orderDetailEntity);
            await _unitOfWork.SaveAsync();
            var orderDetailToReturn = _mapper.Map<OrderDetailDto>(orderDetailEntity);
            return orderDetailToReturn;
        }

        public async Task Update(string id, OrderDetailForUpdateDto orderDetailForUpdate)
        {
            var orderDetailEntity = await _unitOfWork.GetRepository<OrderDetail>().Entities
                    .FirstOrDefaultAsync(p => p.Id == id && !p.DeletedTime.HasValue && p.DeletedBy == null);

            if (orderDetailEntity == null)
                throw new KeyNotFoundException("Order detail not found");

            _mapper.Map(orderDetailForUpdate, orderDetailEntity);
            // orderDetailEntity.LastUpdatedBy = orderDetailForUpdate.UserId.ToString();
            orderDetailEntity.LastUpdatedTime = DateTime.UtcNow;
            await _unitOfWork.GetRepository<OrderDetail>().UpdateAsync(orderDetailEntity);
            await _unitOfWork.SaveAsync();
        }

        public async Task Delete(string id)
        {
            var repo = _unitOfWork.GetRepository<OrderDetail>();
            var orderDetailEntity = await repo.Entities
                .FirstOrDefaultAsync(p => p.Id == id && !p.DeletedTime.HasValue && p.DeletedBy == null);

            if (orderDetailEntity == null)
                throw new KeyNotFoundException("Order Detail not found or has already been deleted");

            orderDetailEntity.DeletedBy = "System"; // update with actual user context later
            orderDetailEntity.DeletedTime = DateTime.UtcNow;

            await repo.UpdateAsync(orderDetailEntity); // Use UpdateAsync to keep object in soft delete
            await _unitOfWork.SaveAsync();
        }


        public async Task SoftDelete(string id)
        {
            var orderDetailEntity = await _unitOfWork.GetRepository<OrderDetail>().Entities
                .FirstOrDefaultAsync(p => p.Id == id);
            if (orderDetailEntity == null)
                throw new KeyNotFoundException("Order Detail not found");
            // orderDetailEntity.DeletedBy = userId.ToString();
            orderDetailEntity.DeletedTime = DateTime.UtcNow;
            await _unitOfWork.GetRepository<OrderDetail>().UpdateAsync(orderDetailEntity);
            await _unitOfWork.SaveAsync();
        }

        public async Task<IList<OrderDetailDto>> GetByOrderId(string orderId)
        {
            var orderDetails = await _unitOfWork.GetRepository<OrderDetail>().Entities
                .Where(od => od.OrderId == orderId)
                .ToListAsync();

            if (orderDetails == null || orderDetails.Count == 0)
                throw new KeyNotFoundException("No order details found for the given Order ID.");

            var orderDetailsDto = _mapper.Map<IList<OrderDetailDto>>(orderDetails);
            return orderDetailsDto;
        }
    }
}