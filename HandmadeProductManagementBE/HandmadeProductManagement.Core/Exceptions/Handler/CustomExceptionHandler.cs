using FluentValidation;
using HandmadeProductManagement.Core.Base;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HandmadeProductManagement.Core.Exceptions.Handler
{
    public class CustomExceptionHandler(ILogger<CustomExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            logger.LogError("Error Message: {exceptionMessage}, Time of occurrence: {time}",
                exception.Message, DateTime.UtcNow);

            (string Detail, string Title, int StatusCode) details = exception switch
            {
                ValidationException validationException => (
                    validationException.Message,
                    validationException.GetType().Name,
                    context.Response.StatusCode = StatusCodes.Status400BadRequest
                ),

                BaseException.BadRequestException badRequestException => (
                    badRequestException.ErrorDetail.ErrorMessage?.ToString() ?? exception.Message,
                    badRequestException.GetType().Name,
                    context.Response.StatusCode = StatusCodes.Status400BadRequest
                ),

                BaseException.NotFoundException notFoundException => (
                    notFoundException.ErrorDetail.ErrorMessage?.ToString() ?? exception.Message,
                    notFoundException.GetType().Name,
                    context.Response.StatusCode = StatusCodes.Status404NotFound
                ),

                UnauthorizedAccessException unauthorizedAccessException => (
                    "Access is denied due to invalid credentials.",
                    unauthorizedAccessException.GetType().Name,
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized
                ),

                BaseException.ForbiddenException forbiddenException => (
                    forbiddenException.ErrorDetail.ErrorMessage?.ToString() ?? "You do not have permission to perform this action.",
                    forbiddenException.GetType().Name,
                    context.Response.StatusCode = StatusCodes.Status403Forbidden
                ),

                _ => (
                    exception.Message,
                    exception.GetType().Name,
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError
                )
            };

            var problemDetails = new ProblemDetails()
            {
                Title = details.Title,
                Detail = details.Detail,
                Status = details.StatusCode,
                Instance = context.Request.Path
            };

            problemDetails.Extensions.Add("traceId", context.TraceIdentifier);

            if (exception is ValidationException vEx)
            {
                problemDetails.Extensions.Add("ValidationErrors", vEx.Errors);
            }

            await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            return true;
        }
    }
}
