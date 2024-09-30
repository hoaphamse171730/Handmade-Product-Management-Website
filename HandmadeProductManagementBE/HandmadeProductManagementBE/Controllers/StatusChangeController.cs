using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Contract.Repositories.Entity;
using Microsoft.AspNetCore.Mvc;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Services.Service;

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
                return StatusCode(500, BaseResponse<string>.FailResponse(ex.Message, StatusCodeHelper.ServerError));
            }
        }

        // GET: api/cancelreason/page?page=1&pageSize=10
        [HttpGet("page")]
        public async Task<IActionResult> GetByPage([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            // Validate page and pageSize
            if (page <= 0)
            {
                return BadRequest(BaseResponse<string>.FailResponse("Page number must be greater than 0."));
            }

            if (pageSize <= 0)
            {
                return BadRequest(BaseResponse<string>.FailResponse("Page size must be greater than 0."));
            }

            try
            {
                var paginatedResult = await _statusChangeService.GetByPage(page, pageSize);
                // Wrap result in BaseResponse
                return Ok(BaseResponse<IList<StatusChange>>.OkResponse(paginatedResult));
            }
            catch (Exception ex)
            {
                // Handle exceptions and return appropriate response
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
                return NotFound(BaseResponse<string>.FailResponse("No status changes found for the given OrderId.", StatusCodeHelper.NotFound));
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, BaseResponse<string>.FailResponse(ex.Message, StatusCodeHelper.ServerError));
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
                return NotFound(BaseResponse<string>.FailResponse("No status changes found for the given OrderId.", StatusCodeHelper.NotFound));
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, BaseResponse<string>.FailResponse(ex.Message, StatusCodeHelper.ServerError));
            }
        }

        // POST: api/StatusChange
        [HttpPost]
        public async Task<ActionResult<BaseResponse<StatusChange>>> CreateStatusChange(StatusChange statusChange)
        {
            if (string.IsNullOrWhiteSpace(statusChange.Status))
            {
                ModelState.AddModelError("status", "Status is required.");
            }

            if (string.IsNullOrWhiteSpace(statusChange.OrderId))
            {
                ModelState.AddModelError("orderId", "OrderId is required.");
            }

            if (statusChange.ChangeTime == default(DateTime))
            {
                ModelState.AddModelError("changeTime", "ChangeTime is required.");
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(BaseResponse<string>.FailResponse("Validation failed: " + string.Join("; ", errors)));
            }

            try
            {
                StatusChange createdStatusChange = await _statusChangeService.Create(statusChange);
                return CreatedAtAction(nameof(GetStatusChange), new { id = createdStatusChange.Id },
                       BaseResponse<StatusChange>.OkResponse(createdStatusChange));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(BaseResponse<string>.FailResponse(ex.Message));
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, BaseResponse<string>.FailResponse(ex.Message, StatusCodeHelper.ServerError));
            }
        }

        // PUT: api/StatusChange/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<BaseResponse<StatusChange>>> UpdateStatusChange(string id, StatusChange updatedStatusChange)
        {
            if (string.IsNullOrWhiteSpace(updatedStatusChange.Status))
            {
                ModelState.AddModelError("status", "Status is required.");
            }

            if (string.IsNullOrWhiteSpace(updatedStatusChange.OrderId))
            {
                ModelState.AddModelError("orderId", "OrderId is required.");
            }

            if (updatedStatusChange.ChangeTime == default(DateTime))
            {
                ModelState.AddModelError("changeTime", "ChangeTime is required.");
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(BaseResponse<string>.FailResponse("Validation failed: " + string.Join("; ", errors)));
            }

            try
            {
                StatusChange statusChange = await _statusChangeService.Update(id, updatedStatusChange);
                return Ok(BaseResponse<StatusChange>.OkResponse(statusChange));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(BaseResponse<string>.FailResponse(ex.Message));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(BaseResponse<string>.FailResponse("No status changes found for the given OrderId.", StatusCodeHelper.NotFound));
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, BaseResponse<string>.FailResponse(ex.Message, StatusCodeHelper.ServerError));
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
                    return NotFound(BaseResponse<string>.FailResponse("Status Change not found", StatusCodeHelper.NotFound));
                }
                return Ok(BaseResponse<string>.OkResponse("Status Change deleted successfully."));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(BaseResponse<string>.FailResponse("No status changes found for the given OrderId.", StatusCodeHelper.NotFound));
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, BaseResponse<string>.FailResponse(ex.Message, StatusCodeHelper.ServerError));
            }
        }
    }
}
