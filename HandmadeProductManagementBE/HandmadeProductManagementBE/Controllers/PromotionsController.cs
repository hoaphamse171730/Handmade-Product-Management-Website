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
            var promotions = await _promotionService.GetAll();
            return Ok(promotions);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPromotion(string id)
        {
            var promotion = await _promotionService.GetById(id);
            return Ok(promotion);
        }

        [HttpPost]
        public async Task<ActionResult<PromotionDto>> CreatePromotion(PromotionForCreationDto promotionForCreation)
        {
            var createdPromotion = await _promotionService.Create(promotionForCreation);
            return Ok(createdPromotion);
        }


        [HttpPut("{id}")]
        public async Task<ActionResult> UpdatePromotion(string id, PromotionForUpdateDto promotionForUpdate)
        {
            var response = await _promotionService.Update(id, promotionForUpdate);
            return Ok(response);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePromotion(string id)
        {

            var response = await _promotionService.Delete(id);
            return Ok(response);
        }

        [HttpDelete("soft-delete/{id}")]
        public async Task<ActionResult> SoftDeletePromotion(string id)
        {
            var response = await _promotionService.SoftDelete(id);
            return Ok(response);
        }

    }
}
