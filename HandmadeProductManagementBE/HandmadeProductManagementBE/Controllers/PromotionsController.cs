using System.Collections.Generic;
using System.Threading.Tasks;
using HandmadeProductManagement.ModelViews.PromotionModelViews;
using HandmadeProductManagement.Contract.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace HandmadeProductManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PromotionsController : ControllerBase
    {
        private readonly IPromotionService _promotionService;

        public PromotionsController(IPromotionService promotionService)
        {
            _promotionService = promotionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var promotions = await _promotionService.GetAll();
            return Ok(promotions);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePromotion([FromBody] PromotionForCreationDto promotionDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var createdPromotion = await _promotionService.Add(promotionDto);
            return CreatedAtAction(nameof(GetPromotion), new { id = createdPromotion.Id }, createdPromotion);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePromotion(string id, [FromBody] PromotionForUpdateDto promotionDto)
        {
            await _promotionService.UpdatePromotionAsync(id, promotionDto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePromotion(string id)
        {
            try
            {
                _promotionService.Delete(id); 
                return NoContent();
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPromotion(string id)
        {
            var promotion = await _promotionService.GetById(id);
            if (promotion == null)
                return NotFound();
            return Ok(promotion);
        }
    }
}
