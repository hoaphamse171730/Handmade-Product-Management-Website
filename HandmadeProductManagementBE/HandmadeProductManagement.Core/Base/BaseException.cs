using HandmadeProductManagement.Core.Constants;

namespace HandmadeProductManagement.Core.Base
{
    public class BaseException : Exception
    {
        public class CoreException : Exception
        {
            public CoreException(string code, string message, int statusCode = (int)StatusCodeHelper.ServerError)
                : base(message)
            {
                Code = code;
                StatusCode = statusCode;
            }


            public string Code { get; }

            public int StatusCode { get; set; }

            public Dictionary<string, object>? AdditionalData { get; set; }
        }

        public class BadRequestException : ErrorException
        {
            public BadRequestException(string errorCode, string message)
                : base(((int)StatusCodeHelper.BadRequest), errorCode, message)
            {
            }

            public BadRequestException(ICollection<KeyValuePair<string, ICollection<string>>> errors)
                : base(((int)StatusCodeHelper.BadRequest), new ErrorDetail
                {
                    ErrorCode = "bad_request",
                    ErrorMessage = errors
                })
            {
            }
        }

        public class NotFoundException : ErrorException
        {
            public NotFoundException(string errorCode, string message) : base(((int)StatusCodeHelper.NotFound), errorCode, message)
            {
            }

            public NotFoundException(ICollection<KeyValuePair<string, ICollection<string>>> errors) : base(((int)StatusCodeHelper.NotFound), new ErrorDetail
            {
                ErrorCode = "not_found",
                ErrorMessage = errors
            })
            {
            }
        }
        public class UnauthorizedException : ErrorException
        {
            public UnauthorizedException(string errorCode, string message)
                : base(((int)StatusCodeHelper.Unauthorized), errorCode, message)
            {
            }
        }

        public class ForbiddenException : ErrorException
        {
            public ForbiddenException(string errorCode, string message)
                : base(((int)StatusCodeHelper.Forbidden), errorCode, message)
            {
            }
        }

        public class ErrorException : Exception
        {
            public int StatusCode { get; }

            public ErrorDetail ErrorDetail { get; }

            public ErrorException(int statusCode, string errorCode, string message)
            {
                StatusCode = statusCode;
                ErrorDetail = new ErrorDetail
                {
                    ErrorCode = errorCode,
                    ErrorMessage = message
                };
            }

            public ErrorException(int statusCode, ErrorDetail errorDetail)
            {
                StatusCode = statusCode;
                ErrorDetail = errorDetail;
            }
        }

        public class ErrorDetail
        {
            public string? ErrorCode { get; set; }

            public object? ErrorMessage { get; set; }
        }
    }
}