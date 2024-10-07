using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.VariationModelViews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HandmadeProductManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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

        [Authorize(Roles = "Admin")]
        // POST: api/variation
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] VariationForCreationDto variation)
        {
            var username = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
            var result = await _variationService.Create(variation, username);

            var response = new BaseResponse<bool>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Variation created successfully.",
                Data = result
            };
            return Ok(response);
        }

        [Authorize(Roles = "Admin")]
        // PUT: api/variation/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] VariationForUpdateDto variation)
        {
            var username = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
            var result = await _variationService.Update(id, variation, username);

            var response = new BaseResponse<bool>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Variation updated successfully.",
                Data = result
            };
            return Ok(response);
        }

        [Authorize(Roles = "Admin")]
        // DELETE: api/variation/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var username = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
            var result = await _variationService.Delete(id, username);

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
