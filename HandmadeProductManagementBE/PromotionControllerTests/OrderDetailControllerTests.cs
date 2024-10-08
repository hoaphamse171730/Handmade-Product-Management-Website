using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.ModelViews.OrderDetailModelViews;
using HandmadeProductManagementAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ControllerTests
{
    public class OrderDetailControllerTests
    {
        private readonly Mock<IOrderDetailService> _orderDetailServiceMock;
        private readonly OrderDetailController _orderDetailController;

        public OrderDetailControllerTests()
        {
            _orderDetailServiceMock = new Mock<IOrderDetailService>();
            _orderDetailController = new OrderDetailController(_orderDetailServiceMock.Object);
        }

        [Fact]
        public async Task GetOrderDetails_ReturnsOkResult_WithListOfOrderDetails()
        {
            var orderDetails = new List<OrderDetailDto>
            {
                new OrderDetailDto { Id = "1", OrderId = "1", ProductItemId = "A",  ProductQuantity = 6},
                new OrderDetailDto { Id = "2", OrderId = "1", ProductItemId = "B", ProductQuantity = 5}
            };
            _orderDetailServiceMock.Setup(service => service.GetAll())
                .ReturnsAsync(orderDetails);
            var result = await _orderDetailController.GetOrderDetails();
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<BaseResponse<IList<OrderDetailDto>>>(okResult.Value);
            Assert.Equal("200", response.Code);
            Assert.Equal(orderDetails, response.Data);
        }

        [Fact]
        public async Task GetOrderDetail_ReturnsOkResult_WithSingleOrderDetail()
        {
            var orderDetail = new OrderDetailDto { Id = "1", OrderId = "1", ProductItemId = "A", ProductQuantity = 2 };
            _orderDetailServiceMock.Setup(service => service.GetById("1"))
                .ReturnsAsync(orderDetail);
            var result = await _orderDetailController.GetOrderDetail("1");
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<BaseResponse<OrderDetailDto>>(okResult.Value);
            Assert.Equal("200", response.Code);
            Assert.Equal(orderDetail, response.Data);
        }

    }
}
