using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Core.Utils;
using HandmadeProductManagement.ModelViews.OrderDetailModelViews;
using HandmadeProductManagement.ModelViews.ProductModelViews;
using HandmadeProductManagement.ModelViews.PromotionModelViews;
using Mapster;

namespace HandmadeProductManagementAPI;

public static class MappingConfig
{
    public static void RegisterMapping()
    {
        #region OrderDetail
        TypeAdapterConfig<OrderDetail, OrderDetailDto>.NewConfig()
            .Map(dest => dest.Id, src => src.OrderDetailId)
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

        #region Product
        TypeAdapterConfig<Product, ProductDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.CategoryId, src => src.CategoryId)
            .Map(dest => dest.ShopId, src => src.ShopId)
            .Map(dest => dest.Rating, src => src.Rating)
            .Map(dest => dest.Status, src => src.Status)
            .Map(dest => dest.SoldCount, src => src.SoldCount);

        TypeAdapterConfig<ProductForUpdateDto, Product>.NewConfig()
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.CategoryId, src => src.CategoryId)
            .Map(dest => dest.ShopId, src => src.ShopId)
            .Map(dest => dest.Rating, src => src.Rating)
            .Map(dest => dest.Status, src => src.Status)
            .Map(dest => dest.SoldCount, src => src.SoldCount);

        TypeAdapterConfig<ProductForCreationDto, Product>.NewConfig()
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.CategoryId, src => src.CategoryId)
            .Map(dest => dest.ShopId, src => src.ShopId)
            .Map(dest => dest.Rating, src => src.Rating)
            .Map(dest => dest.Status, src => src.Status)
            .Map(dest => dest.SoldCount, src => src.SoldCount);
        #endregion
    }
}