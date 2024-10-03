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

        public OrderDetailController(IOrderDetailService orderDetailService)
        {
            _orderDetailService = orderDetailService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDetailDto>>> GetOrderDetails()
        {
            try
            {
                IList<OrderDetailDto> orderDetails = await _orderDetailService.GetAll();
                return Ok(BaseResponse<IList<OrderDetailDto>>.OkResponse(orderDetails));
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, BaseResponse<string>.FailResponse(ex.Message));
            }
        }

        // GET: api/OrderDetail/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDetailDto>> GetOrderDetail(string id)
        {
            try
            {
                var orderDetail = await _orderDetailService.GetById(id);
                if (orderDetail == null)
                {
                    return NotFound(BaseResponse<string>.FailResponse("OrderDetail not found"));
                }
                return Ok(BaseResponse<OrderDetailDto>.OkResponse(orderDetail));
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, BaseResponse<string>.FailResponse(ex.Message));
            }
        }
        [HttpPost]
        public async Task<ActionResult<OrderDetailDto>> CreateOrderDetail(OrderDetailForCreationDto orderDetailForCreation)
        {
            try
            {
                var createdOrderDetail = await _orderDetailService.Create(orderDetailForCreation);
                return Ok(BaseResponse<OrderDetailDto>.OkResponse(createdOrderDetail));

            }
            catch (System.Exception ex)
            {
                return StatusCode(500, BaseResponse<string>.FailResponse(ex.Message));
            }
        }


        [HttpPut("{orderId}/{productId}")]
        public async Task<ActionResult<OrderDetailDto>> UpdateOrderDetail(string orderId, OrderDetailForUpdateDto orderDetailForUpdate)
        {
            try
            {
                await _orderDetailService.Update(orderId,  orderDetailForUpdate);
                return Ok(BaseResponse<string>.OkResponse("OrderDetail updated successfully"));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(BaseResponse<string>.FailResponse("OrderDetail not found"));
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, BaseResponse<string>.FailResponse(ex.Message));
            }
        }


        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteOrderDetail(string id)
        {
            try
            {
                await _orderDetailService.SoftDelete(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(BaseResponse<string>.FailResponse("OrderDetail not found"));
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, BaseResponse<string>.FailResponse(ex.Message));
            }
        }

        [HttpDelete("soft-delete/{id}")]
        public async Task<ActionResult> SoftDeleteOrderDetail(string id)
        {
            try
            {
                await _orderDetailService.SoftDelete(id);
                return Ok(BaseResponse<string>.OkResponse("OrderDetail soft deleted successfully"));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(BaseResponse<string>.FailResponse("OrderDetail not found"));
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, BaseResponse<string>.FailResponse(ex.Message));
            }
        }

        [HttpGet("by-order/{orderId}")]
        public async Task<ActionResult<IEnumerable<OrderDetailDto>>> GetOrderDetailsByOrderId(string orderId)
        {
            try
            {
                var orderDetails = await _orderDetailService.GetByOrderId(orderId);
                return Ok(BaseResponse<IList<OrderDetailDto>>.OkResponse(orderDetails));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(BaseResponse<string>.FailResponse("No order details found for the given Order ID"));
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, BaseResponse<string>.FailResponse(ex.Message));
            }
        }
    }
}
