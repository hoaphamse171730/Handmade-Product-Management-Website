using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Repositories.Entity;
using Microsoft.EntityFrameworkCore;

namespace HandmadeProductManagement.Services.Service
{
    public class PromotionService : IPromotionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PromotionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public async Task<IList<Promotion>> GetAll()
        {
            IQueryable<Promotion> query = _unitOfWork.GetRepository<Promotion>().Entities;
            var result = await query.ToListAsync();
            return result;
        }

       


        public async Task<Promotion> GetById(string id)
        {
            var promotion = await _unitOfWork.GetRepository<Promotion>().GetByIdAsync(Guid.Parse(id));

            if (promotion == null)
            {
                throw new KeyNotFoundException("Promotion not found");
            }

            return promotion;
        }

        public async Task<Promotion> Create(Promotion promotion)
        {
            await _unitOfWork.GetRepository<Promotion>().InsertAsync(promotion);
            await _unitOfWork.SaveAsync();
            return promotion;
        }

        public async Task<Promotion> Update(string id, Promotion updatedPromotion)
        {
            var promotion = await _unitOfWork.GetRepository<Promotion>().GetByIdAsync(id);
            if (promotion == null)
            {
                throw new KeyNotFoundException("Promotion not found");
            }
            promotion.Name = updatedPromotion.Name;
            promotion.Description = updatedPromotion.Description;
            promotion.PromotionName = updatedPromotion.PromotionName;
            promotion.DiscountRate = updatedPromotion.DiscountRate;
            promotion.StartDate = updatedPromotion.StartDate;
            promotion.EndDate = updatedPromotion.EndDate;
            promotion.Status = updatedPromotion.Status;
            _unitOfWork.GetRepository<Promotion>().Update(promotion);
            await _unitOfWork.SaveAsync();
            return promotion;
        }

        public async Task<bool> Delete(string id)
        {
            var promotion = await _unitOfWork.GetRepository<Promotion>().GetByIdAsync(id);
            if (promotion == null)
            {
                throw new KeyNotFoundException("Promotion not found");
            }
            _unitOfWork.GetRepository<Promotion>().Delete(promotion);
            await _unitOfWork.SaveAsync();
            return true;
        }
    }
}