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

            return _mapper.Map<OrderDetailDto>(orderDetail);
        }

        public async Task<OrderDetailDto> Create(OrderDetailForCreationDto orderDetailForCreation)
        {
            var validationResult = await _creationValidator.ValidateAsync(orderDetailForCreation);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var orderDetailEntity = _mapper.Map<OrderDetail>(orderDetailForCreation);
            await _unitOfWork.GetRepository<OrderDetail>().InsertAsync(orderDetailEntity);
            await _unitOfWork.SaveAsync();

            return _mapper.Map<OrderDetailDto>(orderDetailEntity);
        }

        public async Task<OrderDetailDto> Update(string orderId, string productId, OrderDetailForUpdateDto orderDetailForUpdate)
        {
            var validationResult = await _updateValidator.ValidateAsync(orderDetailForUpdate);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);
            var orderDetailEntity = await _unitOfWork.GetRepository<OrderDetail>().Entities
                .FirstOrDefaultAsync(p => p.OrderId == orderId && p.ProductId == productId);
            if (orderDetailEntity == null)
                throw new KeyNotFoundException("Order detail not found");
            _mapper.Map(orderDetailForUpdate, orderDetailEntity);
            orderDetailEntity.LastUpdatedTime = DateTime.UtcNow;
            await _unitOfWork.GetRepository<OrderDetail>().UpdateAsync(orderDetailEntity); 
            await _unitOfWork.SaveAsync();
            return _mapper.Map<OrderDetailDto>(orderDetailEntity);
        }

        //public async Task<OrderDetailDto> Update(string orderId, OrderDetailForUpdateDto orderDetailForUpdate)
        //{
        //    var validationResult = await _updateValidator.ValidateAsync(orderDetailForUpdate);
        //    if (!validationResult.IsValid)
        //        throw new ValidationException(validationResult.Errors);

        //    var orderDetailEntity = await _unitOfWork.GetRepository<OrderDetail>().Entities
        //            .FirstOrDefaultAsync(p => p.Id == orderId && !p.DeletedTime.HasValue && p.DeletedBy == null);

        //    if (orderDetailEntity == null)
        //        throw new KeyNotFoundException("Order detail not found");

        //    _mapper.Map(orderDetailForUpdate, orderDetailEntity);
        //    orderDetailEntity.LastUpdatedTime = DateTime.UtcNow;
        //    await _unitOfWork.GetRepository<OrderDetail>().UpdateAsync(orderDetailEntity);
        //    await _unitOfWork.SaveAsync();

        //    return _mapper.Map<OrderDetailDto>(orderDetailEntity);
        //}

        public async Task<bool> Delete(string id)
        {
            var repo = _unitOfWork.GetRepository<OrderDetail>();
            var orderDetailEntity = await repo.Entities
                .FirstOrDefaultAsync(p => p.Id == id && !p.DeletedTime.HasValue && p.DeletedBy == null);

            if (orderDetailEntity == null)
                throw new KeyNotFoundException("Order Detail not found or has already been deleted");

            orderDetailEntity.DeletedBy = "System"; // update with actual user context later
            orderDetailEntity.DeletedTime = DateTime.UtcNow;
            await repo.UpdateAsync(orderDetailEntity); 
            await _unitOfWork.SaveAsync();
            return true;
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
                .Where(od => od.OrderId == orderId)
                .ToListAsync();
            if (orderDetails == null || orderDetails.Count == 0)
                throw new KeyNotFoundException("No order details found for the given Order ID.");
            return _mapper.Map<IList<OrderDetailDto>>(orderDetails);
        }
    }
}
