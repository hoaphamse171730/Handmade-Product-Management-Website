using System;
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
            var promotion = await _unitOfWork.GetRepository<Promotion>().Entities
                .FirstOrDefaultAsync(p => p.Id == id);

            if (promotion == null)
                throw new KeyNotFoundException("Promotion not found");

            var promotionToReturn = _mapper.Map<PromotionDto>(promotion);
            return promotionToReturn;
        }

        public async Task<PromotionDto> Create(PromotionForCreationDto promotion)
        {
            var promotionEntity = _mapper.Map<Promotion>(promotion);
            promotionEntity.CreatedTime = DateTime.UtcNow;
            await _unitOfWork.GetRepository<Promotion>().InsertAsync(promotionEntity);
            await _unitOfWork.SaveAsync();
            var promotionToReturn = _mapper.Map<PromotionDto>(promotionEntity);
            return promotionToReturn;
        }

        public async Task Update(string id, PromotionForUpdateDto promotion)
        {
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
           var promotionEntity = promotionRepo.Entities
               .FirstOrDefault(p => p.Id == id);
           if (promotionEntity == null)
               throw new KeyNotFoundException("Promotion not found");
           // promotionEntity.DeletedBy = userId.ToString();
           promotionEntity.DeletedTime = DateTime.UtcNow;
           promotionRepo.Delete(promotionEntity);
           await _unitOfWork.SaveAsync();
        }

        public async Task SoftDelete(string id)
        {
            var promotionRepo = _unitOfWork.GetRepository<Promotion>();
            var promotionEntity = await promotionRepo.Entities
                .FirstOrDefaultAsync(p => p.Id == id && p.DeletedBy == null);
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
