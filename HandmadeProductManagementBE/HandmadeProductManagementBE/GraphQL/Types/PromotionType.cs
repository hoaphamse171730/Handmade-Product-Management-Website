using HandmadeProductManagement.ModelViews.PromotionModelViews;

namespace HandmadeProductManagementAPI.GraphQL.Types
{
    public class PromotionType : ObjectType<PromotionDto>
    { protected override void Configure(IObjectTypeDescriptor<PromotionDto> descriptor)
        {
            descriptor.Field(p => p.Id).Type<IdType>();
            descriptor.Field(p => p.Name).Type<StringType>();
            descriptor.Field(p => p.Description).Type<StringType>();
            descriptor.Field(p => p.DiscountRate).Type<DecimalType>();
            descriptor.Field(p => p.StartDate).Type<DateTimeType>();
            descriptor.Field(p => p.EndDate).Type<DateTimeType>();
        }
    }
}
