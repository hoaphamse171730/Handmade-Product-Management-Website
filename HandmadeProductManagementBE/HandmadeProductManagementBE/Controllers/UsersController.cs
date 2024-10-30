
using Microsoft.AspNetCore.Mvc;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.ModelViews.UserModelViews;
using System.Security.Claims;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.NotificationModelViews;
using Microsoft.AspNetCore.Authorization;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Services.Service;
namespace HandmadeProductManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        public UsersController(IUserService userService) => _userService = userService;
        

        // GET: api/Users
        [HttpGet]
        //[Authorize(Roles ="Admin")]
        public async Task<IActionResult> GetApplicationUsers(int PageNumber, int PageSize,[FromQuery] string? userName, [FromQuery] string? phoneNumber)
        {

            var response = new BaseResponse<IList<UserResponseModel>>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Success",
                Data = await _userService.GetAll(PageNumber,PageSize, userName, phoneNumber)
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
        public async Task<IActionResult> UpdateUser(string id, UpdateUserDTO updateUserDTO)
        {
            var updateResult = await _userService.UpdateUser(id, updateUserDTO);
            var response = new BaseResponse<bool>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Success",
                Data = updateResult
            };
            return Ok(response);
        }



        [HttpDelete("{id}")]
        //[Authorize(Roles = "Admin")]

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

        [Authorize(Roles = "Admin")]
        [HttpGet("inactive")]
        public async Task<IActionResult> GetInactiveUsers()
        {
            var response = new BaseResponse<IList<UserResponseModel>>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Get all inactive users successfully!",
                Data = await _userService.GetInactiveUsers()
            };
            return Ok(response);
        }

        [HttpPost("{id}/restore")]
        [Authorize(Roles = "Admin")]

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

        [Authorize(Roles = "Admin, Seller")]
        [HttpGet("notification_review")]
        public async Task<IActionResult> GetNotifications()
        {
            var id = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
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


        [Authorize(Roles = "Admin, Customer, Seller")]
        [HttpGet("notification_statuschange")]
        public async Task<IActionResult> GetNewStatusChangeNotification()
        {
            var id = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

            var response = new BaseResponse<IList<NotificationModel>>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Data = await _userService.GetNewStatusChangeNotificationList(id),
                Message = "Success",
            };
            return Ok(response);
        }

        [HttpGet("notification/new-order")]
        [Authorize]
        public async Task<IActionResult> GetNewOrderNotifications()
        {
            // Lấy thông tin người dùng từ token
            var userIdFromToken = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty; // Giả sử NameIdentifier là claim cho userId
            //var userFullNameFromToken = User.FindFirstValue(ClaimTypes.Name); // Giả sử Name là claim cho tên đầy đủ của người dùng

            // Lấy danh sách thông báo đơn hàng mới
            var orderNotifications = await _userService.GetNewOrderNotificationList(userIdFromToken);

            var response = new BaseResponse<IList<NotificationModel>>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Data = orderNotifications,
                Message = "Thành công",
            };

            return Ok(response);
        }

        [HttpGet("notification/new-reply")]
        [Authorize]
        public async Task<IActionResult> GetNewReplyNotifications()
        {
            // Lấy thông tin người dùng từ token
            var userIdFromToken = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty; // Giả sử NameIdentifier là claim cho userId

            // Lấy danh sách thông báo phản hồi mới
            var replyNotifications = await _userService.GetNewReplyNotificationList(userIdFromToken);

            var response = new BaseResponse<IList<NotificationModel>>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Data = replyNotifications,
                Message = "Thành công",
            };

            return Ok(response);
        }

        [HttpGet("notification/payments")]
        [Authorize]
        public async Task<IActionResult> GetPaymentExpirationNotifications()
        {
            // Lấy thông tin người dùng từ token
            var userIdFromToken = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new BaseException.NotFoundException("not_found", "user not found");
            // Lấy danh sách thông báo thanh toán sắp hết hạn
            var paymentNotifications = await _userService.NotificationForPaymentExpiration(userIdFromToken);

            var response = new BaseResponse<IList<NotificationModel>>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Data = paymentNotifications,
                Message = "Thành công",
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
