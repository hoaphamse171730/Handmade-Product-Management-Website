
using Microsoft.AspNetCore.Mvc;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.ModelViews.UserModelViews;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.NotificationModelViews;
namespace HandmadeProductManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<IActionResult> GetApplicationUsers()
        {

            var response = new BaseResponse<IList<UserResponseModel>>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Success",
                Data = await _userService.GetAll()
            };
            return Ok(response);


        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetApplicationUsersById(String id)
        {
            var response = new BaseResponse<UserResponseByIdModel>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Success",
                Data = await _userService.GetById(id)
        };
            return Ok(response);


        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id,  UpdateUserDTO updateUserDTO)
        {
            /*


            if (!new EmailAddressAttribute().IsValid(updateUserDTO.Email))
            {
                return StatusCode(400, BaseResponse<string>.FailResponse("Email is not valid"));
            }

            
            var phoneRegex = new Regex(@"^\d{10}$");  
            if (!phoneRegex.IsMatch(updateUserDTO.PhoneNumber))
            {
                return StatusCode(400, BaseResponse<string>.FailResponse("Phone number is not valid"));
            }


            }*/
            var response = new BaseResponse<UpdateUserResponseModel>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Success",
                Data = await _userService.UpdateUser(id, updateUserDTO)
            };
            return Ok(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {

            var response = new BaseResponse<bool>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Success",
                Data = await _userService.DeleteUser(id)
            };
            return Ok(response);
        }

        [HttpPost("{id}/restore")] 
        public async Task<IActionResult> ReverseDeleteUser(string id)
        {

            var response = new BaseResponse<bool>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Success",
                Data = await _userService.ReverseDeleteUser(id)
            };
            return Ok(response);
        }

        [HttpGet("{id}/notification_Review")]
        public async Task<IActionResult> GetNotifications(string id)
        {
            var notifications = await _userService.GetNewReviewNotificationList(id);

            var response = new BaseResponse<IList<NotificationModel>>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Data = notifications,
                Message = "Success",
            };

            // Kiểm tra xem notifications có dữ liệu hay không
            if (notifications != null && notifications.Any())
            {
                response.Data = notifications; // Thêm dữ liệu vào phản hồi nếu có
            }
            else
            {
                response.Message = "No new reviews available"; // Thay đổi thông điệp nếu không có dữ liệu
            }         
            return Ok(response);
        }

        [HttpGet("{id}/notification_statuschange")]
        public async Task<IActionResult> GetNewStatusChangeNotification(string id)
        {
            var response = new BaseResponse<IList<NotificationModel>>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Data = await _userService.GetNewStatusChangeNotificationList(id),
                Message = "Success",
            };
            return Ok(response);
        }

        [HttpGet("{userId}/notification/new-order")]
        public async Task<IActionResult> GetNewOrderNotifications(string userId)
        {
            var orderNotifications = await _userService.GetNewOrderNotificationList(userId);

            var response = new BaseResponse<IList<NotificationModel>>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Data = orderNotifications,
                Message = "Success",
            };

            return Ok(response);
        }

        [HttpGet("{userId}/notification/new-reply")]
        public async Task<IActionResult> GetNewReplyNotifications(string userId)
        {
            var replyNotifications = await _userService.GetNewReplyNotificationList(userId);

            var response = new BaseResponse<IList<NotificationModel>>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Data = replyNotifications,
                Message = "Success",
            };

            return Ok(response);
        }


        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPost]
        //public async Task<ActionResult<ApplicationUser>> PostApplicationUser(ApplicationUser applicationUser)
        //{
        //    _userService.Add(applicationUser);
        //    await _context.SaveChangesAsync();

        //    return CreatedAtAction("GetApplicationUser", new { id = applicationUser.Id }, applicationUser);
        //}

        //// DELETE: api/Users/5
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteApplicationUser(Guid id)
        //{
        //    var applicationUser = await _context.ApplicationUsers.FindAsync(id);
        //    if (applicationUser == null)
        //    {
        //        return NotFound();
        //    }

        //    _context.ApplicationUsers.Remove(applicationUser);
        //    await _context.SaveChangesAsync();

        //    return NoContent();
        //}


    }
}
