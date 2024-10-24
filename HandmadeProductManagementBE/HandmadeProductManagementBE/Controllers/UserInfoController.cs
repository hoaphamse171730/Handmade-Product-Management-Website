using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.UserInfoModelViews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HandmadeProductManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserInfoController : ControllerBase
    {
        private readonly IUserInfoService _userInfoService;
        public UserInfoController(IUserInfoService userInfoService)
        {
            _userInfoService = userInfoService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserInfoById()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var response = new BaseResponse<UserInfoDto>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Success",
                Data = await _userInfoService.GetUserInfoByIdAsync(userId)
            };
            return Ok(response);
        }

        // PATCH api/userinfo
        [HttpPatch]
        [Authorize]
        public async Task<IActionResult> PatchUserInfo([FromForm]UserInfoUpdateRequest request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var updatedUserInfo = await _userInfoService.PatchUserInfoAsync(userId, request);
            var response = new BaseResponse<bool>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "User info updated successfully.",
                Data = updatedUserInfo
            };

            return Ok(response);
        }
    }
}
