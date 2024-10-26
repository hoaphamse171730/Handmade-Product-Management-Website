using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Store;
using HandmadeProductManagement.ModelViews.OrderModelViews;
using HandmadeProductManagement.ModelViews.ProductModelViews;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace UI.Pages.Order
{
    public class OrderHistoryModel : PageModel
    {
        private readonly ApiResponseHelper _apiResponseHelper;

        public OrderHistoryModel(ApiResponseHelper apiResponseHelper)
        {
            _apiResponseHelper = apiResponseHelper ?? throw new ArgumentNullException(nameof(apiResponseHelper));
        }

        public List<OrderByUserDto>? Orders { get; set; }

        public async Task OnGetAsync()
        {
            var response = await _apiResponseHelper.GetAsync<List<OrderByUserDto>>(Constants.ApiBaseUrl + "/api/order/user");

            if (response?.StatusCode == StatusCodeHelper.OK && response.Data != null)
            {
                Orders = response.Data.OrderByDescending(o => o.OrderDate).ToList();
            }
            else
            {
                ModelState.AddModelError(string.Empty, response?.Message ?? "An error occurred while fetching orders.");
            }
        }
    }
}