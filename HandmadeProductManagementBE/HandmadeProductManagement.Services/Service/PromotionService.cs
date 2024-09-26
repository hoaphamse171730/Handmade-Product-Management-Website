using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.ModelViews.PromotionModelViews;
using HandmadeProductManagement.Repositories.Entity;
using Microsoft.EntityFrameworkCore;

namespace HandmadeProductManagement.Services.Service
{
    public class PromotionService : IPromotionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PromotionService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }


        public async Task<PromotionDto> GetById(string id)
        {
            var promotion = await _unitOfWork.GetRepository<Promotion>().GetByIdAsync(id);
            if (promotion is null)
                throw new KeyNotFoundException("Promotion not found");
            var promotionToReturn = _mapper.Map<PromotionDto>(promotion);
            return promotionToReturn;
        }

        public async Task<PromotionDto> Create(PromotionForCreationDto promotion)
        {
            var promotionEntity = _mapper.Map<Promotion>(promotion);
            await _unitOfWork.GetRepository<Promotion>().InsertAsync(promotionEntity);
            await _unitOfWork.SaveAsync();
            var promotionToReturn = _mapper.Map<PromotionDto>(promotionEntity);
            return promotionToReturn;
        }

        public async Task Update(string id, PromotionForUpdateDto promotion)
        {
            var promotionEntity = await _unitOfWork.GetRepository<Promotion>().GetByIdAsync(id);
            if (promotionEntity is null)
                throw new KeyNotFoundException("Promotion not found");
            _mapper.Map(promotion, promotionEntity);
            await _unitOfWork.GetRepository<Promotion>().UpdateAsync(promotionEntity);
            await _unitOfWork.SaveAsync();
        }
        
        
        public async Task Delete(string id)
        {
            var promotionEntity = await _unitOfWork.GetRepository<Promotion>().GetByIdAsync(id);
            var promotionRepo = _unitOfWork.GetRepository<Promotion>();
            if (promotionEntity is null)
                throw new KeyNotFoundException("Promotion not found");
            await promotionRepo.DeleteAsync(id);
            await _unitOfWork.SaveAsync();
        }

        Task<IList<PromotionDto>> IPromotionService.GetAll()
        {
            var promotions = _unitOfWork.GetRepository<Promotion>().Entities;
            var promotionsDto = _mapper.Map<IList<PromotionDto>>(promotions);
            return Task.FromResult(promotionsDto);
        } 
    }
}