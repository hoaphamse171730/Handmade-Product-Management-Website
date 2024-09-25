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
        public async Task<ActionResult<IEnumerable<StatusChange>>> GetStatusChanges()
        {
            IList<StatusChange> statusChanges = await _statusChangeService.GetAll();
            return Ok(BaseResponse<IList<StatusChange>>.OkResponse(statusChanges));
        }

        // GET: api/StatusChange/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<StatusChange>> GetStatusChange(string id)
        {
            StatusChange statusChange = await _statusChangeService.GetById(id);
            return Ok(BaseResponse<StatusChange>.OkResponse(statusChange));
        }

        // POST: api/StatusChange
        [HttpPost]
        public async Task<ActionResult<StatusChange>> CreateStatusChange(StatusChange statusChange)
        {
            StatusChange createdStatusChange = await _statusChangeService.Create(statusChange);
            return CreatedAtAction(nameof(GetStatusChange), new { id = createdStatusChange.Id }, createdStatusChange);
        }

        // PUT: api/StatusChange/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<StatusChange>> UpdateStatusChange(string id, StatusChange updatedStatusChange)
        {
            StatusChange statusChange = await _statusChangeService.Update(id, updatedStatusChange);
            return Ok(BaseResponse<StatusChange>.OkResponse(statusChange));
        }

        // DELETE: api/StatusChange/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteStatusChange(string id)
        {
            bool success = await _statusChangeService.Delete(id);
            return NoContent();
        }
    }
}
