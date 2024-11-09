using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using Microsoft.AspNetCore.Http;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Constants;
using Microsoft.EntityFrameworkCore;
using HandmadeProductManagement.ModelViews.ProductImageModelViews;
using HandmadeProductManagement.Core.Common;


namespace HandmadeProductManagement.Services.Service
{
    public class ProductImageService : IProductImageService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductImageService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> UploadProductImage(List<IFormFile> files, string productId)
        {
            if (files == null || files.Count == 0)
            {
                throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageFileNotFound);
            }

            var product = await _unitOfWork.GetRepository<Product>()
                .Entities
                .FirstOrDefaultAsync(p => p.Id == productId)
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageProductNotFound);

            var uploadImageService = new ManageFirebaseImageService();

            foreach (var file in files)
            {
                if (file.Length == 0)
                {
                    throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageFileEmpty);
                }

                using (var stream = file.OpenReadStream())
                {
                    var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                    var imageUrl = await uploadImageService.UploadFileAsync(stream, fileName); 

                    // Tạo đối tượng ProductImage và lưu vào cơ sở dữ liệu
                    var productImage = new ProductImage
                    {
                        Url = imageUrl,
                        ProductId = productId
                    };

                    await _unitOfWork.GetRepository<ProductImage>().InsertAsync(productImage); 
                }
            }

            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<bool> DeleteProductImage(string imageId)
        {
            var productImage = await _unitOfWork.GetRepository<ProductImage>()
                .Entities
                .Where(pi => pi.Id == imageId)
                .FirstOrDefaultAsync()
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageImageNotFound);

            var deleteImageService = new ManageFirebaseImageService();
            await deleteImageService.DeleteFileAsync(productImage.Url);

            _unitOfWork.GetRepository<ProductImage>().Delete(productImage.Id);
            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<IList<productImageByIdResponse>> GetProductImageById(string id)
        {
            var images = await _unitOfWork.GetRepository<ProductImage>()
                .Entities
                .Where(pi => pi.ProductId == id)
                .OrderBy(pi => pi.CreatedTime)
                .Select(pi => new productImageByIdResponse
                {
                    Id = pi.Id,
                    Url = pi.Url,
                })
                .ToListAsync();

            return images;
        }
    }
}
