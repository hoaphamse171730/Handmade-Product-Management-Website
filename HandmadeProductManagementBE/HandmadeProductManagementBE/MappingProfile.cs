﻿using AutoMapper;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.ModelViews.OrderDetailModelViews;
using HandmadeProductManagement.ModelViews.PromotionModelViews;

namespace HandmadeProductManagementAPI;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        #region Promotion
        CreateMap<Promotion, PromotionDto>();
        CreateMap<PromotionForCreationDto, Promotion>();
        CreateMap<PromotionForUpdateDto, Promotion>();
        #endregion

        #region OrderDetail
        CreateMap<OrderDetail, OrderDetailDto>();
        CreateMap<OrderDetailForCreationDto, OrderDetail>();
        CreateMap<OrderDetailForUpdateDto, OrderDetail>();
        
        

        #endregion
       
    }

}