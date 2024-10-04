using AutoMapper;
using FluentValidation;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.ModelViews.CategoryModelViews;
using Microsoft.EntityFrameworkCore;

namespace HandmadeProductManagement.Services.Service
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<CategoryForCreationDto> _creationValidator;
        private readonly IValidator<CategoryForUpdateDto> _updateValidator;

        public CategoryService(IUnitOfWork unitOfWork, IMapper mapper,
            IValidator<CategoryForCreationDto> creationValidator, IValidator<CategoryForUpdateDto> updateValidator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _creationValidator = creationValidator;
            _updateValidator = updateValidator;
        }

        public async Task<IList<CategoryDto>> GetAll()
        {
            var categories = await _unitOfWork.GetRepository<Category>().Entities
                .Where(c => c.DeletedTime == null)
                .ToListAsync();
            return _mapper.Map<IList<CategoryDto>>(categories);
        }

        public async Task<CategoryDto> GetById(string id)
        {
            var category = await _unitOfWork.GetRepository<Category>().Entities
                .FirstOrDefaultAsync(c => c.Id == id && c.DeletedTime == null);
            if (category == null)
                throw new KeyNotFoundException("Category not found");
            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<CategoryDto> Create(CategoryForCreationDto category)
        {
            var validationResult = await _creationValidator.ValidateAsync(category);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);
            var categoryEntity = _mapper.Map<Category>(category);
            categoryEntity.CreatedTime = DateTime.UtcNow;
            await _unitOfWork.GetRepository<Category>().InsertAsync(categoryEntity);
            await _unitOfWork.SaveAsync();
            return _mapper.Map<CategoryDto>(categoryEntity);
        }

        public async Task<CategoryDto> Update(string id, CategoryForUpdateDto category)
        {
            var validationResult = await _updateValidator.ValidateAsync(category);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);
            var categoryEntity = await _unitOfWork.GetRepository<Category>().Entities
                .FirstOrDefaultAsync(c => c.Id == id && c.DeletedTime == null);
            if (categoryEntity == null)
                throw new KeyNotFoundException("Category not found");
            _mapper.Map(category, categoryEntity);
            categoryEntity.LastUpdatedTime = DateTime.UtcNow;
            await _unitOfWork.GetRepository<Category>().UpdateAsync(categoryEntity);
            await _unitOfWork.SaveAsync();
            return _mapper.Map<CategoryDto>(categoryEntity);
        }

        public async Task<bool> SoftDelete(string id)
        {
            var categoryRepo = _unitOfWork.GetRepository<Category>();
            var categoryEntity = await categoryRepo.Entities.FirstOrDefaultAsync(c => c.Id == id);
            if (categoryEntity == null)
                throw new KeyNotFoundException("Category not found");
            categoryEntity.DeletedTime = DateTime.UtcNow;
            await categoryRepo.UpdateAsync(categoryEntity);
            await _unitOfWork.SaveAsync();
            return true;
        }
    }
}
