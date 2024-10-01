using System.Diagnostics;

namespace HandmadeProductManagementAPI.Middlewares;

public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        logger.LogInformation("[START] Request: {Method} {Path}", context.Request.Method, context.Request.Path);

        var stopwatch = Stopwatch.StartNew();

        await next(context);

        stopwatch.Stop();
        var timeTaken = stopwatch.Elapsed;
        logger.LogInformation("[END] Request: {Method} {Path} took {ElapsedMilliseconds} ms", 
            context.Request.Method, context.Request.Path, timeTaken.TotalMilliseconds);

        if (timeTaken.Seconds > 3)
        {
            logger.LogWarning("[PERFORMANCE] The request {Method} {Path} took {TimeTaken} seconds", 
                context.Request.Method, context.Request.Path, timeTaken.TotalSeconds);
        }
    }
}
