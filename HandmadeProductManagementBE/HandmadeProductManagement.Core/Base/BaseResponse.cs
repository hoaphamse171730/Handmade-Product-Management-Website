using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Utils;

namespace HandmadeProductManagement.Core.Base
{
    public class BaseResponse<T>
    {
        public T? Data { get; set; }
        public string? Message { get; set; }
        public StatusCodeHelper StatusCode { get; set; }
        public string? Code { get; set; }

        public BaseResponse()
        {
            
        }
        public BaseResponse(StatusCodeHelper statusCode, string code, T? data, string? message)
        {
            Data = data;
            Message = message;
            StatusCode = statusCode;
            Code = code;
        }

        public BaseResponse(StatusCodeHelper statusCode, string code, T? data)
        {
            Data = data;
            StatusCode = statusCode;
            Code = code;
        }

        public BaseResponse(StatusCodeHelper statusCode, string code, string? message)
        {
            Message = message;
            StatusCode = statusCode;
            Code = code;
        }

        public static BaseResponse<T> OkResponse(T? data)
        {
            return new BaseResponse<T>(StatusCodeHelper.OK, StatusCodeHelper.OK.Name(), data);
        }
        public static BaseResponse<T> OkResponse(string? mess)
        {
            return new BaseResponse<T>(StatusCodeHelper.OK, StatusCodeHelper.OK.Name(), mess);
        }
        public static BaseResponse<T> FailResponse(string? message, StatusCodeHelper statusCode = StatusCodeHelper.BadRequest)
        {
            return new BaseResponse<T>(statusCode, statusCode.Name(), message);
        }

        public static BaseResponse<T> FailResponse(string? message, string code, StatusCodeHelper statusCode)
        {
            return new BaseResponse<T>(statusCode, code, message);
        }

        public static implicit operator BaseResponse<T>(BaseResponse<bool> v)
        {
            throw new NotImplementedException();
        }
    }
}
