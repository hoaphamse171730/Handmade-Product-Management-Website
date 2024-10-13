using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.PromotionModelViews;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace HandmadeProductManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PromotionsController : ControllerBase
    {
        private readonly IPromotionService _promotionService;

        public PromotionsController(IPromotionService promotionService) => _promotionService = promotionService;

        [HttpGet]
        public async Task<IActionResult> GetPromotions(int pageNumber = 1, int pageSize = 10)
        {
            var result = await _promotionService.GetAll(pageNumber, pageSize);
            var response = new BaseResponse<IList<PromotionDto>>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Promotions retrieved successfully.",
                Data = result
            };
            return Ok(response);
        }
        
        
        [HttpGet(Name = "GetExpiredPromotions")]
        public async Task<IActionResult> GetExpiredPromotions(int pageNumber = 1, int pageSize = 10)
        {
            var result = await _promotionService.GetExpiredPromotions(pageNumber, pageSize);
            var response = new BaseResponse<IList<PromotionDto>>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Expired promotions retrieved successfully.",
                Data = result
            };
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPromotion(string id)
        {
            var promotion = await _promotionService.GetById(id);
            var response = new BaseResponse<PromotionDto>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Promotion retrieved successfully.",
                Data = promotion
            };
            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreatePromotion(PromotionForCreationDto promotionForCreation)
        {
            var result = await _promotionService.Create(promotionForCreation);
            var response = new BaseResponse<bool>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK, 
                Message = "Promotion created successfully.",
                Data = result
            };
            return Ok(response);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdatePromotion(string id, PromotionForUpdateDto promotionForUpdate)
        {
            var result = await _promotionService.Update(id, promotionForUpdate);
            var response = new BaseResponse<bool>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Promotion updated successfully.",
                Data = result
            };
            return Ok(response);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SoftDeletePromotion(string id)
        {
            var result = await _promotionService.SoftDelete(id);
            var response = new BaseResponse<bool>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Promotion soft-deleted successfully.",
                Data = result
            };
            return Ok(response);
        }
    }
}
