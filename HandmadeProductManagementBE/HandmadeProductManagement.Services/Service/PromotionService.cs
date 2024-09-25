using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.ModelViews.PromotionModelViews;
using HandmadeProductManagement.ModelViews.UserModelViews;
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

    //     public async Task<IList<Promotion>> GetAll()
    //     {
    //         IQueryable<Promotion> query = _unitOfWork.GetRepository<Promotion>().Entities;
    //
    //         // Map ApplicationUser to UserResponseModel
    //         var result = await query.Select(pi => new Promotion
    //         {
    //             Id = pi.Id,
    //             Name = pi.Name,
    //             Description = pi.Description,
    //             PromotionName = pi.PromotionName,
    //             DiscountRate = pi.DiscountRate,
    //             StartDate = pi.StartDate,
    //             EndDate = pi.EndDate,
    //     //public string Name { get; set; } = string.Empty;
    //     //public string? Description { get; set; }
    //     //public string PromotionName { get; set; } = string.Empty;
    //     //public float DiscountRate { get; set; }
    //     //public DateTime StartDate { get; set; }
    //     //public DateTime EndDate { get; set; }
    //     //public string? Status { get; set; }
    // }).ToListAsync();
    //
    //         return result;  // Cast List to IList
    //     }

        public async Task<IEnumerable<PromotionDto>> GetAllPromotionAsync()
        {
            
        }

        public async Task<PromotionDto> GetPromotionByIdAsync(Guid id)
        {
            
        }

        public async Task DeletePromotionAsync(Guid id)
        {
            
        }

        public Task UpdatePromotionAsync(Guid id, PromotionForUpdateDto promotionDto)
        {
            throw new NotImplementedException();
        }

        public async Task CreatePromotionAsync(PromotionDto promotionDto)
        {
            throw new NotImplementedException();
        }
    }
}
