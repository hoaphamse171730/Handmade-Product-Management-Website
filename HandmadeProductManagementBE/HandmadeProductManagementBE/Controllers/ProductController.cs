using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.ModelViews.ProductModelViews;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HandmadeProductManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            IList<Product> products = await _productService.GetAll();
            return Ok(BaseResponse<IList<Product>>.OkResponse(products));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(string id)
        {
            Product product = await _productService.GetById(id);
            return Ok(BaseResponse<Product>.OkResponse(product));
        }

        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(Product product)
        {
            Product createdProduct = await _productService.Create(product);
            return CreatedAtAction(nameof(GetProduct), new { id = createdProduct.Id }, createdProduct);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Product>> UpdateProduct(string id, Product updatedProduct)
        {
            Product product = await _productService.Update(id, updatedProduct);
            return Ok(BaseResponse<Product>.OkResponse(product));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProduct(string id)
        {
            bool success = await _productService.Delete(id);
            if (!success)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpPost("Search")]
        public async Task<ActionResult<BaseResponse<IEnumerable<ProductResponseModel>>>> SearchProducts(ProductSearchModel searchModel)
        {
            var response = await _productService.SearchProductsAsync(searchModel);
            return Ok(response);
        }

        [HttpPost("Sort")]
        public async Task<ActionResult<BaseResponse<IEnumerable<ProductResponseModel>>>> SortProducts(ProductSortModel sortModel)
        {
            var response = await _productService.SortProductsAsync(sortModel);
            return Ok(response);
        }
    }
}
