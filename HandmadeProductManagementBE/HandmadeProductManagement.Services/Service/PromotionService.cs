using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.PromotionModelViews;
using Microsoft.EntityFrameworkCore;

namespace HandmadeProductManagement.Services.Service
{
    public class PromotionService : IPromotionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<PromotionForCreationDto> _creationValidator;
        private readonly IValidator<PromotionForUpdateDto> _updateValidator;


        public PromotionService(IUnitOfWork unitOfWork, IMapper mapper,
            IValidator<PromotionForCreationDto> creationValidator, IValidator<PromotionForUpdateDto> updateValidator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _creationValidator = creationValidator;
            _updateValidator = updateValidator;
        }

        public async Task<IList<PromotionDto>> GetAll(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0)
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidPageNumber);

            if (pageSize <= 0)
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidPageSize);

            var promotions = await _unitOfWork.GetRepository<Promotion>().Entities
                .Where(p => p.DeletedTime == null)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return _mapper.Map<IList<PromotionDto>>(promotions);
        }

        public async Task<IList<PromotionDto>> GetExpiredPromotions(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0)
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidPageNumber);

            if (pageSize <= 0)
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidPageSize);

            var promotions = await _unitOfWork.GetRepository<Promotion>().Entities
                .Where(p => p.DeletedTime == null && p.EndDate < DateTime.UtcNow)
                .ToListAsync();

            return _mapper.Map<IList<PromotionDto>>(promotions);
        }

        public async Task<PromotionDto> GetById(string id)
        {
            if (!Guid.TryParse(id, out _))
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidGuidFormat);

            var promotion = await _unitOfWork.GetRepository<Promotion>().Entities
                .FirstOrDefaultAsync(p => p.Id == id && p.DeletedTime == null);

            return promotion == null
                ? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessagePromotionNotFound)
                : _mapper.Map<PromotionDto>(promotion);
        }

        public async Task<bool> Create(PromotionForCreationDto promotion, string userId)
        {
            var validationResult = await _creationValidator.ValidateAsync(promotion);
            if (!validationResult.IsValid)
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), validationResult.Errors.First().ErrorMessage);

            var isNameDuplicated = await _unitOfWork.GetRepository<Promotion>().Entities
                .AnyAsync(p => p.Name == promotion.Name && p.DeletedTime == null);

            if (isNameDuplicated)
                throw new ValidationException(new List<ValidationFailure>
                {
                    new(nameof(promotion.Name), Constants.ErrorMessageNameInUse)
                });

            var promotionEntity = _mapper.Map<Promotion>(promotion);
            promotionEntity.CreatedTime = DateTime.UtcNow;
            promotionEntity.Status = promotion.StartDate > DateTime.UtcNow ? Constants.PromotionStatusInactive : Constants.PromotionStatusActive;
            promotionEntity.CreatedBy = userId;
            promotionEntity.LastUpdatedBy = userId;
            promotionEntity.LastUpdatedTime = DateTime.UtcNow;

            await _unitOfWork.GetRepository<Promotion>().InsertAsync(promotionEntity);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> Update(string id, PromotionForUpdateDto promotion, string userId)
        {
            if (!Guid.TryParse(id, out _))
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(),
                    Constants.ErrorMessageInvalidGuidFormat);

            var validationResult = await _updateValidator.ValidateAsync(promotion);
            if (!validationResult.IsValid)
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), validationResult.Errors.First().ErrorMessage);

            var promotionEntity = await _unitOfWork.GetRepository<Promotion>().Entities
                .FirstOrDefaultAsync(p => p.Id == id && p.DeletedTime == null)
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(),
                    Constants.ErrorMessagePromotionNotFound);

            var isNameDuplicated = await _unitOfWork.GetRepository<Promotion>().Entities
                .AnyAsync(p => p.Name == promotion.Name && p.DeletedTime == null);

            if (isNameDuplicated)
                throw new ValidationException(new List<ValidationFailure>
        {
            new(nameof(promotion.Name), Constants.ErrorMessageNameInUse)
        });

            _mapper.Map(promotion, promotionEntity);
            promotionEntity.LastUpdatedTime = DateTime.UtcNow;
            promotionEntity.LastUpdatedBy = userId;

            await _unitOfWork.GetRepository<Promotion>().UpdateAsync(promotionEntity);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> SoftDelete(string id)
        {
            if (!Guid.TryParse(id, out _))
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(),
                    Constants.ErrorMessageInvalidGuidFormat);

            var promotionRepo = _unitOfWork.GetRepository<Promotion>();
            var promotionEntity = await promotionRepo.Entities.FirstOrDefaultAsync(p => p.Id == id);

            if (promotionEntity is null)
                throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(),
                    Constants.ErrorMessagePromotionNotFound);

            promotionEntity.Status = Constants.PromotionStatusInactive;
            promotionEntity.LastUpdatedBy = "user";
            promotionEntity.DeletedTime = DateTime.UtcNow;

            await promotionRepo.UpdateAsync(promotionEntity);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> UpdatePromotionStatusByRealtime(string id)
        {
            var promotion = await _unitOfWork.GetRepository<Promotion>().Entities
                .FirstOrDefaultAsync(p => p.Id == id && p.DeletedTime == null)
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessagePromotionNotFound);

            if (DateTime.UtcNow < promotion.StartDate || DateTime.UtcNow > promotion.EndDate)
                promotion.Status = Constants.PromotionStatusInactive; 
            else
                promotion.Status = Constants.PromotionStatusActive;

            await _unitOfWork.GetRepository<Promotion>().UpdateAsync(promotion);
            await _unitOfWork.SaveAsync();
            return true;
        }

    }
}