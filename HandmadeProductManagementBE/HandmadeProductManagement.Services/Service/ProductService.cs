using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.ModelViews.ProductModelViews;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using HandmadeProductManagement.Contract.Repositories.Interface;

namespace HandmadeProductManagement.Services.Service
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IList<Product>> GetAll()
        {
            var products = await _unitOfWork.GetRepository<Product>().Entities.ToListAsync();
            return products;
        }
        public async Task<Product> GetById(string id)
        {
            var product = await _unitOfWork.GetRepository<Product>().GetByIdAsync(id);
            return product ?? throw new KeyNotFoundException("Product not found");
        }
        public async Task<Product> Create(Product product)
        {
            var productRepo = _unitOfWork.GetRepository<Product>();
            await productRepo.InsertAsync(product);
            await _unitOfWork.SaveAsync();
            return product;
        }

        public async Task<Product> Update(string id, Product updatedProduct)
        {
            var productRepo = _unitOfWork.GetRepository<Product>();
            var existingProduct = await productRepo.GetByIdAsync(id);
            if (existingProduct == null)
                throw new KeyNotFoundException("Product not found");
            existingProduct.Name = updatedProduct.Name;
            existingProduct.Description = updatedProduct.Description;
            existingProduct.Rating = updatedProduct.Rating;
            existingProduct.CategoryId = updatedProduct.CategoryId;
            existingProduct.SoldCount = updatedProduct.SoldCount;
            existingProduct.CreatedBy = updatedProduct.CreatedBy;
            existingProduct.DeletedBy = updatedProduct.DeletedBy;
            existingProduct.CreatedTime = updatedProduct.CreatedTime;
            existingProduct.DeletedTime = updatedProduct.DeletedTime;
            existingProduct.LastUpdatedBy = updatedProduct.LastUpdatedBy;
            existingProduct.LastUpdatedTime = updatedProduct.LastUpdatedTime;
            await productRepo.UpdateAsync(existingProduct);
            await _unitOfWork.SaveAsync();
            return existingProduct;
        }

        public async Task<bool> Delete(string id)
        {
            var productRepo = _unitOfWork.GetRepository<Product>();
            var product = await productRepo.GetByIdAsync(id);
            if (product == null)
                return false;
            await productRepo.DeleteAsync(product);
            await _unitOfWork.SaveAsync();
            return true;
        }
    }
}
