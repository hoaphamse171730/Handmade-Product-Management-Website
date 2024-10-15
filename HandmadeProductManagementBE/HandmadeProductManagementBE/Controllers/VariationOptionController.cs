using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using Microsoft.AspNetCore.Mvc;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.VariationOptionModelViews;
using Microsoft.AspNetCore.Authorization;

namespace HandmadeProductManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VariationOptionController : ControllerBase
    {
        private readonly IVariationOptionService _variationOptionService;

        public VariationOptionController(IVariationOptionService variationOptionService)
        {
            _variationOptionService = variationOptionService;
        }

        // GET: api/variationoption/variation/{variationId}
        [HttpGet("variation/{variationId}")]
        public async Task<IActionResult> GetByVariationId(string variationId)
        {
            var response = new BaseResponse<IList<VariationOptionDto>>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Get Variation Options by Variation ID successfully.",
                Data = await _variationOptionService.GetByVariationId(variationId)
            };
            return Ok(response);
        }

        [Authorize(Roles = "Seller")]
        // POST: api/variationoption
        [HttpPost]
        public async Task<IActionResult> CreateVariationOption([FromBody] VariationOptionForCreationDto variationOption)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var result = await _variationOptionService.Create(variationOption, userId);
            var response = new BaseResponse<bool>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Created Variation Option successfully.",
                Data = result
            };
            return Ok(response);
        }

        [Authorize(Roles = "Seller")]
        // PATCH: api/variationoption/{id}
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateVariationOption(string id, [FromBody] VariationOptionForUpdateDto variationOption)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var result = await _variationOptionService.Update(id, variationOption, userId);
            var response = new BaseResponse<bool>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Updated Variation Option successfully.",
                Data = result
            };
            return Ok(response);
        }

        [Authorize(Roles = "Seller")]
        // DELETE: api/variationoption/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVariationOption(string id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            await _variationOptionService.Delete(id, userId);

            var response = new BaseResponse<string>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = $"Variation Option with ID {id} has been successfully deleted.",
                Data = null
            };
            return Ok(response);
        }
    }
}
