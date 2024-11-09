using HandmadeProductManagement.ModelViews.ProductImageModelViews;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface IProductImageService
    {
        Task<bool> UploadProductImage(List<IFormFile> files, string productId);

        Task<bool> DeleteProductImage(string ImageId);

        Task<IList<productImageByIdResponse>> GetProductImageById(string id);
    }
}
