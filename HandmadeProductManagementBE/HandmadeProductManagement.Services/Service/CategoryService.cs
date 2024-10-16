using AutoMapper;
using Firebase.Auth;
using FluentValidation;
using FluentValidation.Results;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Constants;
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
        private readonly IValidator<CategoryForUpdatePromotion> _updatePromotionValidator;
        

        public CategoryService(IUnitOfWork unitOfWork, IMapper mapper,
            IValidator<CategoryForCreationDto> creationValidator, IValidator<CategoryForUpdateDto> updateValidator
            ,IValidator<CategoryForUpdatePromotion> updatePromotionValidator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _creationValidator = creationValidator;
            _updateValidator = updateValidator;
            _updatePromotionValidator = updatePromotionValidator;
        }

        public async Task<IList<CategoryDto>> GetAll()
        {
            var categories = await _unitOfWork.GetRepository<Category>().Entities
                .Include(c => c.Promotion)
                .Where(c => c.DeletedTime == null)
                .ToListAsync();
            return _mapper.Map<IList<CategoryDto>>(categories);
        }

        public async Task<CategoryDto> GetById(string id)
        {
            var category = await _unitOfWork.GetRepository<Category>().Entities
                .Include(c => c.Promotion)
                .FirstOrDefaultAsync(c => c.Id == id && c.DeletedTime == null);
            return category == null
                ? throw new BaseException.NotFoundException("404", "Category not found")
                : _mapper.Map<CategoryDto>(category);
        }

        public async Task<bool> Create(CategoryForCreationDto category)
        {
            var validationResult = await _creationValidator.ValidateAsync(category);
            if (!validationResult.IsValid)
                throw new BaseException.BadRequestException("validation_failed", validationResult.Errors.First().ErrorMessage);

            var existedCategory = await _unitOfWork.GetRepository<Category>().Entities
                .FirstOrDefaultAsync(c => c.Name == category.Name && c.DeletedTime == null);
            if (existedCategory is not null)
                throw new ValidationException("Category name already exists");

            var categoryEntity = _mapper.Map<Category>(category);
            categoryEntity.PromotionId = null;
            categoryEntity.CreatedTime = DateTime.UtcNow;
            categoryEntity.Status = "Active";
            categoryEntity.CreatedBy = "user";
            categoryEntity.LastUpdatedBy = "user";
            await _unitOfWork.GetRepository<Category>().InsertAsync(categoryEntity);
            await _unitOfWork.SaveAsync();
            _mapper.Map<CategoryDto>(categoryEntity);

            return true; 
        }




        public async Task<CategoryDto> Update(string id, CategoryForUpdateDto category)
        {
            var validationResult = await _updateValidator.ValidateAsync(category);
            if (!validationResult.IsValid)
                throw new BaseException.BadRequestException("validation_failed", validationResult.Errors.First().ErrorMessage);

            var categoryEntity = await _unitOfWork.GetRepository<Category>().Entities
                .FirstOrDefaultAsync(c => c.Id == id && c.DeletedTime == null) ?? throw new KeyNotFoundException("Category not found");

            var existedCategory = await _unitOfWork.GetRepository<Category>().Entities
                .FirstOrDefaultAsync(c => c.Name == category.Name && c.DeletedTime == null);
            if (existedCategory is not null)
                throw new ValidationException("Category name already exists");

            _mapper.Map(category, categoryEntity);
            categoryEntity.LastUpdatedTime = DateTime.UtcNow;
            await _unitOfWork.GetRepository<Category>().UpdateAsync(categoryEntity);
            await _unitOfWork.SaveAsync();
            return _mapper.Map<CategoryDto>(categoryEntity);
        }
        public async Task<CategoryDto> UpdatePromotion(string id, CategoryForUpdatePromotion category)
        {
            if (!Guid.TryParse(id, out _))
            {
                throw new BaseException.BadRequestException("invalid_id_format",
                    "The provided ID is not in a valid GUID format.");
            }

            var categoryRepo = await _unitOfWork.GetRepository<Category>()
                .Entities
                .FirstOrDefaultAsync(c => c.Id == id && c.DeletedTime == null);

            if (categoryRepo is null)
                throw new BaseException.NotFoundException("400", "Category not found");

            var validationResult = await _updatePromotionValidator.ValidateAsync(category);
            if(!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            // Kiểm tra xem promotionId có tồn tại hay không
            var promotionExists = await _unitOfWork.GetRepository<Promotion>()
                .Entities
                .AnyAsync(p => p.Id == category.promotionId && p.DeletedTime == null);

            if (!promotionExists)
            {
                throw new BaseException.NotFoundException("404", "Promotion not found");
            }

            categoryRepo.PromotionId = category.promotionId;
            categoryRepo.LastUpdatedTime = DateTime.UtcNow;

            await _unitOfWork.GetRepository<Category>().UpdateAsync(categoryRepo);
            await _unitOfWork.SaveAsync();
            return _mapper.Map<CategoryDto>(categoryRepo);
        }

        public async Task<bool> SoftDelete(string id)
        {
            var categoryRepo = _unitOfWork.GetRepository<Category>();
            var categoryEntity = await categoryRepo.Entities.FirstOrDefaultAsync(c => c.Id == id) ?? throw new KeyNotFoundException("Category not found");
            categoryEntity.DeletedTime = DateTime.UtcNow;
            await categoryRepo.UpdateAsync(categoryEntity);
            await _unitOfWork.SaveAsync();
            return true;
        }
    }
}