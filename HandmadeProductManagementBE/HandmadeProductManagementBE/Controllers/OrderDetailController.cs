using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.ModelViews.OrderDetailModelViews;
using HandmadeProductManagement.Core.Base;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
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
        public async Task<ActionResult<BaseResponse<IList<OrderDetailDto>>>> GetOrderDetails()
        {
            var orderDetails = await _orderDetailService.GetAll();
            return Ok(BaseResponse<IList<OrderDetailDto>>.OkResponse(orderDetails));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BaseResponse<OrderDetailDto>>> GetOrderDetail(string id)
        {
            var orderDetail = await _orderDetailService.GetById(id);

            if (orderDetail == null)
            {
                return NotFound(BaseResponse<OrderDetailDto>.ErrorResponse("OrderDetail not found"));
            }

            return Ok(BaseResponse<OrderDetailDto>.OkResponse(orderDetail));
        }

        [HttpPost]
        public async Task<ActionResult<BaseResponse<OrderDetailDto>>> AddOrderDetail([FromBody] OrderDetailForCreationDto orderDetailDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(BaseResponse<string>.ErrorResponse("Invalid model state"));
            }

            var createdOrderDetail = await _orderDetailService.Add(orderDetailDto);
            return CreatedAtAction(nameof(GetOrderDetail), new { id = createdOrderDetail.OrderDetailId }, BaseResponse<OrderDetailDto>.OkResponse(createdOrderDetail));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrderDetail(string id, [FromBody] OrderDetailForUpdateDto orderDetailDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(BaseResponse<string>.ErrorResponse("Invalid model state"));
            }

            try
            {
                await _orderDetailService.UpdateOrderDetailAsync(id, orderDetailDto);
                return Ok(BaseResponse<string>.OkResponse("OrderDetail updated successfully"));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(BaseResponse<string>.ErrorResponse("OrderDetail not found"));
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, BaseResponse<string>.ErrorResponse($"Error updating order detail: {ex.Message}"));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrderDetail(string id)
        {
            try
            {
                await _orderDetailService.Delete(id);
                return Ok(BaseResponse<string>.OkResponse("OrderDetail deleted successfully"));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(BaseResponse<string>.ErrorResponse("OrderDetail not found"));
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, BaseResponse<string>.ErrorResponse($"Error deleting order detail: {ex.Message}"));
            }
        }
    }
}
