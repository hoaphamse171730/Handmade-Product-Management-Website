using GraphQLParser;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Store;
using HandmadeProductManagement.ModelViews.ProductModelViews;
using HandmadeProductManagement.ModelViews.UserModelViews;
using HotChocolate.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace UI.Pages.UserInfo
{

    public class UserInfoModel : PageModel
    {
        private readonly ApiResponseHelper _apiResponseHelper;
        private readonly IHttpClientFactory _httpClientFactory;


        public UserInfoModel(ApiResponseHelper apiResponseHelper, IHttpClientFactory httpClientFactory)
        {
            _apiResponseHelper = apiResponseHelper ?? throw new ArgumentNullException(nameof(apiResponseHelper));
            _httpClientFactory = httpClientFactory;
        }

        public UserResponseByIdModel userInfo { get; set; }
        public void OnGet()
        {
            string token = HttpContext.Session.GetString("Token");
            if (!string.IsNullOrEmpty(token))
            {
                string userId = GetUserIdFromToken(token);
                if (!string.IsNullOrEmpty(userId))
                {
                    userInfo = GetUserResponseById(userId);
                }
            }
        }

        private string GetUserIdFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "nameid");
            return userIdClaim?.Value;
        }
        private UserResponseByIdModel GetUserResponseById(string id)
        {
            var response = _apiResponseHelper.GetAsync<UserResponseByIdModel>(Constants.ApiBaseUrl + $"/api/users/{id}").Result;
            if (response.StatusCode == StatusCodeHelper.OK && response.Data != null)
            {
                return response.Data;
            }
            return new UserResponseByIdModel();
        }
    }
}
