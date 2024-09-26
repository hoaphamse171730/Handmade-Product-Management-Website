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
    public class PromotionsController : ControllerBase
    {
        private readonly IPromotionService _promotionService;

        public PromotionsController(IPromotionService promotionService)
        {
            _promotionService = promotionService;
        }

        // GET: api/Promotions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Promotion>>> GetPromotions()
        {
            IList<Promotion> promotions = await _promotionService.GetAll();
            return Ok(BaseResponse<IList<Promotion>>.OkResponse(promotions));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Promotion>> GetPromotion(string id)
        {
            Promotion promotion = await _promotionService.GetById(id);
            if (promotion == null)
            {
                return NotFound(BaseResponse<Promotion>.FailResponse("Promotion not found"));
            }
            return Ok(BaseResponse<Promotion>.OkResponse(promotion));
        }

        [HttpPost]
        public async Task<ActionResult<Promotion>> CreatePromotion(Promotion promotion)
        {
            Promotion createdPromotion = await _promotionService.Create(promotion);
            return CreatedAtAction(nameof(GetPromotion), new { id = createdPromotion.Id }, BaseResponse<Promotion>.OkResponse(createdPromotion));
        }
        
        [HttpPut("{id}")]
        public async Task<ActionResult<Promotion>> UpdatePromotion(string id, Promotion updatedPromotion)
        {
            Promotion promotion = await _promotionService.Update(id, updatedPromotion);
            if (promotion == null)
            {
                return NotFound(BaseResponse<Promotion>.FailResponse("Promotion not found"));
            }
            return Ok(BaseResponse<Promotion>.OkResponse(promotion));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePromotion(string id)
        {
            bool success = await _promotionService.Delete(id);
            if (!success)
            {
                return NotFound(BaseResponse<Promotion>.FailResponse("Promotion not found"));
            }
            return NoContent();
        }
    }
}
