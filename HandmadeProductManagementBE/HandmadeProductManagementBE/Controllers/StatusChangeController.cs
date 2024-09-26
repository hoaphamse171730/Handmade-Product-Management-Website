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
    public class StatusChangeController : ControllerBase
    {
        private readonly IStatusChangeService _statusChangeService;

        public StatusChangeController(IStatusChangeService statusChangeService)
        {
            _statusChangeService = statusChangeService;
        }

        // GET: api/StatusChange
        [HttpGet]
        public async Task<ActionResult<BaseResponse<IList<StatusChange>>>> GetStatusChanges()
        {
            try
            {
                IList<StatusChange> statusChanges = await _statusChangeService.GetAll();
                return Ok(BaseResponse<IList<StatusChange>>.OkResponse(statusChanges));
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, BaseResponse<string>.FailResponse(ex.Message));
            }
        }

        // GET: api/StatusChange/Order/{orderId}
        [HttpGet("Order/{orderId}")]
        public async Task<ActionResult<BaseResponse<IList<StatusChange>>>> GetStatusChangesByOrderId(string orderId)
        {
            try
            {
                IList<StatusChange> statusChanges = await _statusChangeService.GetByOrderId(orderId);
                return Ok(BaseResponse<IList<StatusChange>>.OkResponse(statusChanges));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(BaseResponse<string>.FailResponse("No status changes found for the given OrderId."));
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, BaseResponse<string>.FailResponse(ex.Message));
            }
        }

        // GET: api/StatusChange/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<BaseResponse<StatusChange>>> GetStatusChange(string id)
        {
            try
            {
                StatusChange statusChange = await _statusChangeService.GetById(id);
                return Ok(BaseResponse<StatusChange>.OkResponse(statusChange));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(BaseResponse<string>.FailResponse("Status Change not found"));
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, BaseResponse<string>.FailResponse(ex.Message));
            }
        }

        // POST: api/StatusChange
        [HttpPost]
        public async Task<ActionResult<BaseResponse<StatusChange>>> CreateStatusChange(StatusChange statusChange)
        {
            try
            {
                StatusChange createdStatusChange = await _statusChangeService.Create(statusChange);
                return CreatedAtAction(nameof(GetStatusChange), new { id = createdStatusChange.Id }, 
                       BaseResponse<StatusChange>.OkResponse(createdStatusChange));
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, BaseResponse<string>.FailResponse(ex.Message));
            }
        }

        // PUT: api/StatusChange/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<BaseResponse<StatusChange>>> UpdateStatusChange(string id, StatusChange updatedStatusChange)
        {
            try
            {
                StatusChange statusChange = await _statusChangeService.Update(id, updatedStatusChange);
                return Ok(BaseResponse<StatusChange>.OkResponse(statusChange));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(BaseResponse<string>.FailResponse("Status Change not found"));
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, BaseResponse<string>.FailResponse(ex.Message));
            }
        }

        // DELETE: api/StatusChange/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<BaseResponse<string>>> DeleteStatusChange(string id)
        {
            try
            {
                bool success = await _statusChangeService.Delete(id);
                if (!success)
                {
                    return NotFound(BaseResponse<string>.FailResponse("Status Change not found"));
                }
                return Ok(BaseResponse<string>.OkResponse("Status Change deleted successfully."));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(BaseResponse<string>.FailResponse("Status Change not found"));
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, BaseResponse<string>.FailResponse(ex.Message));
            }
        }
    }
}
