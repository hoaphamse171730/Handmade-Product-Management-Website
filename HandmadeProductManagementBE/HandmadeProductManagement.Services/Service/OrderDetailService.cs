using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.ModelViews.OrderDetailModelViews;

namespace HandmadeProductManagement.Services.Service;

public class OrderDetailService : IOrderDetailService
{
    private readonly IUnitOfWork _unitOfWork;
    public OrderDetailService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<OrderDetailDto>> GetAllAsync()
    {
        var result = _unitOfWork.GetRepository<Or>()
    }

}