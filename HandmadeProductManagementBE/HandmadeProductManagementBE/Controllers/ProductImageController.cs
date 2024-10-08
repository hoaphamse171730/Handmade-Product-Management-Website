using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Constants;

using Microsoft.AspNetCore.Mvc;

namespace HandmadeProductManagementAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class ProductImageController : ControllerBase
    {
        private readonly IProductImageService _productImageService;

        public ProductImageController(IProductImageService productImageService) {
            _productImageService = productImageService;
        }
        [HttpPost("Upload")]
        public async Task<IActionResult> UploadProductImage(IFormFile file, string productId)
        {
            var response = new BaseResponse<bool>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Success",
                Data = await _productImageService.UploadProductImage(file, productId)
            };
            return Ok(response);

        }
    }
}
