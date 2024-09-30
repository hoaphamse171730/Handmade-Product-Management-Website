using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.ModelViews.PromotionModelViews;
using HandmadeProductManagement.Repositories.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;


namespace HandmadeProductManagement.Services.Service
{
    public class PromotionService : IPromotionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<PromotionForCreationDto> _creationValidator;
        private readonly IValidator<PromotionForUpdateDto> _updateValidator;

        public PromotionService(IUnitOfWork unitOfWork, IMapper mapper, IValidator<PromotionForCreationDto> validator, IValidator<PromotionForUpdateDto> updateValidator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _creationValidator = validator;
            _updateValidator = updateValidator;

        }

        public async Task<PromotionDto> GetById(string id)
        {
            var promotion = await _unitOfWork.GetRepository<Promotion>().Entities
                .FirstOrDefaultAsync(p => p.Id == id);

            if (promotion == null)
                throw new KeyNotFoundException("Promotion not found");

            var promotionToReturn = _mapper.Map<PromotionDto>(promotion);
            return promotionToReturn;
        }

        public async Task<PromotionDto> Create(PromotionForCreationDto promotion)
        {
            var result = _creationValidator.ValidateAsync(promotion);
            if (!result.Result.IsValid)
                throw new ValidationException(result.Result.Errors); 
            var promotionEntity = _mapper.Map<Promotion>(promotion);
            promotionEntity.CreatedTime = DateTime.UtcNow;
            await _unitOfWork.GetRepository<Promotion>().InsertAsync(promotionEntity);
            await _unitOfWork.SaveAsync();
            var promotionToReturn = _mapper.Map<PromotionDto>(promotionEntity);
            return promotionToReturn;
        }

        public async Task Update(string id, PromotionForUpdateDto promotion)
        {
            var result = _updateValidator.ValidateAsync(promotion);
            if (!result.Result.IsValid)
                throw new ValidationException(result.Result.Errors);
            var promotionEntity = await _unitOfWork.GetRepository<Promotion>().Entities
                .FirstOrDefaultAsync(p => p.Id == id);
            if (promotionEntity == null)
                throw new KeyNotFoundException("Promotion not found");
            _mapper.Map(promotion, promotionEntity);
            promotionEntity.LastUpdatedTime = DateTime.UtcNow;
            await _unitOfWork.GetRepository<Promotion>().UpdateAsync(promotionEntity);
            await _unitOfWork.SaveAsync();
        }

        public async Task Delete(string id)
        {
            var promotionRepo = _unitOfWork.GetRepository<Promotion>();
            var promotionEntity = await promotionRepo.Entities.FirstOrDefaultAsync(x => x.Id == id);
            if (promotionEntity == null)
            {
                throw new KeyNotFoundException("Promotion not found");
            }
            promotionEntity.DeletedTime = DateTime.UtcNow;
            await promotionRepo.DeleteAsync(id);
            await _unitOfWork.SaveAsync();
        }

        public async Task SoftDelete(string id)
        {
            var promotionRepo = _unitOfWork.GetRepository<Promotion>();
            var promotionEntity = await promotionRepo.Entities.FirstOrDefaultAsync(x => x.Id == id.ToString());
            if (promotionEntity == null)
                throw new KeyNotFoundException("Promotion not found");
            // promotionEntity.DeletedBy = userId.ToString();
            promotionEntity.DeletedTime = DateTime.UtcNow;
            await promotionRepo.UpdateAsync(promotionEntity);
            await _unitOfWork.SaveAsync();
        }

        public async Task<IList<PromotionDto>> GetAll()
        {
            var promotions = await _unitOfWork.GetRepository<Promotion>().Entities
                .Where(p => p.DeletedBy == null)
                .ToListAsync();
            var promotionsDto = _mapper.Map<IList<PromotionDto>>(promotions);
            return promotionsDto;
        }
    }
}
