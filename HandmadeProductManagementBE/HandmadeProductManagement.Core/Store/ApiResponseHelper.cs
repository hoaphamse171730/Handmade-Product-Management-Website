using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Exceptions.Handler; // Adjust this based on your actual namespace
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace HandmadeProductManagement.Core.Store;
public class ApiResponseHelper
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApiResponseHelper(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
    }
    // Add JWT Authorization header to the HttpClient
    private void AddAuthorizationHeader()
    {
        var token = _httpContextAccessor.HttpContext?.Session.GetString("JwtToken");
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    // Generic method to handle GET requests
    public async Task<BaseResponse<T>> GetAsync<T>(string url)
    {
        AddAuthorizationHeader();
        var response = await _httpClient.GetAsync(url);
        return await HandleApiResponse<T>(response);
    }

    // Generic method to handle POST requests
    public async Task<BaseResponse<T>> PostAsync<T>(string url, object payload)
    {
        AddAuthorizationHeader();

        var response = await _httpClient.PostAsJsonAsync(url, payload);
        return await HandleApiResponse<T>(response);
    }

    // Generic method to handle PUT requests
    public async Task<BaseResponse<T>> PutAsync<T>(string url, object payload)
    {
        AddAuthorizationHeader();
        var response = await _httpClient.PutAsJsonAsync(url, payload);
        return await HandleApiResponse<T>(response);
    }

    // Generic method to handle DELETE requests
    public async Task<BaseResponse<T>> DeleteAsync<T>(string url)
    {
        AddAuthorizationHeader();
        var response = await _httpClient.DeleteAsync(url);
        return await HandleApiResponse<T>(response);
    }

    // Centralized method to handle API response and exceptions
    private async Task<BaseResponse<T>> HandleApiResponse<T>(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            // Deserialize the successful response into the BaseResponse<T>
            var baseResponse = await response.Content.ReadFromJsonAsync<BaseResponse<T>>();
            return baseResponse ?? new BaseResponse<T>();
        }
        else
        {
            // Read the error content
            var content = await response.Content.ReadAsStringAsync();

            // Attempt to deserialize the response as ProblemDetails (middleware-thrown exceptions)
            var problemDetails = TryDeserializeProblemDetails(content);
            if (problemDetails != null)
            {
                HandleProblemDetailsExceptions(problemDetails);
            }

            // Attempt to deserialize the response as BaseResponse<string>
            var baseResponse = TryDeserializeBaseResponse(content);
            if (baseResponse != null)
            {
                // Handle based on StatusCodeHelper
                throw MapToCustomException(baseResponse);
            }

            // If nothing matches, throw a generic exception
            throw new HttpRequestException($"Request failed with status code {response.StatusCode} and content: {content}");
        }
    }

    private ProblemDetails? TryDeserializeProblemDetails(string content)
    {
        try
        {
            return JsonConvert.DeserializeObject<ProblemDetails>(content);
        }
        catch
        {
            return null;
        }
    }

    private BaseResponse<string>? TryDeserializeBaseResponse(string content)
    {
        try
        {
            return JsonConvert.DeserializeObject<BaseResponse<string>>(content);
        }
        catch
        {
            return null;
        }
    }

    private void HandleProblemDetailsExceptions(ProblemDetails problemDetails)
    {
        // Map ProblemDetails to specific exceptions
        switch (problemDetails.Status)
        {
            case StatusCodes.Status400BadRequest:
                throw new BaseException.BadRequestException("bad_request", problemDetails.Detail ?? "Bad Request");

            case StatusCodes.Status401Unauthorized:
                throw new BaseException.UnauthorizedException("unauthorized", problemDetails.Detail ?? "Unauthorized");

            case StatusCodes.Status403Forbidden:
                throw new BaseException.ForbiddenException("forbidden", problemDetails.Detail ?? "Forbidden");

            case StatusCodes.Status404NotFound:
                throw new BaseException.NotFoundException("not_found", problemDetails.Detail ?? "Not Found");

            default:
                throw new BaseException.CoreException("error", problemDetails.Detail ?? "Internal Server Error", problemDetails.Status ?? 500);
        }
    }

    private Exception MapToCustomException(BaseResponse<string> baseResponse)
    {
        switch (baseResponse.StatusCode)
        {
            case StatusCodeHelper.BadRequest:
                return new BaseException.BadRequestException(baseResponse.Code ?? "bad_request", baseResponse.Message ?? "Bad Request");

            case StatusCodeHelper.NotFound:
                return new BaseException.NotFoundException(baseResponse.Code ?? "not_found", baseResponse.Message ?? "Not Found");

            case StatusCodeHelper.Unauthorized:
                return new BaseException.UnauthorizedException(baseResponse.Code ?? "unauthorized", baseResponse.Message ?? "Unauthorized");

            case StatusCodeHelper.Forbidden:
                return new BaseException.ForbiddenException(baseResponse.Code ?? "forbidden", baseResponse.Message ?? "Forbidden");

            default:
                return new BaseException.CoreException(baseResponse.Code ?? "error", baseResponse.Message ?? "Internal Server Error", (int)baseResponse.StatusCode);
        }
    }
}

