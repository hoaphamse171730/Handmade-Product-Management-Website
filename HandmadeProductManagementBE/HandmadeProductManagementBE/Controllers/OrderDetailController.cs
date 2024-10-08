using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.ModelViews.OrderDetailModelViews;
using HandmadeProductManagement.ModelViews.OrderModelViews;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using HandmadeProductManagement.Contract.Services;
using HandmadeProductManagement.Core.Constants;

namespace HandmadeProductManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderDetailController : ControllerBase
    {
        private readonly IOrderDetailService _orderDetailService;

        public OrderDetailController(IOrderDetailService orderDetailService) => _orderDetailService = orderDetailService;
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetOrderDetails()
        {
            var result = await _orderDetailService.GetAll();
            var response = new BaseResponse<IList<OrderDetailDto>>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Success",
                Data = result
            };
            return Ok(response);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetOrderDetail(string id)
        {
            var result = await _orderDetailService.GetById(id);
            var response = new BaseResponse<OrderDetailDto>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Success",
                Data = result
            };
            return Ok(response);
        }
    }
}
