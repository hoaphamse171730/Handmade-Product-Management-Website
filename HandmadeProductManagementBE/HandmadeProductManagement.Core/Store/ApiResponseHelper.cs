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
using System.Text.Json;
using System.Net;

namespace HandmadeProductManagement.Core.Store;
public class ApiResponseHelper
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApiResponseHelper(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
    {
        var handler = new HttpClientHandler()
        {
            AllowAutoRedirect = false // Disable automatic redirects
        };

        _httpClient = new HttpClient(handler); // Use handler to disable redirect
        _httpContextAccessor = httpContextAccessor;
    }

    private void AddAuthorizationHeader(HttpRequestMessage request)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context != null)
        {
            var token = context.Session.GetString("Token");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Optional: Log the Authorization header for debugging
                // Consider using a logging framework instead of Console.WriteLine
                Console.WriteLine($"Authorization Header: Bearer {token}");
            }
        }
    }


    public async Task<BaseResponse<T>> GetAsync<T>(string url, object queryParams = null)
    {
        if (queryParams != null)
        {
            var query = QueryStringHelper.ToQueryString(queryParams);
            url = string.IsNullOrEmpty(query) ? url : $"{url}&{query}";
        }

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        AddAuthorizationHeader(request);
        Console.WriteLine(request.Headers.ToString());

        var response = await _httpClient.SendAsync(request);

        // Check if the response status is redirect (307 or 301/302)
        if (response.StatusCode == HttpStatusCode.RedirectKeepVerb ||
            response.StatusCode == HttpStatusCode.MovedPermanently ||
            response.StatusCode == HttpStatusCode.Found)
        {
            // Handle redirection manually
            var newUrl = response.Headers.Location.ToString();
            request = new HttpRequestMessage(HttpMethod.Get, newUrl);
            AddAuthorizationHeader(request); // http://localhost:5041 - Header: Authorization Bearer <token> BODY

            response = await _httpClient.SendAsync(request); // https://localhost:7159 - BODY
        }

        return await HandleApiResponse<T>(response);
    }


    public async Task<BaseResponse<T>> PostAsync<T>(string url, object payload)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(payload)
        };
        AddAuthorizationHeader(request);

        var response = await _httpClient.SendAsync(request);

        if (response.StatusCode == HttpStatusCode.RedirectKeepVerb ||
            response.StatusCode == HttpStatusCode.MovedPermanently ||
            response.StatusCode == HttpStatusCode.Found)
        {
            var newUrl = response.Headers.Location.ToString();
            request = new HttpRequestMessage(HttpMethod.Post, newUrl)
            {
                Content = JsonContent.Create(payload)
            };
            AddAuthorizationHeader(request);

            response = await _httpClient.SendAsync(request);
        }

        return await HandleApiResponse<T>(response);
    }

    public async Task<BaseResponse<T>> PutAsync<T>(string url, object payload)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, url)
        {
            Content = JsonContent.Create(payload)
        };
        AddAuthorizationHeader(request);

        var response = await _httpClient.SendAsync(request);

        if (response.StatusCode == HttpStatusCode.RedirectKeepVerb ||
            response.StatusCode == HttpStatusCode.MovedPermanently ||
            response.StatusCode == HttpStatusCode.Found)
        {
            var newUrl = response.Headers.Location.ToString();
            request = new HttpRequestMessage(HttpMethod.Put, newUrl)
            {
                Content = JsonContent.Create(payload)
            };
            AddAuthorizationHeader(request);

            response = await _httpClient.SendAsync(request);
        }

        return await HandleApiResponse<T>(response);
    }

    public async Task<BaseResponse<T>> DeleteAsync<T>(string url)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, url);
        AddAuthorizationHeader(request);

        var response = await _httpClient.SendAsync(request);

        if (response.StatusCode == HttpStatusCode.RedirectKeepVerb ||
            response.StatusCode == HttpStatusCode.MovedPermanently ||
            response.StatusCode == HttpStatusCode.Found)
        {
            var newUrl = response.Headers.Location.ToString();
            request = new HttpRequestMessage(HttpMethod.Delete, newUrl);
            AddAuthorizationHeader(request);

            response = await _httpClient.SendAsync(request);
        }

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
            return JsonSerializer.Deserialize<ProblemDetails>(content);
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
            return JsonSerializer.Deserialize<BaseResponse<string>>(content);
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

