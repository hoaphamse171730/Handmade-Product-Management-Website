using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.ModelViews.CategoryModelViews;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using HandmadeProductManagement.Core.Constants;

namespace HandmadeProductManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService) => _categoryService = categoryService;

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _categoryService.GetAll();
            var response = new BaseResponse<IList<CategoryDto>>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Categories retrieved successfully",
                Data = categories
            };
            return Ok(response);
        }

        [HttpGet("{id}")]
        // [Authorize]
        public async Task<IActionResult> GetCategory(string id)
        {
            var category = await _categoryService.GetById(id);
            var response = new BaseResponse<CategoryDto>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Category retrieved successfully",
                Data = category
            };
            return Ok(response);
        }

        [HttpPost]
        // [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCategory(CategoryForCreationDto categoryForCreation)
        {

            var createdCategory = await _categoryService.Create(categoryForCreation);
            var response = new BaseResponse<CategoryDto>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Category created successfully",
                Data = createdCategory
            };
            return Ok(response);


        }

        [HttpPut("{categoryId}")]
        // [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCategory(string categoryId, CategoryForUpdateDto categoryForUpdate)
        {
            var updatedCategory = await _categoryService.Update(categoryId, categoryForUpdate);
            var response = new BaseResponse<CategoryDto>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Category updated successfully",
                Data = updatedCategory
            };
            return Ok(response);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(string id)
        {
            var isDeleted = await _categoryService.SoftDelete(id);
            var response = new BaseResponse<bool>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Category deleted successfully",
                Data = isDeleted
            };
            return Ok(response);
        }
    }
}
