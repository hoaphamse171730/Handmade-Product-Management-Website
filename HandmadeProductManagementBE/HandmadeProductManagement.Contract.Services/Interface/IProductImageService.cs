using Microsoft.AspNetCore.Http;


namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface IProductImageService
    {
        Task<bool> UploadProductImage(IFormFile file, string productId);
    }
}
