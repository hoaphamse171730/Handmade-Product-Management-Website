using AutoMapper;
using FluentValidation;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Utils;
using HandmadeProductManagement.ModelViews.CategoryModelViews;
using HandmadeProductManagement.ModelViews.PromotionModelViews;
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

        public async Task<PaginatedList<CategoryDtoWithDetail>> GetAllWithDetailByPageAsync(int pageNumber, int pageSize)
        {
            var categories = await _unitOfWork.GetRepository<Category>().Entities
                .Include(c => c.Promotion)
                .Where(c => c.DeletedTime == null)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalCount = await _unitOfWork.GetRepository<Category>().Entities.CountAsync();

            var paginatedList = new PaginatedList<CategoryDtoWithDetail>(
                categories.Select(c => _mapper.Map<CategoryDtoWithDetail>(c)).ToList(),
                totalCount,
                pageNumber,
                pageSize
            );

            return paginatedList;
        }

        public async Task<CategoryDto> GetById(string id)
        {
            var category = await _unitOfWork.GetRepository<Category>().Entities
                .Include(c => c.Promotion)
                .FirstOrDefaultAsync(c => c.Id == id && c.DeletedTime == null);

            return category == null
                ? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageCategoryNotFound)
                : _mapper.Map<CategoryDto>(category);
        }

        public async Task<bool> Create(CategoryForCreationDto category)
        {
            var validationResult = await _creationValidator.ValidateAsync(category);
            if (!validationResult.IsValid)
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), validationResult.Errors.First().ErrorMessage);

            var existedCategory = await _unitOfWork.GetRepository<Category>().Entities
                .FirstOrDefaultAsync(c => c.Name == category.Name && c.DeletedTime == null);
            if (existedCategory is not null)
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageCategoryNameExists);

            var categoryEntity = _mapper.Map<Category>(category);
            categoryEntity.PromotionId = null;
            categoryEntity.CreatedTime = DateTime.UtcNow;
            categoryEntity.Status = Constants.CategoryStatusActive;
            categoryEntity.CreatedBy = Constants.RoleAdmin;
            categoryEntity.LastUpdatedBy = Constants.RoleAdmin;
            await _unitOfWork.GetRepository<Category>().InsertAsync(categoryEntity);
            await _unitOfWork.SaveAsync();
            _mapper.Map<CategoryDto>(categoryEntity);

            return true;
        }


        public async Task<CategoryDto> Update(string id, CategoryForUpdateDto category)

        {
            var validationResult = await _updateValidator.ValidateAsync(category);
            if (!validationResult.IsValid)
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), validationResult.Errors.First().ErrorMessage);

            var categoryEntity = await _unitOfWork.GetRepository<Category>().Entities
                .FirstOrDefaultAsync(c => c.Id == id && c.DeletedTime == null) ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageCategoryNotFound);
            
            if (!string.IsNullOrEmpty(category.Name))
            {
                var existedCategory = await _unitOfWork.GetRepository<Category>().Entities
                    .FirstOrDefaultAsync(c => c.Name == category.Name && c.DeletedTime == null);
                if (existedCategory is not null)
                    throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageCategoryNameExists);

                categoryEntity.Name = category.Name;
            }

            if (!string.IsNullOrEmpty(category.Description))
            {
                categoryEntity.Description = category.Description;
            }
            categoryEntity.LastUpdatedTime = DateTime.UtcNow;

            await _unitOfWork.GetRepository<Category>().UpdateAsync(categoryEntity);
            await _unitOfWork.SaveAsync();


            return _mapper.Map<CategoryDto>(categoryEntity);

        }


        public async Task<CategoryDto> UpdatePromotion(string id, CategoryForUpdatePromotion category)
        {
            if (!Guid.TryParse(id, out _))
            {
                throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageCategoryNotFound);
            }

            var categoryRepo = await _unitOfWork.GetRepository<Category>()
                .Entities
                .FirstOrDefaultAsync(c => c.Id == id && c.DeletedTime == null);

            if (categoryRepo is null)

                throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageCategoryNotFound);

            var validationResult = await _updatePromotionValidator.ValidateAsync(category);
            if (!validationResult.IsValid)
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), validationResult.Errors.First().ErrorMessage);

            // Kiểm tra xem promotionId có tồn tại hay không

            var promotionExists = await _unitOfWork.GetRepository<Promotion>()
                .Entities
                .AnyAsync(p => p.Id == category.promotionId && p.DeletedTime == null);
            if (!promotionExists)
            {
                throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessagePromotionNotFound);
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
            var categoryEntity = await categoryRepo.Entities
                .FirstOrDefaultAsync(c => c.Id == id)
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageCategoryNotFound);

            categoryEntity.DeletedTime = DateTime.UtcNow;
            await categoryRepo.UpdateAsync(categoryEntity);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> RestoreCategory(string id, string userId)
        {
            var categoryRepo = _unitOfWork.GetRepository<Category>();
            var categoryEntity = await categoryRepo.Entities.FirstOrDefaultAsync(x => x.Id == id);

            // Check if the category exists and has been deleted
            if (categoryEntity == null || categoryEntity.DeletedBy == null && !categoryEntity.DeletedTime.HasValue)
                throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageCategoryNotFound);

            // Recover the category
            categoryEntity.DeletedTime = null;
            categoryEntity.DeletedBy = null;
            categoryEntity.LastUpdatedBy = userId;

            await categoryRepo.UpdateAsync(categoryEntity);
            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<IList<CategoryDto>> GetAllDeleted()
        {
            var deletedCategories = await _unitOfWork.GetRepository<Category>().Entities
                .Include(c => c.Promotion)
                .Where(c => c.DeletedTime != null)
                .ToListAsync();
            return _mapper.Map<IList<CategoryDto>>(deletedCategories);
        }


    }
}