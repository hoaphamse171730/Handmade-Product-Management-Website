using AutoMapper;
using FluentValidation;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
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
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), validationResult.Errors.First().ErrorMessage);
            }

            // Check if ProductItemId exists
            var productItemExists = await _unitOfWork.GetRepository<ProductItem>().Entities
                .AnyAsync(p => p.Id == productConfigurationDto.ProductItemId);
            if (!productItemExists)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidProductItem);
            }

            // Check if VariationOptionId exists
            var variationOptionExists = await _unitOfWork.GetRepository<VariationOption>().Entities
                .AnyAsync(v => v.Id == productConfigurationDto.VariationOptionId);
            if (!variationOptionExists)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidVariationOption);
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
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidGuidFormat);
            }

            if (!Guid.TryParse(variationOptionId, out _))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidGuidFormat);
            }

            var productConfigurationRepo = _unitOfWork.GetRepository<ProductConfiguration>();
            var productConfigurationEntity = await productConfigurationRepo.Entities
                .FirstOrDefaultAsync(x => x.ProductItemId == productItemId && x.VariationOptionId == variationOptionId)
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageProductConfigurationNotFound);

            await productConfigurationRepo.DeleteAsync(productItemId, variationOptionId);
            await _unitOfWork.SaveAsync();

            return true;
        }
    }
}