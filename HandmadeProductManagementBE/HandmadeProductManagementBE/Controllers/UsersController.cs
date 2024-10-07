
using Microsoft.AspNetCore.Mvc;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.ModelViews.UserModelViews;
using HandmadeProductManagement.Core.Constants;
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
