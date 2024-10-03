using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.PaymentModelViews;
using HandmadeProductManagement.ModelViews.PromotionModelViews;
using HandmadeProductManagement.Services.Service;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HandmadeProductManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PromotionsController : ControllerBase
    {
        private readonly IPromotionService _promotionService;
        public PromotionsController(IPromotionService promotionService) => _promotionService = promotionService;

        [HttpGet]
        public async Task<IActionResult> GetPromotions()
        {

            var result = await _promotionService.GetAll();
            return Ok(BaseResponse<IList<PromotionDto>>.OkResponse(result));

        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPromotion(string id)
        {

            var promotion = await _promotionService.GetById(id);
            return Ok(BaseResponse<PromotionDto>.OkResponse(promotion));

        }

        [HttpPost]
        public async Task<ActionResult<BaseResponse<PromotionDto>>> CreatePromotion(PromotionForCreationDto promotionForCreation)
        {

            var result = await _promotionService.Create(promotionForCreation);
            return Ok(BaseResponse<PromotionDto>.OkResponse(result));

        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePromotion(string id, PromotionForUpdateDto promotionForUpdate)
        {
            var result = await _promotionService.Update(id, promotionForUpdate);
            return Ok(BaseResponse<PromotionDto>.OkResponse(result));

        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDeletePromotion(string id)
        {

            var result = await _promotionService.SoftDelete(id);
            return Ok(BaseResponse<bool>.OkResponse(result));

        }

        [HttpGet("Expired/{id}")]
        public async Task<IActionResult> ExpiredPromotion(string id)
        {

            var isExpired = await _promotionService.updatePromotionStatusByRealtime(id);
            var response = new BaseResponse<bool>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Promotion Status Updated Successfully!",
                Data = isExpired
            };
            return Ok(response);

        }
    }
}
