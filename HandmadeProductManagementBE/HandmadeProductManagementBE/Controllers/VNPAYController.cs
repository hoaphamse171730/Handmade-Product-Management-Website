using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.VariationOptionModelViews;
using HandmadeProductManagement.ModelViews.VNPayModelViews;
using HandmadeProductManagement.Services.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace HandmadeProductManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VNPAYController : ControllerBase
    {
        private readonly IVNPayService _VNPAYService;

        public VNPAYController(IVNPayService vNPAYService)
        {
            _VNPAYService = vNPAYService;
        }
        //dùng để test
        [HttpGet("get-transaction-status-vnpay")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetTransactionStatusVNPay(string orderId, Guid userId, String urlReturn)
        {
            var response = new BaseResponse<string>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Transaction created successfully.",
                Data = await _VNPAYService.GetTransactionStatusVNPay(orderId, userId, urlReturn)
            };
            return Ok(response);
        }
        // Tự lấy param từ url và token, có thể dùng sau khi deploy
/*
        [HttpGet("get-transaction-status-vnpay")]
        [Authorize(Roles = "CUSTOMER")]
        public async Task<IActionResult> GetTransactionStatusVNPay([FromQuery] string orderId)
        {

            try
            {

                Guid userId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value);
                var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
                var response = await _VNPAYService.GetTransactionStatusVNPay(orderId, userId, baseUrl);

                if (response != null)
                {
                    Redirect(response);
                }

                return Ok(response);
            }
            catch (Exception)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), "Bad Request");
            }
        }

*/
        [HttpGet("vnpay-payment")]
        public async Task<IActionResult> VNPAYPayment()
        {
            

            VNPAYRequest request = new()
            {
                VnpSecureHash = Request.Query["vnp_SecureHash"],
                VnpOrderInfo = Request.Query["vnp_OrderInfo"],
                VnpAmount = Request.Query["vnp_Amount"],
                VnpTransactionNo = Request.Query["vnp_TransactionNo"],
                VnpCardType = Request.Query["vnp_CardType"],
                VnpTransactionStatus = Request.Query["vnp_TransactionStatus"],
                VnpBankCode = Request.Query["vnp_BankCode"],
                VnpBankTranNo = Request.Query["vnp_BankTranNo"],
                VnpTxnRef = Request.Query["vnp_TxnRef"],
                VnpPayDate = Request.Query["vnp_PayDate"],
                VnpResponseCode = Request.Query["vnp_ResponseCode"],
                VnpTmnCode = Request.Query["vnp_TmnCode"]
            };


            var paymentStatus = await _VNPAYService.VNPAYPayment(request);
            //redirect về webpage của project, sau khi deploy set lại returnUrl trong service có thể mở ra
            /*if (paymentStatus.IsSucceed && paymentStatus.Text != null)
            {
                return Redirect(paymentStatus.Text);
            }*/

            return Ok(paymentStatus);
        }
    }
}
