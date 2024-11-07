﻿using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.ModelViews.CategoryModelViews;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Utils;

namespace HandmadeProductManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService) => _categoryService = categoryService;

        [HttpGet]
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

        [Authorize]
        [HttpGet("pagedetail")]
        public async Task<IActionResult> GetCategoriesWithDetailByPage([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var categories = await _categoryService.GetAllWithDetailByPageAsync(pageNumber, pageSize);
            var response = new BaseResponse<IList<CategoryDtoWithDetail>>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Categories with details retrieved successfully",
                Data = categories
            };
            return Ok(response);
        }

        [HttpGet("{id}")]
        [Authorize]
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCategory(CategoryForCreationDto categoryForCreation)
        {
            await _categoryService.Create(categoryForCreation);
            var response = new BaseResponse<bool>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Category created successfully",
                Data = true
            };
            return Ok(response);
        }


        [HttpPut("{categoryId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCategory(string categoryId, CategoryForUpdateDto categoryForUpdate)
        {
            await _categoryService.Update(categoryId, categoryForUpdate);
            var response = new BaseResponse<bool>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Category updated successfully",
                Data = true
            };
            return Ok(response);
        }

        [Authorize(Roles = "Admin, Seller")]
        [HttpPut("updatepromotion/{categoryId}")]
        public async Task<IActionResult> UpdatePromotion(string categoryId, CategoryForUpdatePromotion categoryForUpdatePromotion)
        {
            await _categoryService.UpdatePromotion(categoryId, categoryForUpdatePromotion);
            var reponse = new BaseResponse<bool>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Category updated successfully",
                Data = true
            };
            return Ok(reponse);
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

        [HttpPatch("restore/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RestoreCategory(string id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var isRestored = await _categoryService.RestoreCategory(id, userId);
            var response = new BaseResponse<bool>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Category restored successfully",
                Data = isRestored
            };
            return Ok(response);
        }

        [HttpGet("GetAllDelete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllDeletedCategories([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var deletedCategories = await _categoryService.GetAllDeleted(pageNumber, pageSize);
            var response = new BaseResponse<IList<CategoryDto>>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Message = "Get the successfully deleted categories",
                Data = deletedCategories
            };
            return Ok(response);
        }


    }
}
