using HandmadeProductManagement.Contract.Services.Interface;
using Microsoft.AspNetCore.Http;

namespace HandmadeProductManagement.Services.Service;

public class UserAgentService(IHttpContextAccessor contextAccessor) : IUserAgentService
{
    public (string operatingSystem, string browser) GetClientInfo()
    {
        var userAgent = contextAccessor.HttpContext!.Request.Headers["User-Agent"].ToString();
        string operatingSystem = GetOperatingSystem(userAgent);
        string browser = GetBrowserName(userAgent);
        return (operatingSystem, browser);
    }

    private string GetOperatingSystem(string userAgent)
    {
        if (userAgent.Contains("Windows")) return "Windows";
        if (userAgent.Contains("Macintosh")) return "Mac OS";
        if (userAgent.Contains("Linux")) return "Linux";
        if (userAgent.Contains("Android")) return "Android";
        if (userAgent.Contains("iPhone")) return "iOS";
        return "Unknown OS";
    }

    private string GetBrowserName(string userAgent)
    {
        if (userAgent.Contains("Firefox")) return "Firefox";
        if (userAgent.Contains("Chrome") && !userAgent.Contains("Chromium")) return "Chrome";
        if (userAgent.Contains("Safari") && !userAgent.Contains("Chrome")) return "Safari";
        if (userAgent.Contains("Edge")) return "Edge";
        if (userAgent.Contains("Opera") || userAgent.Contains("OPR")) return "Opera";
        return "Unknown Browser";
    }
}