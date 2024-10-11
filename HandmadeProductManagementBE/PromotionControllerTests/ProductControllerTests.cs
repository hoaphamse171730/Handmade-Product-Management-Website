using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.ModelViews.ProductDetailModelViews;
using HandmadeProductManagement.ModelViews.ProductModelViews;
using HandmadeProductManagementAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ControllerTests
{
    public class ProductControllerTests
    {
        private readonly Mock<IProductService> _productServiceMock;
        private readonly ProductController _productController;

        public ProductControllerTests()
        {
            _productServiceMock = new Mock<IProductService>();
            _productController = new ProductController(_productServiceMock.Object);
        }

       
        [Fact]
        public async Task GetProduct_ReturnsOkResult_WithSingleProduct()
        {
            var product = new ProductDto { Id = "1", Name = "Product 1" };
            _productServiceMock.Setup(service => service.GetById("1"))
                .ReturnsAsync(product);
            var result = await _productController.GetProduct("1");
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<BaseResponse<ProductDto>>(okResult.Value);
            Assert.Equal("200", response.Code);
            Assert.Equal(product, response.Data);
        }

        [Fact]
        public async Task GetProductDetails_ReturnsOkResult_WithProductDetails()
        {
            var productDetails = new ProductDetailResponseModel { Id = "1", Description = "Product 1 details" };
            _productServiceMock.Setup(service => service.GetProductDetailsByIdAsync("1"))
                .ReturnsAsync(productDetails);
            var result = await _productController.GetProductDetails("1");
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<BaseResponse<ProductDetailResponseModel>>(okResult.Value);
            Assert.Equal("200", response.Code);
            Assert.Equal(productDetails, response.Data);
        }

        [Fact]
        public async Task CalculateAverageRating_ReturnsOkResult_WithAverageRating()
        {
            var averageRating = 4.5m;
            _productServiceMock.Setup(service => service.CalculateAverageRatingAsync("1"))
                .ReturnsAsync(averageRating);
            var result = await _productController.CalculateAverageRating("1");
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<BaseResponse<decimal>>(okResult.Value);
            Assert.Equal("200", response.Code);
            Assert.Equal(averageRating, response.Data);
        }
    }
}