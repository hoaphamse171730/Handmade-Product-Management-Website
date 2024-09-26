using AutoMapper;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.ModelViews.PromotionModelViews;

namespace HandmadeProductManagementAPI;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Promotion, PromotionDto>();
        CreateMap<PromotionForCreationDto, Promotion>();
        CreateMap<PromotionForUpdateDto, Promotion>();
    }

}