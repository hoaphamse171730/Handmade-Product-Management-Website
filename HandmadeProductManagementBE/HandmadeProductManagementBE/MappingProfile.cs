using AutoMapper;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.ModelViews.CancelReasonModelViews;
using HandmadeProductManagement.ModelViews.OrderDetailModelViews;
using HandmadeProductManagement.ModelViews.ProductModelViews;
using HandmadeProductManagement.ModelViews.PromotionModelViews;
using HandmadeProductManagement.ModelViews.StatusChangeModelViews;
using HandmadeProductManagement.ModelViews.VariationModelViews;

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

        #region Product
        CreateMap<Product, ProductDto>();
        CreateMap<ProductForCreationDto, Product>();
        CreateMap<ProductForUpdateDto, Product>();
        #endregion

        #region CancelReason
        CreateMap<CancelReason, CancelReasonDto>();
        CreateMap<CancelReasonForCreationDto, CancelReason>();
        CreateMap<CancelReasonForUpdateDto, CancelReason>();
        #endregion

        #region StatusChange
        CreateMap<StatusChange, StatusChangeDto>();
        CreateMap<StatusChangeForCreationDto, StatusChange>();
        CreateMap<StatusChangeForUpdateDto, StatusChange>();
        #endregion

        #region Variation
        CreateMap<Variation, VariationDto>();
        CreateMap<VariationForCreationDto, Variation>();
        CreateMap<VariationForUpdateDto, Variation>();
        #endregion
    }

}