using AutoMapper;
using FluentValidation;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.ModelViews.ProductConfigurationModelViews;
using Microsoft.EntityFrameworkCore;

namespace HandmadeProductManagement.Services.Service
{
    public class ProductConfigurationService : IProductConfigurationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<ProductConfigurationForCreationDto> _creationValidator;

        public ProductConfigurationService(IUnitOfWork unitOfWork, IMapper mapper, IValidator<ProductConfigurationForCreationDto> creationValidator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _creationValidator = creationValidator;
        }

        // Create a new Product Configuration
        public async Task<bool> Create(ProductConfigurationForCreationDto productConfigurationDto)
        {
            var validationResult = await _creationValidator.ValidateAsync(productConfigurationDto);
            if (!validationResult.IsValid)
            {
                throw new BaseException.BadRequestException("validation_failed", validationResult.Errors.First().ErrorMessage);
            }

            // Check if ProductItemId exists
            var productItemExists = await _unitOfWork.GetRepository<ProductItem>().Entities
                .AnyAsync(p => p.Id == productConfigurationDto.ProductItemId);
            if (!productItemExists)
            {
                throw new BaseException.BadRequestException("invalid_product_item", "ProductItemId does not exist.");
            }

            // Check if VariationOptionId exists
            var variationOptionExists = await _unitOfWork.GetRepository<VariationOption>().Entities
                .AnyAsync(v => v.Id == productConfigurationDto.VariationOptionId);
            if (!variationOptionExists)
            {
                throw new BaseException.BadRequestException("invalid_variation_option", "VariationOptionId does not exist.");
            }

            var productConfigurationEntity = _mapper.Map<ProductConfiguration>(productConfigurationDto);

            await _unitOfWork.GetRepository<ProductConfiguration>().InsertAsync(productConfigurationEntity);
            await _unitOfWork.SaveAsync();

            return true;
        }

        // Delete
        public async Task<bool> Delete(string productItemId, string variationOptionId)
        {
            if (!Guid.TryParse(productItemId, out _))
            {
                throw new BaseException.BadRequestException("invalid_input", "ID is not in a valid GUID format.");
            }

            if (!Guid.TryParse(variationOptionId, out _))
            {
                throw new BaseException.BadRequestException("invalid_input", "ID is not in a valid GUID format.");
            }
            var productConfigurationRepo = _unitOfWork.GetRepository<ProductConfiguration>();
            var productConfigurationEntity = await productConfigurationRepo.Entities
                .FirstOrDefaultAsync(x => x.ProductItemId == productItemId && x.VariationOptionId == variationOptionId) ?? throw new BaseException.NotFoundException("not_found", "Product Configuration not found");
            await productConfigurationRepo.DeleteAsync(productItemId, variationOptionId);
            await _unitOfWork.SaveAsync();

            return true;
        }
    }
}