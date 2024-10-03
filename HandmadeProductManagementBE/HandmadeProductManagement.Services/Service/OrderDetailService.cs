using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.ModelViews.OrderDetailModelViews;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using HandmadeProductManagement.Contract.Repositories.Entity;

namespace HandmadeProductManagement.Services.Service
{
    public class OrderDetailService : IOrderDetailService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<OrderDetailForCreationDto> _creationValidator;
        private readonly IValidator<OrderDetailForUpdateDto> _updateValidator;

        public OrderDetailService(IUnitOfWork unitOfWork, IMapper mapper, IValidator<OrderDetailForCreationDto> creationValidator, IValidator<OrderDetailForUpdateDto> updateValidator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _creationValidator = creationValidator;
            _updateValidator = updateValidator;
        }

        public async Task<IList<OrderDetailDto>> GetAll()
        {
            var orderDetails = await _unitOfWork.GetRepository<OrderDetail>().Entities.Where(p => p.DeletedTime == null).ToListAsync();
            return _mapper.Map<IList<OrderDetailDto>>(orderDetails);
        }

        public async Task<OrderDetailDto> GetById(string id)
        {
            var orderDetail = await _unitOfWork.GetRepository<OrderDetail>().Entities.FirstOrDefaultAsync(p => p.Id == id && p.DeletedTime == null);
            if (orderDetail == null)
                throw new KeyNotFoundException("Order Detail not found");
            return _mapper.Map<OrderDetailDto>(orderDetail);
        }

        public async Task<OrderDetailDto> Create(OrderDetailForCreationDto orderDetailForCreation)
        {
            var validationResult = await _creationValidator.ValidateAsync(orderDetailForCreation);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);
            var orderDetailEntity = _mapper.Map<OrderDetail>(orderDetailForCreation);
            orderDetailEntity.CreatedBy = "user";
            orderDetailEntity.LastUpdatedBy = "user";
            await _unitOfWork.GetRepository<OrderDetail>().InsertAsync(orderDetailEntity);
            await _unitOfWork.SaveAsync();
            return _mapper.Map<OrderDetailDto>(orderDetailEntity);
        }

        public async Task<OrderDetailDto> Update(string orderDetailId, OrderDetailForUpdateDto orderDetailForUpdate)
        {
            var validationResult = await _updateValidator.ValidateAsync(orderDetailForUpdate);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);
            var orderDetailEntity = await _unitOfWork.GetRepository<OrderDetail>().Entities
                .FirstOrDefaultAsync(p => p.Id == orderDetailId && p.DeletedTime == null);
            if (orderDetailEntity == null)
                throw new KeyNotFoundException("Order detail not found"); 
            _mapper.Map(orderDetailForUpdate, orderDetailEntity);
            orderDetailEntity.LastUpdatedTime = DateTime.UtcNow;
            await _unitOfWork.GetRepository<OrderDetail>().UpdateAsync(orderDetailEntity);
            await _unitOfWork.SaveAsync();
            return _mapper.Map<OrderDetailDto>(orderDetailEntity);
        }

        public async Task<bool> SoftDelete(string id)
        {
            var orderDetailEntity = await _unitOfWork.GetRepository<OrderDetail>().Entities.FirstOrDefaultAsync(p => p.Id == id);
            if (orderDetailEntity == null)
                throw new KeyNotFoundException("Order Detail not found");
            orderDetailEntity.DeletedTime = DateTime.UtcNow;
            await _unitOfWork.GetRepository<OrderDetail>().UpdateAsync(orderDetailEntity);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<IList<OrderDetailDto>> GetByOrderId(string orderId)
        {
            var orderDetails = await _unitOfWork.GetRepository<OrderDetail>().Entities
                .Where(od => od.OrderId == orderId && od.DeletedTime == null)
                .ToListAsync();
            if (orderDetails == null || orderDetails.Count == 0)
                throw new KeyNotFoundException("No order details found for the given Order ID.");
            return _mapper.Map<IList<OrderDetailDto>>(orderDetails);
        }
    }
}
