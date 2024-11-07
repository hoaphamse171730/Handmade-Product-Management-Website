using AutoMapper;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.ModelViews.CancelReasonModelViews;
using HandmadeProductManagement.ModelViews.CartItemModelViews;
using HandmadeProductManagement.ModelViews.CategoryModelViews;
using HandmadeProductManagement.ModelViews.OrderDetailModelViews;
using HandmadeProductManagement.ModelViews.ProductConfigurationModelViews;
using HandmadeProductManagement.ModelViews.ProductItemModelViews;
using HandmadeProductManagement.ModelViews.ProductModelViews;
using HandmadeProductManagement.ModelViews.PromotionModelViews;
using HandmadeProductManagement.ModelViews.StatusChangeModelViews;
using HandmadeProductManagement.ModelViews.UserInfoModelViews;
using HandmadeProductManagement.ModelViews.VariationModelViews;
using HandmadeProductManagement.ModelViews.VariationOptionModelViews;

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
        CreateMap<Variation, LatestVariationId>();
        #endregion

        #region VariationOption
        CreateMap<VariationOption, VariationOptionDto>();
        CreateMap<VariationOptionForCreationDto, VariationOption>();
        CreateMap<VariationOptionForUpdateDto, VariationOption>();
        CreateMap<VariationOption, LatestVariationOptionId>();
        #endregion

        #region Category
        CreateMap<Category, CategoryDto>();
        CreateMap<Category, CategoryDtoWithDetail>()
            .ForMember(dest => dest.Promotion, opt => opt.MapFrom(src => src.Promotion));
        CreateMap<CategoryForCreationDto, Category>();
        CreateMap<CategoryForUpdateDto, Category>();
        CreateMap<CategoryForUpdatePromotion, Category>();
        #endregion

        #region ProductItem
        CreateMap<ProductItem, ProductItemDto>();
        CreateMap<ProductItemForCreationDto, ProductItem>();
        CreateMap<ProductItemForUpdateDto, ProductItem>();
        #endregion

        #region ProductConfiguration
        CreateMap<ProductConfiguration, ProductConfigurationDto>();
        CreateMap<ProductConfigurationForCreationDto, ProductConfiguration>();
        #endregion

        #region UserInfo
        CreateMap<UserInfo, UserInfoDto>();
        CreateMap<UserInfoForUpdateDto, UserInfo>();
        #endregion

        #region CartItem
        CreateMap<CartItem, CartItemDto>();
        CreateMap<CartItemForCreationDto, CartItem>();
        CreateMap<CartItemForUpdateDto, CartItem>();
        #endregion
    }

}