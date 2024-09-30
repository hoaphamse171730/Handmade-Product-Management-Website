using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Contract.Repositories.Entity;
using Microsoft.AspNetCore.Mvc;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Utils;

namespace HandmadeProductManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CancelReasonController : ControllerBase
    {
        private readonly ICancelReasonService _cancelReasonService;

        public CancelReasonController(ICancelReasonService cancelReasonService)
        {
            _cancelReasonService = cancelReasonService;
        }

        // GET: api/CancelReason
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CancelReason>>> GetCancelReasons()
        {
            try
            {
                IList<CancelReason> reasons = await _cancelReasonService.GetAll();
                return Ok(BaseResponse<IList<CancelReason>>.OkResponse(reasons));
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, BaseResponse<string>.FailResponse(ex.Message));
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
                var paginatedResult = await _cancelReasonService.GetByPage(page, pageSize);
                // Wrap result in BaseResponse
                return Ok(BaseResponse<IList<CancelReason>>.OkResponse(paginatedResult));
            }
            catch (Exception ex)
            {
                // Handle exceptions and return appropriate response
                return StatusCode(500, BaseResponse<string>.FailResponse(ex.Message));
            }
        }


        // GET: api/CancelReason/{id} (string id)
        [HttpGet("{id}")]
        public async Task<ActionResult<CancelReason>> GetCancelReason(string id)
        {
            try
            {
                CancelReason reason = await _cancelReasonService.GetById(id);
                return Ok(BaseResponse<CancelReason>.OkResponse(reason));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(BaseResponse<string>.FailResponse("Cancel Reason not found", StatusCodeHelper.NotFound));
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, BaseResponse<string>.FailResponse(ex.Message, StatusCodeHelper.ServerError));
            }
        }

        // POST: api/CancelReason
        [HttpPost]
        public async Task<ActionResult<BaseResponse<CancelReason>>> CreateCancelReason(CancelReason reason)
        {
            if (string.IsNullOrWhiteSpace(reason.Description))
            {
                ModelState.AddModelError("description", "Description is required.");
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
                CancelReason createdReason = await _cancelReasonService.Create(reason);
                return CreatedAtAction(nameof(GetCancelReason), new { id = createdReason.Id },
                       BaseResponse<CancelReason>.OkResponse(createdReason));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(BaseResponse<string>.FailResponse(ex.Message));
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, BaseResponse<string>.FailResponse(ex.Message));
            }
        }

        // PUT: api/CancelReason/{id} (string id)
        [HttpPut("{id}")]
        public async Task<ActionResult<BaseResponse<CancelReason>>> UpdateCancelReason(string id, CancelReason updatedReason)
        {
            if (string.IsNullOrWhiteSpace(updatedReason.Description))
            {
                ModelState.AddModelError("description", "Description is required.");
            }

            if (updatedReason.RefundRate < 0 || updatedReason.RefundRate > 1)
            {
                ModelState.AddModelError("refundRate", "RefundRate must be between 0 and 100.");
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
                CancelReason reason = await _cancelReasonService.Update(id, updatedReason);
                return Ok(BaseResponse<CancelReason>.OkResponse(reason));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(BaseResponse<string>.FailResponse("Cancel Reason not found", StatusCodeHelper.NotFound));
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


        // DELETE: api/CancelReason/{id} (string id)
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCancelReason(string id)
        {
            try
            {
                bool success = await _cancelReasonService.Delete(id);
                if (!success)
                {
                    return NotFound(BaseResponse<string>.FailResponse($"Cancel Reason with ID {id} not found.", StatusCodeHelper.NotFound));
                }
                return Ok(BaseResponse<string>.OkResponse($"Cancel Reason with ID {id} has been successfully deleted."));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(BaseResponse<string>.FailResponse("Cancel Reason not found", StatusCodeHelper.NotFound));
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, BaseResponse<string>.FailResponse(ex.Message, StatusCodeHelper.ServerError));
            }
        }

        // PUT: api/CancelReason/{id}/soft-delete
        [HttpPut("{id}/soft-delete")]
        public async Task<ActionResult> SoftDeleteCancelReason(string id)
        {
            try
            {
                bool success = await _cancelReasonService.SoftDelete(id);
                if (!success)
                {
                    return NotFound(BaseResponse<string>.FailResponse($"Cancel Reason with ID {id} not found.", StatusCodeHelper.NotFound));
                }
                return Ok(BaseResponse<string>.OkResponse($"Cancel Reason with ID {id} has been successfully deleted."));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(BaseResponse<string>.FailResponse("Cancel Reason not found", StatusCodeHelper.NotFound));
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, BaseResponse<string>.FailResponse(ex.Message, StatusCodeHelper.ServerError));
            }
            
        }

    }
}
