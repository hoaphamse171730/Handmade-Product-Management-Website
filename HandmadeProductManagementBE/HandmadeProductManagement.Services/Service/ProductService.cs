using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.ModelViews.ProductModelViews;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandmadeProductManagement.Services.Service
{
    public class ProductService : IProductService
    {

        private readonly IUnitOfWork _unitOfWork;

        public ProductService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse<IEnumerable<ProductResponseModel>>> SearchProductsAsync(ProductSearchModel searchModel)
        {
            var query = _unitOfWork.GetRepository<Product>().Entities.AsQueryable();

            // Apply Search Filters
            if (!string.IsNullOrEmpty(searchModel.Name))
            {
                query = query.Where(p => p.Name.Contains(searchModel.Name));
            }

            if (!string.IsNullOrEmpty(searchModel.CategoryId))
            {
                query = query.Where(p => p.CategoryId == searchModel.CategoryId);
            }

            if (!string.IsNullOrEmpty(searchModel.ShopId))
            {
                query = query.Where(p => p.ShopId == searchModel.ShopId);
            }

            if (!string.IsNullOrEmpty(searchModel.Status))
            {
                query = query.Where(p => p.Status == searchModel.Status);
            }

            if (searchModel.MinRating.HasValue)
            {
                query = query.Where(p => p.Rating >= searchModel.MinRating.Value);
            }


            // Sort Logic
            if (searchModel.SortByPrice)
            {
                query = searchModel.SortDescending
                    ? query.OrderByDescending(p => p.ProductItems.Min(pi => pi.Price))
                    : query.OrderBy(p => p.ProductItems.Min(pi => pi.Price));
            }

            else
            {
                query = searchModel.SortDescending
                    ? query.OrderByDescending(p => p.Rating)
                    : query.OrderBy(p => p.Rating);
            }



            var productResponseModels = await query
                .GroupBy(p => new
                {
                    p.Id,
                    p.Name,
                    p.Description,
                    p.CategoryId,
                    p.ShopId,
                    p.Rating,
                    p.Status,
                    p.SoldCount
                })
                .Select(g => new ProductResponseModel
                {
                    Id = g.Key.Id,
                    Name = g.Key.Name,
                    Description = g.Key.Description,
                    CategoryId = g.Key.CategoryId,
                    ShopId = g.Key.ShopId,
                    Rating = g.Key.Rating,
                    Status = g.Key.Status,
                    SoldCount = g.Key.SoldCount,
                    // Avoid duplicates
                    Price = g.SelectMany(p => p.ProductItems).Any() ? g.SelectMany(p => p.ProductItems).Min(pi => pi.Price) : 0
                }).OrderBy(pr => searchModel.SortByPrice
                    ? (searchModel.SortDescending ? -pr.Price : pr.Price) // Sort by price ascending or descending
                    : (searchModel.SortDescending ? -pr.Rating : pr.Rating)) // Sort by rating ascending or descending
                .ToListAsync();

            return BaseResponse<IEnumerable<ProductResponseModel>>.OkResponse(productResponseModels);

        }


        // Sort Function

        public async Task<BaseResponse<IEnumerable<ProductResponseModel>>> SortProductsAsync(ProductSortModel sortModel)
        {
            var query = _unitOfWork.GetRepository<Product>().Entities
                .Include(p => p.ProductItems)
                .Include(p => p.Reviews)
                .AsQueryable();

            // Sort by Price
            if (sortModel.SortByPrice)
            {
                query = sortModel.SortDescending
                    ? query.OrderByDescending(p => p.ProductItems.Min(pi => pi.Price))
                    : query.OrderBy(p => p.ProductItems.Min(pi => pi.Price));
            }

            // Sort by Rating
            else if (sortModel.SortByRating)
            {
                query = sortModel.SortDescending
                    ? query.OrderByDescending(p => p.Rating)
                    : query.OrderBy(p => p.Rating);
            }

            var products = await query.ToListAsync();

            var productResponseModels = products.Select(p => new ProductResponseModel
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                CategoryId = p.CategoryId,
                ShopId = p.ShopId,
                Rating = p.Rating,
                Status = p.Status,
                SoldCount = p.SoldCount,
                Price = p.ProductItems.Any() ? p.ProductItems.Min(pi => pi.Price) : 0
            });

            return BaseResponse<IEnumerable<ProductResponseModel>>.OkResponse(productResponseModels);

        }

    }
}
