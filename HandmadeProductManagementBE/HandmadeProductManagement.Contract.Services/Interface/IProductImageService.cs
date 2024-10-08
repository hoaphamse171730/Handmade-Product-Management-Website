using HandmadeProductManagement.ModelViews.ProductImageModelViews;
using Microsoft.AspNetCore.Http;


namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface IProductImageService
    {
        Task<bool> UploadProductImage(IFormFile file, string productId);

        Task<bool> DeleteProductImage(string ImageId);

        Task<IList<productImageByIdResponse>> GetProductImageById(string id);
    }
}
