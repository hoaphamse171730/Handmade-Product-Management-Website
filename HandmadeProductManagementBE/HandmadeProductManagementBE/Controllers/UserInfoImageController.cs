using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.UserInfoImageModelViews;
using HandmadeProductManagement.Services.Service;
using Microsoft.AspNetCore.Mvc;

namespace HandmadeProductManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserInfoImageController : Controller
    {
        private readonly IUserInfoImageService _userInfoImageService;

        public UserInfoImageController(IUserInfoImageService userInfoImageService)
        {
            _userInfoImageService = userInfoImageService;
        }

        [HttpPost("Upload")]

        public async Task<IActionResult> UploadUserInfoImage(IFormFile file, string UserInfoId)
        {
            var response = new BaseResponse<bool>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Success",
                Data = await _userInfoImageService.UploadUserInfoImage(file, UserInfoId)
            };
            return Ok(response);
        }

        [HttpDelete("Delete")]
        public async Task<IActionResult> DeleteUserInfoImage(string imageId)
        {
            var response = new BaseResponse<bool>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Success",
                Data = await _userInfoImageService.DeleteUserInfoImage(imageId)
            };
            return Ok(response);

        }

        [HttpGet("GetImage")]
        public async Task<IActionResult> GetUserInfoImage(string UserInfoId)
        {
            var response = new BaseResponse<IList<userinfoimage>>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Success",
                Data = await _userInfoImageService.GetUserInfoImageById(UserInfoId)
            };
            return Ok(response);

        }
    }
}
