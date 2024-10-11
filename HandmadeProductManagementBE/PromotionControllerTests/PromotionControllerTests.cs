using FluentAssertions;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.PromotionModelViews;
using HandmadeProductManagementAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ControllerTests
{
    public class PromotionsControllerTests
    {
        private readonly Mock<IPromotionService> _mockPromotionService;
        private readonly PromotionsController _controller;

        public PromotionsControllerTests()
        {
            _mockPromotionService = new Mock<IPromotionService>();
            _controller = new PromotionsController(_mockPromotionService.Object);
        }

        [Fact]
        public async Task GetPromotions_ReturnsOkResult_WithPromotions()
        {
            var promotions = new List<PromotionDto>
            {
                new PromotionDto
                {
                    Id = "promo1",
                    Name = "New Year Sale",
                    Description = "Discount for New Year holiday",
                    DiscountRate = 0.15m,
                    StartDate = new DateTime(2024, 12, 20),
                    EndDate = new DateTime(2025, 1, 5),
                },
                new PromotionDto
                {
                    Id = "promo2",
                    Name = "Summer Sale",
                    Description = "Discount on summer items",
                    DiscountRate = 0.20m,
                    StartDate = new DateTime(2024, 6, 1),
                    EndDate = new DateTime(2024, 6, 30),
                }
            };
            var pageNumber = 1;
            var pageSize = 10;
            _mockPromotionService.Setup(service => service.GetAll(pageNumber, pageSize)).ReturnsAsync(promotions);
            var result = await _controller.GetPromotions(pageNumber, pageSize);
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            var response = okResult.Value as BaseResponse<IList<PromotionDto>>;
            response.Should().NotBeNull();
            response.Code.Should().Be("200");
            response.StatusCode.Should().Be(StatusCodeHelper.OK);
            response.Data.Should().BeEquivalentTo(promotions);
        }

        [Fact]
        public async Task GetPromotion_ReturnsOkResult_WithPromotion()
        {
            // Arrange
            var promotionId = "promo1";
            var promotion = new PromotionDto
            {
                Id = "promo1",
                Name = "New Year Sale",
                Description = "Discount for New Year holiday",
                DiscountRate = 0.15m,
                StartDate = new DateTime(2024, 12, 20),
                EndDate = new DateTime(2025, 1, 5),
            };
            _mockPromotionService.Setup(service => service.GetById(promotionId)).ReturnsAsync(promotion);
            var result = await _controller.GetPromotion(promotionId);
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            var response = okResult.Value as BaseResponse<PromotionDto>;
            response.Should().NotBeNull();
            response.Code.Should().Be("200");
            response.StatusCode.Should().Be(StatusCodeHelper.OK);
            response.Data.Should().BeEquivalentTo(promotion);
        }

        [Fact]
        public async Task CreatePromotion_ReturnsOkResult_OnSuccess()
        {
            var promotionForCreation = new PromotionForCreationDto
            {
                Name = "New Year Sale",
                Description = "Discount for New Year holiday",
                DiscountRate = 0.15m,
                StartDate = new DateTime(2024, 12, 20),
                EndDate = new DateTime(2025, 1, 5)
            };
            var result = true;
            _mockPromotionService.Setup(service => service.Create(promotionForCreation)).ReturnsAsync(result);
            var actionResult = await _controller.CreatePromotion(promotionForCreation);
            var okResult = actionResult as OkObjectResult;
            okResult.Should().NotBeNull();
            var response = okResult.Value as BaseResponse<bool>;
            response.Should().NotBeNull();
            response.Code.Should().Be("200");
            response.StatusCode.Should().Be(StatusCodeHelper.OK);
            response.Data.Should().BeTrue();
        }

        [Fact]
        public async Task UpdatePromotion_ReturnsOkResult_OnSuccess()
        {
            var promotionId = "promo1";
            var promotionForUpdate = new PromotionForUpdateDto
            {
                Name = "Updated Sale",
                Description = "Updated description",
                DiscountRate = 0.10m,
                StartDate = new DateTime(2025, 1, 1),
                EndDate = new DateTime(2025, 1, 10)
            };
            var result = true;
            _mockPromotionService.Setup(service => service.Update(promotionId, promotionForUpdate))
                .ReturnsAsync(result);
            var actionResult = await _controller.UpdatePromotion(promotionId, promotionForUpdate);
            var okResult = actionResult as OkObjectResult;
            okResult.Should().NotBeNull();
            var response = okResult.Value as BaseResponse<bool>;
            response.Should().NotBeNull();
            response.Code.Should().Be("200");
            response.StatusCode.Should().Be(StatusCodeHelper.OK);
            response.Data.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeletePromotion_ReturnsOkResult_OnSuccess()
        {
            var promotionId = "promo1";
            var result = true;
            _mockPromotionService.Setup(service => service.SoftDelete(promotionId)).ReturnsAsync(result);
            var actionResult = await _controller.SoftDeletePromotion(promotionId);
            var okResult = actionResult as OkObjectResult;
            okResult.Should().NotBeNull();
            var response = okResult.Value as BaseResponse<bool>;
            response.Should().NotBeNull();
            response.Code.Should().Be("200");
            response.StatusCode.Should().Be(StatusCodeHelper.OK);
            response.Data.Should().BeTrue();
        }
    }
}