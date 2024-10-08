using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.ModelViews.CategoryModelViews;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace HandmadeProductManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService) 
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetCategories()
        {
            var result = await _categoryService.GetAll();
            return Ok(BaseResponse<IList<CategoryDto>>.OkResponse(result));
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetCategory(string id)
        {
            var result = await _categoryService.GetById(id);
            return Ok(BaseResponse<CategoryDto>.OkResponse(result));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCategory(CategoryForCreationDto categoryForCreation)
        {
            try
            {
                var result = await _categoryService.Create(categoryForCreation);
                return Ok(BaseResponse<CategoryDto>.OkResponse(result));
            }
            catch (DbUpdateException dbEx)
            {
                var innerException = dbEx.InnerException?.Message;
                return StatusCode(500, new
                {
                    title = "DbUpdateException",
                    status = 500,
                    detail = "An error occurred while saving the entity changes. See the inner exception for details.",
                    instance = HttpContext.Request.Path,
                    innerException
                });
            }
        }

        [HttpPut("{categoryId}")]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> UpdateCategory(string categoryId, CategoryForUpdateDto categoryForUpdate)
        {
            var result = await _categoryService.Update(categoryId, categoryForUpdate);
            return Ok(BaseResponse<CategoryDto>.OkResponse(result));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> DeleteCategory(string id)
        {
            var result = await _categoryService.SoftDelete(id);
            return Ok(BaseResponse<bool>.OkResponse(result));
        }
    }
}