using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.VariationModelViews;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HandmadeProductManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VariationController : ControllerBase
    {
        private readonly IVariationService _variationService;

        public VariationController(IVariationService variationService)
        {
            _variationService = variationService;
        }

        // GET: api/variation/page?page=1&pageSize=10
        [HttpGet("page")]
        public async Task<IActionResult> GetByPage([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var response = new BaseResponse<IList<VariationDto>>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Get Variations by page successfully.",
                Data = await _variationService.GetByPage(page, pageSize)
            };
            return Ok(response);
        }

        // GET: api/variation/category/{categoryId}
        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetByCategoryId(string categoryId)
        {
            var response = new BaseResponse<IList<VariationDto>>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Get Variations by Category successfully.",
                Data = await _variationService.GetByCategoryId(categoryId)
            };
            return Ok(response);
        }

        // POST: api/variation
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] VariationForCreationDto variation)
        {
            try
            {
                var result = await _variationService.Create(variation);

                var response = new BaseResponse<bool>
                {
                    Code = "Success",
                    StatusCode = StatusCodeHelper.OK,
                    Message = "Variation created successfully.",
                    Data = result
                };
                return Ok(response);
            }
            catch (DbUpdateException ex)
            {
                var innerExceptionMessage = ex.InnerException?.Message ?? ex.Message;

                var errorResponse = new BaseResponse<string>
                {
                    Code = "DbUpdateException",
                    StatusCode = StatusCodeHelper.ServerError,
                    Message = $"An error occurred while saving the entity changes: {innerExceptionMessage}",
                    Data = innerExceptionMessage
                };

                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        // PUT: api/variation/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] VariationForUpdateDto variation)
        {
            var result = await _variationService.Update(id, variation);

            var response = new BaseResponse<bool>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Variation updated successfully.",
                Data = result
            };
            return Ok(response);
        }

        // DELETE: api/variation/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _variationService.Delete(id);

            var response = new BaseResponse<bool>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Variation deleted successfully.",
                Data = result
            };
            return Ok(response);
        }
    }
}
