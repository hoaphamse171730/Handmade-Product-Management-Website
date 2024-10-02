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
            var result = _orderDetailService.GetAll();
            return Ok(result);
        }

        // GET: api/OrderDetail/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderDetail(string id)
        {
            var result = await _orderDetailService.GetById(id);
            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> CreateOrderDetail(OrderDetailForCreationDto orderDetailForCreation)
        {
            var result = _orderDetailService.Create(orderDetailForCreation);
            return Ok(result);
        }


        [HttpPut("{orderId}/{productId}")]
        public async Task<ActionResult<OrderDetailDto>> UpdateOrderDetail(string orderId, string productId, OrderDetailForUpdateDto orderDetailForUpdate)
        {
            var result = await _orderDetailService.Update(orderId, productId, orderDetailForUpdate);
            return Ok(result);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrderDetail(string id)
        {
            var result = await _orderDetailService.Delete(id);
            return Ok(result);
        }

        [HttpDelete("soft-delete/{id}")]
        public async Task<IActionResult> SoftDeleteOrderDetail(string id)
        {
            var result = await _orderDetailService.SoftDelete(id);
            return Ok(result);
        }

        [HttpGet("by-order/{orderId}")]
        public async Task<IActionResult> GetOrderDetailsByOrderId(string orderId)
        {
            var result = await _orderDetailService.GetByOrderId(orderId);
            return Ok(result);
        }
    }
}
