using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Contract.Repositories.Entity;
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

        public OrderDetailController(IOrderDetailService orderDetailService)
        {
            _orderDetailService = orderDetailService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDetail>>> GetOrderDetails()
        {
            IList<OrderDetail> orderDetails = await _orderDetailService.GetAll();
            return Ok(BaseResponse<IList<OrderDetail>>.OkResponse(orderDetails));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDetail>> GetOrderDetail(string id)
        {
            OrderDetail orderDetail = await _orderDetailService.GetById(id);
            if (orderDetail == null)
            {
                return NotFound(BaseResponse<OrderDetail>.FailResponse("OrderDetail not found"));
            }
            return Ok(BaseResponse<OrderDetail>.OkResponse(orderDetail));
        }

        [HttpPost]
        public async Task<ActionResult<OrderDetail>> CreateOrderDetail(OrderDetail orderDetail)
        {
            OrderDetail createdOrderDetail = await _orderDetailService.Create(orderDetail);
            return CreatedAtAction(nameof(GetOrderDetail), new { id = createdOrderDetail.Id }, BaseResponse<OrderDetail>.OkResponse(createdOrderDetail));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<OrderDetail>> UpdateOrderDetail(string id, OrderDetail updatedOrderDetail)
        {
            OrderDetail orderDetail = await _orderDetailService.Update(id, updatedOrderDetail);
            if (orderDetail == null)
            {
                return NotFound(BaseResponse<OrderDetail>.FailResponse("OrderDetail not found"));
            }
            return Ok(BaseResponse<OrderDetail>.OkResponse(orderDetail));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteOrderDetail(string id)
        {
            bool success = await _orderDetailService.Delete(id);
            if (!success)
            {
                return NotFound(BaseResponse<string>.FailResponse("OrderDetail not found"));
            }
            return NoContent();
        }
    }
}
