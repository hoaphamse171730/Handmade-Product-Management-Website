using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.PaymentDetailModelViews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace HandmadeProductManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentDetailController : ControllerBase
    {
        private readonly IPaymentDetailService _paymentDetailService;

        public PaymentDetailController(IPaymentDetailService paymentDetailService)
        {
            _paymentDetailService = paymentDetailService;
        }
        [Authorize(Roles = "Customer")]
        [HttpPost]
        public async Task<IActionResult> CreatePaymentDetail([FromBody] CreatePaymentDetailDto createPaymentDetailDto)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var createdPaymentDetail = await _paymentDetailService.CreatePaymentDetailAsync(userId, createPaymentDetailDto);
            var response = new BaseResponse<bool>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Payment detail created successfully",
                Data = createdPaymentDetail
            };
            return Ok(response);
        }

        [HttpGet("payment/{paymentId}")]
        public async Task<IActionResult> GetPaymentDetailByPaymentId(string paymentId)
        {
            var paymentDetail = await _paymentDetailService.GetPaymentDetailByPaymentIdAsync(paymentId);
            var response = new BaseResponse<PaymentDetailResponseModel>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Payment detail retrieved successfully",
                Data = paymentDetail
            };
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPaymentDetailById(string id)
        {
            var paymentDetail = await _paymentDetailService.GetPaymentDetailByIdAsync(id);
            var response = new BaseResponse<PaymentDetailResponseModel>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Payment detail retrieved successfully",
                Data = paymentDetail
            };
            return Ok(response);
        }
    }
}