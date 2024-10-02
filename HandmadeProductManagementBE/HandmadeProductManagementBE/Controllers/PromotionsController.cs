using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.PaymentModelViews;
using HandmadeProductManagement.ModelViews.PromotionModelViews;
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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PromotionDto>>> GetPromotions()
        {
            try
            {
                IList<PromotionDto> promotions = await _promotionService.GetAll();
                return Ok(BaseResponse<IList<PromotionDto>>.OkResponse(promotions));
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, BaseResponse<string>.FailResponse(ex.Message));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PromotionDto>> GetPromotion(string id)
        {
            try
            {
                PromotionDto promotion = await _promotionService.GetById(id);
                return Ok(BaseResponse<PromotionDto>.OkResponse(promotion));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(BaseResponse<string>.FailResponse("Promotion not found"));
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, BaseResponse<string>.FailResponse(ex.Message));
            }
        }

        [HttpPost]
        public async Task<ActionResult<PromotionDto>> CreatePromotion(PromotionForCreationDto promotionForCreation)
        {
            try
            {
                PromotionDto createdPromotion = await _promotionService.Create(promotionForCreation);
                return Ok(BaseResponse<PromotionDto>.OkResponse(createdPromotion));
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, BaseResponse<string>.FailResponse(ex.Message));
            }
        }


        [HttpPut("{id}")]
        public async Task<ActionResult> UpdatePromotion(string id, PromotionForUpdateDto promotionForUpdate)
        {
            try
            {
                await _promotionService.Update(id, promotionForUpdate);
                return Ok(BaseResponse<string>.OkResponse("Promotion updated successfully"));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(BaseResponse<string>.FailResponse("Promotion not found"));
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, BaseResponse<string>.FailResponse(ex.Message));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePromotion(string id)
        {
            try
            {
                await _promotionService.Delete(id);
                return Ok(new BaseResponse<bool>(StatusCodeHelper.OK, "Promotion deleted successfully.", true));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(BaseResponse<string>.FailResponse("Promotion not found"));
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, BaseResponse<string>.FailResponse(ex.Message));
            }
        }

        [HttpDelete("soft-delete/{id}")]
        public async Task<ActionResult> SoftDeletePromotion(string id)
        {
            try
            {
                await _promotionService.SoftDelete(id);
                return Ok(BaseResponse<string>.OkResponse("Promotion soft-deleted successfully"));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(BaseResponse<string>.FailResponse("Promotion not found"));
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, BaseResponse<string>.FailResponse(ex.Message));
            }
        }

        [HttpGet("Expired/{id}")]
        public async Task<ActionResult> ExpiredPromotion(string id)
        {
            var isExpired = await _promotionService.IsExpiredPromotionAsync(id);

            var response = new BaseResponse<bool>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "...",
                Data = isExpired
            };
            return Ok(response);
        }
    }
}
