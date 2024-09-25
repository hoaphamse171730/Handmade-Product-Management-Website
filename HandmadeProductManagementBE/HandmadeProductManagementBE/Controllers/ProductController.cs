using Azure;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.ProductModelViews;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet("search")]
        public async Task<IActionResult> searchProducts([FromQuery] ProductSearchModel searchModel){
            var response = await _productService.SearchProductsAsync(searchModel);
            if (response.StatusCode == StatusCodeHelper.OK)
            {
                return Ok(response);
            }

            return StatusCode((int)response.StatusCode, response);

        }

        [HttpGet("sort")]
        public async Task<IActionResult> SortProducts([FromQuery] ProductSortModel sortModel)
        {
            var response = await _productService.SortProductsAsync(sortModel);
            if (response.StatusCode == StatusCodeHelper.OK)
            {
                return Ok(response);
            }
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _service.GetAll();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var product = await _service.GetById(id);
            if (product == null)
                return NotFound();
            return Ok(product);
        }


        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] ProductDto productDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var createdProduct = await _service.Add(productDto);
            return CreatedAtAction(nameof(GetById), new { id = createdProduct.Id }, createdProduct);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(string id, [FromBody] ProductDto productDto)
        {
            await _service.UpdateProductAsync(id, productDto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            try
            {
                await _service.Delete(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }


    }
}

