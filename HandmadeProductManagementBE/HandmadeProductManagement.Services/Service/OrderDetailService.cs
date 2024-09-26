using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HandmadeProductManagement.Services.Service
{
    public class OrderDetailService : IOrderDetailService
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderDetailService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IList<OrderDetail>> GetAll()
        {
            var orderDetails = await _unitOfWork.GetRepository<OrderDetail>().Entities.ToListAsync();
            return orderDetails;
        }

        public async Task<OrderDetail> GetById(string id)
        {
            var orderDetail = await _unitOfWork.GetRepository<OrderDetail>().GetByIdAsync(id);
            if (orderDetail == null)
            {
                throw new KeyNotFoundException("OrderDetail not found");
            }
            return orderDetail;
        }
        public async Task<OrderDetail> Create(OrderDetail orderDetail)
        {
            var orderDetailRepo = _unitOfWork.GetRepository<OrderDetail>();
            await orderDetailRepo.InsertAsync(orderDetail);
            await _unitOfWork.SaveAsync();
            return orderDetail;
        }

        public async Task<OrderDetail> Update(string id, OrderDetail updatedOrderDetail)
        {
            var orderDetailRepo = _unitOfWork.GetRepository<OrderDetail>();
            var existingOrderDetail = await orderDetailRepo.GetByIdAsync(id);
            if (existingOrderDetail == null)
            {
                throw new KeyNotFoundException("OrderDetail not found");
            }
            existingOrderDetail.ProductId = updatedOrderDetail.ProductId;
            existingOrderDetail.OrderId = updatedOrderDetail.OrderId;
            existingOrderDetail.ProductQuantity = updatedOrderDetail.ProductQuantity;
            existingOrderDetail.UnitPrice = updatedOrderDetail.UnitPrice;
            await orderDetailRepo.UpdateAsync(updatedOrderDetail);
            await _unitOfWork.SaveAsync();
            return existingOrderDetail;
        }

        public async Task<bool> Delete(string id)
        {
            var orderDetailRepo = _unitOfWork.GetRepository<OrderDetail>();
            var orderDetail = await orderDetailRepo.GetByIdAsync(id);
            if (orderDetail == null)
            {
                return false;
            }
            await orderDetailRepo.DeleteAsync(orderDetail);
            await _unitOfWork.SaveAsync();
            return true;
        }
    }
}
