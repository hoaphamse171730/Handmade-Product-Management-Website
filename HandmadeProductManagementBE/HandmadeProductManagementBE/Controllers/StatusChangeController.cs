using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using Microsoft.AspNetCore.Mvc;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.StatusChangeModelViews;
using Microsoft.AspNetCore.Authorization;

namespace HandmadeProductManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatusChangeController : ControllerBase
    {
        private readonly IStatusChangeService _statusChangeService;

        public StatusChangeController(IStatusChangeService statusChangeService)
        {
            _statusChangeService = statusChangeService;
        }

        [Authorize]
        [HttpGet("page")]
        public async Task<IActionResult> GetByPage([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var response = new BaseResponse<IList<StatusChangeDto>>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Get Status Change sucessfully!",
                Data = await _statusChangeService.GetByPage(page, pageSize)
            };
            return Ok(response);
        }

        [Authorize]
        [HttpGet("Order/{orderId}")]
        public async Task<IActionResult> GetStatusChangesByOrderId(string orderId)
        {
            var response = new BaseResponse<IList<StatusChangeDto>>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Get Status Change sucessfully!",
                Data = await _statusChangeService.GetByOrderId(orderId)
            };
            return Ok(response);
        }

        //[Authorize(Roles = "Admin")]
        //[HttpPost]
        //public async Task<IActionResult> CreateStatusChange([FromBody] StatusChangeForCreationDto statusChange)
        //{
        //    var username = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
        //    var result = await _statusChangeService.Create(statusChange, username);
        //    var response = new BaseResponse<bool>
        //    {
        //        Code = "Success",
        //        StatusCode = StatusCodeHelper.OK,
        //        Message = "Created Status Change successfully!",
        //        Data = result
        //    };
        //    return Ok(response);
        //}

        //[Authorize(Roles = "Admin")]
        //[HttpPut("{id}")]
        //public async Task<IActionResult> UpdateStatusChange(string id, [FromBody] StatusChangeForUpdateDto updatedStatusChange)
        //{
        //    var username = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
        //    var result = await _statusChangeService.Update(id, updatedStatusChange, username);
        //    var response = new BaseResponse<bool>
        //    {
        //        Code = "Success",
        //        StatusCode = StatusCodeHelper.OK,
        //        Message = "Updated Status Change successfully!",
        //        Data = result
        //    };
        //    return Ok(response);
        //}

        //[Authorize(Roles = "Admin")]
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteStatusChange(string id)
        //{
        //    var username = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
        //    await _statusChangeService.Delete(id, username);

        //    var response = new BaseResponse<string>
        //    {
        //        Code = "Success",
        //        StatusCode = StatusCodeHelper.OK,
        //        Message = $"Status Change with ID {id} has been successfully deleted.",
        //        Data = null
        //    };
        //    return Ok(response);
        //}
    }
}
