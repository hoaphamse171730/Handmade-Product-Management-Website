using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.ModelViews.OrderDetailModelViews;
using HandmadeProductManagement.ModelViews.OrderModelViews;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HandmadeProductManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderDetailController : ControllerBase
    {
        private readonly IOrderDetailService _orderDetailService;

        public OrderDetailController(IOrderDetailService orderDetailService) => _orderDetailService = orderDetailService;

        [HttpGet]
        public async Task<IActionResult> GetOrderDetails()
        {
            var result = await _orderDetailService.GetAll();
            return Ok(BaseResponse<IList<OrderDetailDto>>.OkResponse(result));
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderDetail(string id)
        {
            var result = await _orderDetailService.GetById(id);
            return Ok(BaseResponse<OrderDetailDto>.OkResponse(result));
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrderDetail(OrderDetailForCreationDto orderDetailForCreation)
        {

            var result = await _orderDetailService.Create(orderDetailForCreation);
            return Ok(BaseResponse<OrderDetailDto>.OkResponse(result));
        }

        [HttpPut("{orderDetailId}")]
        public async Task<IActionResult> UpdateOrderDetail(string orderDetailId, OrderDetailForUpdateDto orderDetailForUpdate)
        {

            var result = await _orderDetailService.Update(orderDetailId, orderDetailForUpdate);
            return Ok(BaseResponse<OrderDetailDto>.OkResponse(result));

        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrderDetail(string id)
        {

            var result = await _orderDetailService.SoftDelete(id);
            return Ok(BaseResponse<bool>.OkResponse(result));

        }

        [HttpGet("by-order/{orderId}")]
        public async Task<IActionResult> GetOrderDetailsByOrderId(string orderId)
        {

            var result = await _orderDetailService.GetByOrderId(orderId);
            return Ok(BaseResponse<IList<OrderDetailDto>>.OkResponse(result));

        }
    }
}
