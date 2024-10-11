using AutoMapper;
using FluentValidation;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Utils;
using HandmadeProductManagement.ModelViews.ProductItemModelViews;
using Microsoft.EntityFrameworkCore;

namespace HandmadeProductManagement.Services.Service
{
    public class ProductItemService : IProductItemService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<ProductItemForCreationDto> _creationValidator;
        private readonly IValidator<ProductItemForUpdateDto> _updateValidator;

        public ProductItemService(IUnitOfWork unitOfWork, IMapper mapper, IValidator<ProductItemForCreationDto> creationValidator, IValidator<ProductItemForUpdateDto> updateValidator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _creationValidator = creationValidator;
            _updateValidator = updateValidator;
        }

        // Get product item by productId
        public async Task<ProductItemDto> GetByProductId(string productId)
        {
            // Validate id format
            if (!Guid.TryParse(productId, out var guidProductId))
            {
                throw new BaseException.BadRequestException("invalid_input", "Product ID is not in a valid GUID format.");
            }

            var productItemEntity = await _unitOfWork.GetRepository<ProductItem>().Entities
                .FirstOrDefaultAsync(pi => pi.ProductId == productId && (!pi.DeletedTime.HasValue || pi.DeletedBy == null));

            if (productItemEntity == null)
            {
                throw new BaseException.NotFoundException("not_found", "Product item not found");
            }

            var productItemDto = _mapper.Map<ProductItemDto>(productItemEntity);
            return productItemDto;
        }

        // Create a new product item
        public async Task<bool> Create(ProductItemForCreationDto productItemDto, string userId)
        {
            // Validate
            var validationResult = await _creationValidator.ValidateAsync(productItemDto);
            if (!validationResult.IsValid)
            {
                throw new BaseException.BadRequestException("validation_failed", validationResult.Errors.Select(e => e.ErrorMessage).FirstOrDefault());
            }

            var productItemEntity = _mapper.Map<ProductItem>(productItemDto);

            productItemEntity.CreatedBy = userId;
            productItemEntity.LastUpdatedBy = userId;

            await _unitOfWork.GetRepository<ProductItem>().InsertAsync(productItemEntity);
            await _unitOfWork.SaveAsync();

            return true;
        }

        // Update an existing product item partially (PATCH)
        public async Task<bool> Update(string id, ProductItemForUpdateDto productItemDto, string userId)
        {
            // Validate id format
            if (!Guid.TryParse(id, out var guidId))
            {
                throw new BaseException.BadRequestException("invalid_input", "ID is not in a valid GUID format.");
            }

            var validationResult = await _updateValidator.ValidateAsync(productItemDto);
            if (!validationResult.IsValid)
            {
                throw new BaseException.BadRequestException("validation_failed", validationResult.Errors.Select(e => e.ErrorMessage).FirstOrDefault());
            }

            var productItemEntity = await _unitOfWork.GetRepository<ProductItem>().Entities
                .FirstOrDefaultAsync(p => p.Id == id && (!p.DeletedTime.HasValue || p.DeletedBy == null));

            if (productItemEntity == null)
            {
                throw new BaseException.NotFoundException("not_found", "Product item not found");
            }

            if (productItemDto.Price.HasValue)
            {
                productItemEntity.Price = productItemDto.Price.Value;
            }

            if (productItemDto.QuantityInStock.HasValue)
            {
                productItemEntity.QuantityInStock = productItemDto.QuantityInStock.Value;
            }

            // Cập nhật thông tin metadata
            productItemEntity.LastUpdatedTime = DateTime.UtcNow;
            productItemEntity.LastUpdatedBy = userId;

            await _unitOfWork.GetRepository<ProductItem>().UpdateAsync(productItemEntity);
            await _unitOfWork.SaveAsync();

            return true;
        }


        // Soft delete a product item
        public async Task<bool> Delete(string id, string userId)
        {
            // Validate id format
            if (!Guid.TryParse(id, out var guidId))
            {
                throw new BaseException.BadRequestException("invalid_input", "ID is not in a valid GUID format.");
            }

            var productItemRepo = _unitOfWork.GetRepository<ProductItem>();
            var productItemEntity = await productItemRepo.Entities.FirstOrDefaultAsync(p => p.Id == id);

            if (productItemEntity == null || productItemEntity.DeletedTime.HasValue || productItemEntity.DeletedBy != null)
            {
                throw new BaseException.NotFoundException("not_found", "Product item not found");
            }

            productItemEntity.DeletedTime = DateTime.UtcNow;
            productItemEntity.DeletedBy = userId;

            await productItemRepo.UpdateAsync(productItemEntity);
            await _unitOfWork.SaveAsync();

            return true;
        }
    }
}
