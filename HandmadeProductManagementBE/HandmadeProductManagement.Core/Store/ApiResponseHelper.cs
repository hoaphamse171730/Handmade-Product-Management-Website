using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Net;
using Microsoft.Extensions.Logging;

namespace HandmadeProductManagement.Core.Store;
public class ApiResponseHelper
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ILogger<ApiResponseHelper> _logger;

    public ApiResponseHelper(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ILogger<ApiResponseHelper> logger)
    {
        var handler = new HttpClientHandler()
        {
            AllowAutoRedirect = false // Disable automatic redirects
        };
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        _httpClient = new HttpClient(handler); // Use handler to disable redirect
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
            }
        }
    }
    public async Task<BaseResponse<bool>> UploadImageAsync(string url, IFormFile file, string productId)
    {
        try
        {
            using var client = new HttpClient();
            using var formData = new MultipartFormDataContent();
            using var streamContent = new StreamContent(file.OpenReadStream());

            formData.Add(streamContent, "file", file.FileName);
            var response = await client.PostAsync($"{url}?productId={productId}", formData);
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<BaseResponse<bool>>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            return new BaseResponse<bool>
            {
                Code = StatusCodeHelper.ServerError.ToString(),
                StatusCode = StatusCodeHelper.ServerError,
                Message = ex.Message,
                Data = false
            };
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

    public async Task<BaseResponse<T>> PutAsync<T>(string url, object payload = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, url);

        if (payload != null)
        {
            request.Content = JsonContent.Create(payload);
        }
        AddAuthorizationHeader(request);

        var response = await _httpClient.SendAsync(request);

        if (response.StatusCode == HttpStatusCode.RedirectKeepVerb ||
            response.StatusCode == HttpStatusCode.MovedPermanently ||
            response.StatusCode == HttpStatusCode.Found)
        {
            var newUrl = response.Headers.Location.ToString();
            request = new HttpRequestMessage(HttpMethod.Put, newUrl);

            if (payload != null)
            {
                request.Content = JsonContent.Create(payload);
            }
            AddAuthorizationHeader(request);

            response = await _httpClient.SendAsync(request);
        }

        return await HandleApiResponse<T>(response);
    }

    public async Task<BaseResponse<T>> PatchAsync<T>(string url, object payload)
    {
        var request = new HttpRequestMessage(HttpMethod.Patch, url)
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
            request = new HttpRequestMessage(HttpMethod.Patch, newUrl)
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

    public async Task<BaseResponse<T>> PostMultipartAsync<T>(string url, MultipartFormDataContent content)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = content
        };
        AddAuthorizationHeader(request);

        var response = await _httpClient.SendAsync(request);
        return await HandleApiResponse<T>(response);
    }

    public async Task<BaseResponse<bool>> PutProductStatusUpdateAsync(string url, bool isAvailable)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, url)
        {
            Content = JsonContent.Create(new { IsAvailable = isAvailable })
        };
        AddAuthorizationHeader(request);

        var response = await _httpClient.SendAsync(request);
        return await HandleProductStatusResponse(response);
    }

    private async Task<BaseResponse<bool>> HandleProductStatusResponse(HttpResponseMessage response)
    {
        var responseBody = await response.Content.ReadAsStringAsync();
        if (response.IsSuccessStatusCode)
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<BaseResponse<bool>>(responseBody, options) ?? new BaseResponse<bool>();
        }
        else
        {
            // Handle non-success status codes with an error message
            return new BaseResponse<bool>
            {
                StatusCode = StatusCodeHelper.ServerError,
                Code = ((int)response.StatusCode).ToString(),
                Message = $"Failed to update product status. Status code: {response.StatusCode}"
            };
        }
    }


    // Centralized method to handle API response and exceptions
    private async Task<BaseResponse<T>> HandleApiResponse<T>(HttpResponseMessage response)
    {
        Console.WriteLine(response);
        string responseContent = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            // Attempt to deserialize the successful response
            try
            {
                var successResponse = JsonSerializer.Deserialize<BaseResponse<T>>(responseContent, _jsonOptions);
                return successResponse ?? new BaseResponse<T>();
            }
            catch (JsonException ex)
            {
                // Log or handle deserialization errors as needed
                throw new HttpRequestException("Failed to deserialize the successful response.", ex);
            }
        }
        else
        {
            if (string.IsNullOrWhiteSpace(responseContent))
            {
                HandleEmptyErrorResponse(response.StatusCode);
            }

            // Attempt to deserialize the response as ProblemDetails
            var problemDetails = DeserializeProblemDetails(responseContent);
            if (problemDetails != null)
            {
                HandleProblemDetailsExceptions(problemDetails);
                // The above method will throw an exception, so the following code won't execute
            }

            // Attempt to deserialize the response as BaseResponse<string>
            var baseResponse = DeserializeBaseResponse<string>(responseContent);
            if (baseResponse != null)
            {
                // Map to a custom exception based on the BaseResponse
                throw MapToCustomException(baseResponse);
            }

            // If deserialization did not match any known formats, throw a generic exception
            throw new HttpRequestException($"Request failed with status code {response.StatusCode} and content: {responseContent}");
        }
    }
    private void HandleEmptyErrorResponse(HttpStatusCode statusCode)
    {
        switch (statusCode)
        {
            case HttpStatusCode.BadRequest:
                throw new BaseException.BadRequestException("bad_request", "Bad Request");

            case HttpStatusCode.Unauthorized:
                throw new BaseException.UnauthorizedException("unauthorized", "Unauthorized");

            case HttpStatusCode.Forbidden:
                throw new BaseException.ForbiddenException("forbidden", "Forbidden");

            case HttpStatusCode.NotFound:
                throw new BaseException.NotFoundException("not_found", "Not Found");

            default:
                throw new BaseException.CoreException("error", $"Error: response status is {(int)statusCode}", (int)statusCode);
        }
    }


    private ProblemDetails? DeserializeProblemDetails(string content)
    {
        try
        {
            return JsonSerializer.Deserialize<ProblemDetails>(content, _jsonOptions);
        }
        catch (JsonException)
        {
            // Log the exception if necessary
            return null;
        }
    }


    private BaseResponse<T>? DeserializeBaseResponse<T>(string content)
    {
        try
        {
            return JsonSerializer.Deserialize<BaseResponse<T>>(content, _jsonOptions);
        }
        catch (JsonException)
        {
            // Log the exception if necessary
            return null;
        }
    }


    private void HandleProblemDetailsExceptions(ProblemDetails problemDetails)
    {
        // Ensure that the Status property is set; default to 500 if not
        int status = problemDetails.Status ?? (int)HttpStatusCode.InternalServerError;
        string detail = problemDetails.Detail ?? "An error occurred.";

        switch (status)
        {
            case StatusCodes.Status400BadRequest:
                throw new BaseException.BadRequestException(problemDetails.Title ?? "bad_request", detail);

            case StatusCodes.Status401Unauthorized:
                throw new BaseException.UnauthorizedException(problemDetails.Title ?? "unauthorized", detail);

            case StatusCodes.Status403Forbidden:
                throw new BaseException.ForbiddenException(problemDetails.Title ?? "forbidden", detail);

            case StatusCodes.Status404NotFound:
                throw new BaseException.NotFoundException(problemDetails.Title ?? "not_found", detail);

            default:
                throw new BaseException.CoreException(problemDetails.Title ?? "error", detail, status);
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

