using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.ModelViews.OrderDetailModelViews;
using HandmadeProductManagement.ModelViews.PromotionModelViews;
using Mapster;

namespace HandmadeProductManagementAPI;

public static class MappingConfig
{
    public static void RegisterMapping()
    {
        #region Promotion
        TypeAdapterConfig<Promotion, PromotionDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.PromotionName, src => src.PromotionName)  
            .Map(dest => dest.DiscountRate, src => src.DiscountRate)
            .Map(dest => dest.StartDate, src => src.StartDate)
            .Map(dest => dest.EndDate, src => src.EndDate)
            .Map(dest => dest.Status, src => src.Status);  
        
        TypeAdapterConfig<PromotionForCreationDto, Promotion>.NewConfig()
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.PromotionName, src => src.PromotionName)  
            .Map(dest => dest.DiscountRate, src => src.DiscountRate)
            .Map(dest => dest.StartDate, src => src.StartDate)
            .Map(dest => dest.EndDate, src => src.EndDate)
            .Map(dest => dest.Status, src => src.Status);

        TypeAdapterConfig<PromotionForUpdateDto, Promotion>.NewConfig()
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.PromotionName, src => src.PromotionName)  
            .Map(dest => dest.DiscountRate, src => src.DiscountRate)
            .Map(dest => dest.StartDate, src => src.StartDate)
            .Map(dest => dest.EndDate, src => src.EndDate)
            .Map(dest => dest.Status, src => src.Status);
        #endregion

        #region OrderDetail
        TypeAdapterConfig<OrderDetail, OrderDetailDto>.NewConfig()
            .Map(dest => dest.OrderDetailId, src => src.OrderDetailId)
            .Map(dest => dest.ProductId, src => src.ProductId)
            .Map(dest => dest.OrderId, src => src.OrderId)
            .Map(dest => dest.ProductQuantity, src => src.ProductQuantity)
            .Map(dest => dest.UnitPrice, src => src.UnitPrice);

        TypeAdapterConfig<OrderDetailForCreationDto, OrderDetail>.NewConfig()
            .Map(dest => dest.ProductId, src => src.ProductId)
            .Map(dest => dest.OrderId, src => src.OrderId)
            .Map(dest => dest.ProductQuantity, src => src.ProductQuantity)
            .Map(dest => dest.UnitPrice, src => src.UnitPrice);

        TypeAdapterConfig<OrderDetailForUpdateDto, OrderDetail>.NewConfig()
            .Map(dest => dest.ProductId, src => src.ProductId)
            .Map(dest => dest.OrderId, src => src.OrderId)
            .Map(dest => dest.ProductQuantity, src => src.ProductQuantity)
            .Map(dest => dest.UnitPrice, src => src.UnitPrice);
        

        #endregion
       
        
        
        
    }
}