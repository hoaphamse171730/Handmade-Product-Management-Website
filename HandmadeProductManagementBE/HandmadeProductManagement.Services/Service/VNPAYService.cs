using Firebase.Auth;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.OrderModelViews;
using HandmadeProductManagement.ModelViews.StatusChangeModelViews;
using HandmadeProductManagement.ModelViews.VNPayModelViews;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace HandmadeProductManagement.Services.Service
{
    public class VNPAYService : IVNPayService
    {
        private readonly IOrderService _orderService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStatusChangeService _statusChangeService;


        public VNPAYService(IUnitOfWork unitOfWork, IOrderService orderService, IStatusChangeService statusChangeService)
        {
            _unitOfWork = unitOfWork;
            _orderService = orderService;
            _statusChangeService = statusChangeService;
        }

        public string vnp_PayUrl = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
        public string vnp_ReturnUrl = "/api/VNPAY/vnpay-payment";
        public string vnp_TmnCode = "SLEKT1ZI";
        public string vnp_HashSecret = "O4W76MM4DOZ95BRUZMX5J5P52ECRLLHC";
        public string vnp_apiUrl = "https://sandbox.vnpayment.vn/merchant_webapi/api/transaction";
        public string vnp_Version = "2.1.0";
        public string vnp_Command = "pay";
        public string vnp_IpAddr = "127.0.0.1";
        public string orderType = "250000";



        public async Task<string> GetTransactionStatusVNPay(string orderId, Guid userId, String urlReturn)
        {
            var order = await _unitOfWork.GetRepository<Order>().Entities
                .Where(o => o.Id == orderId).FirstOrDefaultAsync();

           

            if (order == null || order.UserId != userId) {
                throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), "Order not found");
            }

            if (order.Status == "Processing")
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), "Order's already paid");
            }

            string vnp_TxnRef = GetRandomNumber(8);
            int? money = (int)order.TotalPrice * 100;
            string totalPrice = money.ToString() ?? "000";

            Dictionary<string, string> vnp_Params = new();
            vnp_Params.Add("vnp_Version", vnp_Version);
            vnp_Params.Add("vnp_Command", vnp_Command);
            vnp_Params.Add("vnp_TmnCode", vnp_TmnCode);
            vnp_Params.Add("vnp_Amount", totalPrice);
            vnp_Params.Add("vnp_CurrCode", "VND");

            vnp_Params.Add("vnp_TxnRef", vnp_TxnRef);
            vnp_Params.Add("vnp_OrderInfo", order.Id.ToString());
            vnp_Params.Add("vnp_OrderType", orderType);

            string locate = "vn";
            vnp_Params.Add("vnp_Locale", locate);

            urlReturn += vnp_ReturnUrl;
            vnp_Params.Add("vnp_ReturnUrl", urlReturn);
            vnp_Params.Add("vnp_IpAddr", vnp_IpAddr);


            var formatter = "yyyyMMddHHmmss";
            var now = DateTime.UtcNow.AddHours(7).AddHours(7); // GMT+7
            var vnp_CreateDate = now.ToString(formatter, CultureInfo.InvariantCulture);
            vnp_Params["vnp_CreateDate"] = vnp_CreateDate;

            var expireTime = now.AddMinutes(15);
            var vnp_ExpireDate = expireTime.ToString(formatter, CultureInfo.InvariantCulture);
            vnp_Params["vnp_ExpireDate"] = vnp_ExpireDate;

            var fieldNames = vnp_Params.Keys.ToList();
            fieldNames.Sort();

            var hashData = new StringBuilder();
            var query = new StringBuilder();

           

            foreach (var fieldName in fieldNames)
            {
                var fieldValue = vnp_Params[fieldName];
                if (!string.IsNullOrEmpty(fieldValue))
                {
                    hashData.Append(fieldName)
                            .Append('=')
                            .Append(Uri.EscapeDataString(fieldValue));
                    query.Append(Uri.EscapeDataString(fieldName))
                         .Append('=')
                         .Append(Uri.EscapeDataString(fieldValue));
                    if (fieldNames.IndexOf(fieldName) != fieldNames.Count - 1)
                    {
                        query.Append('&');
                        hashData.Append('&');
                    }
                }
            }

            var queryUrl = query.ToString();
            var vnp_SecureHash = HmacSHA512(vnp_HashSecret, hashData.ToString());
            queryUrl += "&vnp_SecureHash=" + vnp_SecureHash;

        

            var existedPayment = await _unitOfWork.GetRepository<Payment>().Entities
                .Where(p => p.OrderId == orderId).FirstOrDefaultAsync();

            if(existedPayment != null)
            {
                if(existedPayment.ExpirationDate < now) {
                    throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), "Payment Expired");
                }
                
                existedPayment.Status = "Processing";
                await _unitOfWork.GetRepository<Payment>().UpdateAsync(existedPayment);
                await _unitOfWork.SaveAsync();

                order.Status = "Awaiting Payment";
                await _unitOfWork.GetRepository<Order>().UpdateAsync(order);
                await _unitOfWork.SaveAsync();
                return vnp_PayUrl + "?" + queryUrl;
            }

           

            Payment payment = new()
            {
                OrderId = orderId,
                CreatedTime = now,
                ExpirationDate = now.AddHours(24),
                TotalAmount = order.TotalPrice,
                Status = "Processing",
                Method = "VNPay_banking"

            };
            await _unitOfWork.GetRepository<Payment>().InsertAsync(payment);
            await _unitOfWork.SaveAsync();
           

            order.Status = "Awaiting Payment";
            await _unitOfWork.GetRepository<Order>().UpdateAsync(order);
            await _unitOfWork.SaveAsync();

            return vnp_PayUrl + "?" + queryUrl;
        }

        public async Task<VNPAYResponse> VNPAYPayment(VNPAYRequest request)
        {
            VNPAYResponse response = new();

            var fields = new Dictionary<string, string>();

            var totalPrice = request.VnpAmount;
            var bankCode = request.VnpBankCode;
            var bankTranNo = request.VnpBankTranNo;
            var cardType = request.VnpCardType;
            var orderInfo = request.VnpOrderInfo;
            var payDate = request.VnpPayDate;
            var responseCode = request.VnpResponseCode;
            var tmnCode = request.VnpTmnCode;
            var transactionNo = request.VnpTransactionNo;
            var transactionStatus = request.VnpTransactionStatus;
            var vnpSecureHash = request.VnpSecureHash;
            var tnxRef = request.VnpTxnRef;

            var payment = await _unitOfWork.GetRepository<Payment>().Entities
                                  .Where(p => p.OrderId == orderInfo).FirstOrDefaultAsync();

            var order = await _unitOfWork.GetRepository<Order>().Entities
              .Where(o => o.Id == orderInfo).FirstOrDefaultAsync();
            
            if (payment == null)
            {
                throw new BaseException.NotFoundException("not_found", "Payment Not Found");

            }
            if (order == null)
            {
                throw new BaseException.NotFoundException("not_found", "Order Not Found");

            }

            if (response == null) {
                //tao payment detail
                PaymentDetail paymentDetail = new()
                {
                    PaymentId = payment.Id,
                    Status = "Failed",
                    Method = "Transfer",
                    ExternalTransaction = "VNPAY",
                    CreatedTime = DateTime.Now,
                    CreatedBy = "VNPAY"
                };
                await _unitOfWork.GetRepository<PaymentDetail>().InsertAsync(paymentDetail);
                await _unitOfWork.SaveAsync();

                order.Status = "Payment Failed";
                await _unitOfWork.GetRepository<Order>().UpdateAsync(order);
                await _unitOfWork.SaveAsync();

                response = new VNPAYResponse
                {
                    IsSucceed = false,
                    Text = "Payment approve failed"
                };
                return response;
            }


            if (totalPrice == null || !double.TryParse(totalPrice, out _))
            {
                response.IsSucceed = false;
                response.Text = "Order price not found";
                return response;
            }

            if (orderInfo == null)  
            {
                response.IsSucceed = false;
                response.Text = "Order not found";
            }

            var amount = double.Parse(totalPrice) / 100;
            //var returnUrl = $"(url trang web sau khi deploy/{orderInfo}";
            var returnUrl = vnp_ReturnUrl;//dung de test

          


            fields.Add("vnp_Amount", totalPrice ?? string.Empty);
            fields.Add("vnp_BankCode", bankCode ?? string.Empty);
            fields.Add("vnp_BankTranNo", bankTranNo ?? string.Empty);
            fields.Add("vnp_CardType", cardType ?? string.Empty);
            fields.Add("vnp_OrderInfo", orderInfo ?? string.Empty);
            fields.Add("vnp_PayDate", payDate ?? string.Empty);
            fields.Add("vnp_ResponseCode", responseCode ?? string.Empty);
            fields.Add("vnp_TmnCode", tmnCode ?? string.Empty);
            fields.Add("vnp_TransactionNo", transactionNo ?? string.Empty);
            fields.Add("vnp_TransactionStatus", transactionStatus ?? string.Empty);
            fields.Add("vnp_TxnRef", tnxRef ?? string.Empty);



            var signValue = HashAllFields(fields);
            
            if (signValue.Equals(vnpSecureHash))
            {
                if ("00".Equals(request.VnpTransactionStatus))
                {

                    //tao payment detail
                    PaymentDetail paymentDetail = new()
                    {
                        PaymentId = payment.Id,
                        Status = "Success",
                        Method = "Transfer",
                        ExternalTransaction = "VNPAY",
                        CreatedTime = DateTime.Now,
                        CreatedBy = "VNPAY"
                    };
                    await _unitOfWork.GetRepository<PaymentDetail>().InsertAsync(paymentDetail);
                    await _unitOfWork.SaveAsync();

                    payment.Status = "Completed";
                    await _unitOfWork.GetRepository<Payment>().UpdateAsync(payment);
                    await _unitOfWork.SaveAsync();

                    order.Status = "Processing";
                    await _unitOfWork.GetRepository<Order>().UpdateAsync(order);
                    await _unitOfWork.SaveAsync();

                    var statusChangeDto = new StatusChangeForCreationDto
                    {
                        OrderId = order.Id,
                        Status = "Processing"
                    };
                    await _statusChangeService.Create(statusChangeDto, order.UserId.ToString());
                }

                else
                {
                    
                    //tao payment detail
                    PaymentDetail paymentDetail = new()
                    {
                        PaymentId = payment.Id,
                        Status = "Failed",
                        Method = "Transfer",
                        ExternalTransaction = "VNPAY",
                        CreatedTime = DateTime.Now,
                        CreatedBy = "VNPAY"
                    };
                    await _unitOfWork.GetRepository<PaymentDetail>().InsertAsync(paymentDetail);
                    await _unitOfWork.SaveAsync();

                    order.Status = "Payment Failed";
                    await _unitOfWork.GetRepository<Order>().UpdateAsync(order);
                    await _unitOfWork.SaveAsync();

                    response.IsSucceed = false;
                    response.Text = "Payment approve failed";
                    return response;
                }


                response.IsSucceed = true;  
                response.Text = returnUrl;
                return response;

            }
            else
            {
                //tao payment detail
                PaymentDetail paymentDetail = new()
                {
                    PaymentId = payment.Id,
                    Status = "Failed",
                    Method = "Transfer",
                    ExternalTransaction = "VNPAY",
                    CreatedTime = DateTime.Now,
                    CreatedBy = "VNPAY"
                };
                await _unitOfWork.GetRepository<PaymentDetail>().InsertAsync(paymentDetail);
                await _unitOfWork.SaveAsync();

                order.Status = "Payment Failed";
                await _unitOfWork.GetRepository<Order>().UpdateAsync(order);
                await _unitOfWork.SaveAsync();

                response.IsSucceed = false;
                response.Text = "Payment approve failed";
                return response;
            }
        }

        public string HashAllFields(Dictionary<string, string> fields)
            {
                // Sort the field names
                var sortedFieldNames = fields.Keys.OrderBy(k => k).ToList();
                var sb = new StringBuilder();

                foreach (var fieldName in sortedFieldNames)
                {
                    if (fields.TryGetValue(fieldName, out var fieldValue) && !string.IsNullOrEmpty(fieldValue))
                    {
                        if (sb.Length > 0)
                        {
                            sb.Append("&");
                        }
                        sb.Append(fieldName);
                        sb.Append("=");
                        sb.Append(Uri.EscapeDataString(fieldValue));
                    }
                }

                return HmacSHA512(vnp_HashSecret, sb.ToString());
            }





        public static string HmacSHA512(string key, string data)
        {
            if (key == null || data == null)
            {
                throw new ArgumentNullException();
            }

            try
            {
                using (var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key)))
                {
                    byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                    return BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public string GetRandomNumber(int len)
        {
            Random rnd = new();
            const string chars = "0123456789";
            StringBuilder sb = new StringBuilder(len);
            for (int i = 0; i < len; i++)
            {
                sb.Append(chars[rnd.Next(chars.Length)]);
            }
            return sb.ToString();
        }
    }
}
