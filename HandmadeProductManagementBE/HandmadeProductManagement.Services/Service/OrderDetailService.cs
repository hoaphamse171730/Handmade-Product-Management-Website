using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.ModelViews.OrderDetailModelViews;
using AutoMapper;
using FluentValidation;
using HandmadeProductManagement.Contract.Repositories.Entity;
using Microsoft.EntityFrameworkCore;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Constants;

namespace HandmadeProductManagement.Services.Service
{
    public class OrderDetailService : IOrderDetailService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<OrderDetailForCreationDto> _creationValidator;
        private readonly IValidator<OrderDetailForUpdateDto> _updateValidator;

        public OrderDetailService(IUnitOfWork unitOfWork, IMapper mapper, IValidator<OrderDetailForCreationDto> creationValidator, IValidator<OrderDetailForUpdateDto> updateValidator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _creationValidator = creationValidator;
            _updateValidator = updateValidator;
        }

        public async Task<IList<OrderDetailDto>> GetAll()
        {
            var orderDetails = await _unitOfWork.GetRepository<OrderDetail>().Entities.Where(p => p.DeletedTime == null).ToListAsync();
            return _mapper.Map<IList<OrderDetailDto>>(orderDetails);
        }

        public async Task<OrderDetailDto> GetById(string id)
        {
            var orderDetail = await _unitOfWork.GetRepository<OrderDetail>().Entities.FirstOrDefaultAsync(p => p.Id == id && p.DeletedTime == null) ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), "Order Detail not found");
            return _mapper.Map<OrderDetailDto>(orderDetail);
        }

        public async Task<OrderDetailDto> Create(OrderDetailForCreationDto orderDetailForCreation, string userId)
        {
            var validationResult = await _creationValidator.ValidateAsync(orderDetailForCreation);
            if (!validationResult.IsValid)
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), validationResult.Errors.First().ErrorMessage);

            var orderDetailEntity = _mapper.Map<OrderDetail>(orderDetailForCreation);
            orderDetailEntity.CreatedBy = userId;
            orderDetailEntity.LastUpdatedBy = userId;

            await _unitOfWork.GetRepository<OrderDetail>().InsertAsync(orderDetailEntity);
            await _unitOfWork.SaveAsync();

            return _mapper.Map<OrderDetailDto>(orderDetailEntity);
        }
    }
}
