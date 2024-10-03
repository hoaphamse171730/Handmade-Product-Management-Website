using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
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


        public PromotionService(IUnitOfWork unitOfWork, IMapper mapper, IValidator<PromotionForCreationDto> creationValidator, IValidator<PromotionForUpdateDto> updateValidator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _creationValidator = creationValidator;
            _updateValidator = updateValidator;
        }

        public async Task<PromotionDto> GetById(string id)
        {
            var promotion = await _unitOfWork.GetRepository<Promotion>().Entities
                .FirstOrDefaultAsync(p => p.Id == id && p.DeletedTime == null);
            if (promotion == null)
                throw new KeyNotFoundException("Promotion not found");
            return _mapper.Map<PromotionDto>(promotion);
        }

        public async Task<PromotionDto> Create(PromotionForCreationDto promotion)
        {
            var validationResult = await _creationValidator.ValidateAsync(promotion);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);
            var promotionEntity = _mapper.Map<Promotion>(promotion);
            promotionEntity.CreatedTime = DateTime.UtcNow;
            await _unitOfWork.GetRepository<Promotion>().InsertAsync(promotionEntity);
            await _unitOfWork.SaveAsync();
            return _mapper.Map<PromotionDto>(promotionEntity);
        }

        public async Task<PromotionDto> Update(string id, PromotionForUpdateDto promotion)
        {
            var validationResult = await _updateValidator.ValidateAsync(promotion);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);
            var promotionEntity = await _unitOfWork.GetRepository<Promotion>().Entities
                .FirstOrDefaultAsync(p => p.Id == id && p.DeletedTime == null);
            if (promotionEntity == null)
                throw new KeyNotFoundException("Promotion not found");
            _mapper.Map(promotion, promotionEntity);
            promotionEntity.LastUpdatedTime = DateTime.UtcNow;
            await _unitOfWork.GetRepository<Promotion>().UpdateAsync(promotionEntity);
            await _unitOfWork.SaveAsync();
            return _mapper.Map<PromotionDto>(promotionEntity);
        }
        public async Task<bool> SoftDelete(string id)
        {
            var promotionRepo = _unitOfWork.GetRepository<Promotion>();
            var promotionEntity = await promotionRepo.Entities.FirstOrDefaultAsync(p => p.Id == id);
            if (promotionEntity == null)
                throw new KeyNotFoundException("Promotion not found");
            promotionEntity.DeletedTime = DateTime.UtcNow;
            await promotionRepo.UpdateAsync(promotionEntity);
            await _unitOfWork.SaveAsync();
            return true;
        }


        public async Task<IList<PromotionDto>> GetAll()
        {
            var promotions = await _unitOfWork.GetRepository<Promotion>().Entities
                .Where(p => p.DeletedTime == null)
                .ToListAsync();
            return _mapper.Map<IList<PromotionDto>>(promotions);
        }


        public async Task<bool> updatePromotionStatusByRealtime(string id)
        {
            var promotion = await _unitOfWork.GetRepository<Promotion>().Entities
                .FirstOrDefaultAsync(p => p.Id == id && p.DeletedTime == null);
            if (promotion == null)
            {
                throw new BaseException.NotFoundException("not_found", "Promotion Not Found!");
            }
            if (DateTime.UtcNow > promotion.EndDate)
            {
                promotion.Status = "inactive";
                await _unitOfWork.GetRepository<Promotion>().UpdateAsync(promotion);
                await _unitOfWork.SaveAsync();
            }
            return true;
        }

    }
}
