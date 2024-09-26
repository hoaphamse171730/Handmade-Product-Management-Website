using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HandmadeProductManagement.Repositories.Context;
using HandmadeProductManagement.Repositories.Entity;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.ModelViews.UserModelViews;
using System.Security.Claims;

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
        public async Task<ActionResult<IEnumerable<ApplicationUser>>> GetApplicationUsers()
        {
            IList<UserResponseModel> a = await _userService.GetAll();
            return Ok(BaseResponse<IList<UserResponseModel>>.OkResponse(a));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponseByIdModel>> GetApplicationUsersById(String id)
        {
            UserResponseByIdModel userResponse = await _userService.GetById(id);

            if (userResponse == null)
            {
                return NotFound("User not found.");
            }

            // Return a 200 OK response with the user data
            return Ok(BaseResponse<UserResponseByIdModel>.OkResponse(userResponse));
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id,  UpdateUserDTO updateUserDTO)
        {
            if (id == null || updateUserDTO == null)
            {
                return BadRequest("Invalid data.");
            }

            var updatedUser = await _userService.UpdateUser(id, updateUserDTO);

            if (updatedUser == null)
            {
                return NotFound("User not found.");
            }

            // Return the updated user in the response
            return Ok(BaseResponse<UpdateUserResponseModel>.OkResponse(updatedUser));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            // Assume you have a way to get the current user's ID or username
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Adjust as needed

            var result = await _userService.DeleteUser(id);

            if (!result)
            {
                return NotFound(new { Message = "User not found." });
            }

            return Ok(BaseResponse<UpdateUserResponseModel>.OkResponse("Deleted successfuly")); // Return a 204 No Content response on successful deletion
        }

        [HttpPost("{id}/restore")] // Assuming you want to use a POST request to restore
        public async Task<IActionResult> ReverseDeleteUser(string id)
        {
            var result = await _userService.ReverseDeleteUser(id);

            if (!result)
            {
                return NotFound(new { Message = "User not found or already active." });
            }

            return Ok(BaseResponse<UpdateUserResponseModel>.OkResponse(" Undo deleted successfuly"));
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
