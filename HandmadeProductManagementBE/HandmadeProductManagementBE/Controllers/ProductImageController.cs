using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.ProductImageModelViews;
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

        [HttpPost("Upload/{productId}")]
        public async Task<IActionResult> UploadProductImage(List<IFormFile> files, string productId)
        {
            // Tiến hành xử lý các tệp đã gửi
            var response = new BaseResponse<bool>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Success",
                Data = await _productImageService.UploadProductImage(files, productId)
            };

            return Ok(response);
        }

        [HttpDelete("Delete/{imageId}")]
        public async Task<IActionResult> DeleteProductImage(string imageId)
        {
            var response = new BaseResponse<bool>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Success",
                Data = await _productImageService.DeleteProductImage(imageId)
            };
            return Ok(response);

        }

        [HttpGet("GetImage/{productId}")]
        public async Task<IActionResult> GetProductImagesByProductId(string productId)
        {
            var response = new BaseResponse<IList<productImageByIdResponse>>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Success",
                Data = await _productImageService.GetProductImageById(productId)
            };
            return Ok(response);

        }

    }
}
