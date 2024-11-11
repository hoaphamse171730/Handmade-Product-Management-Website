using HandmadeProductManagement.Core.Store;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HandmadeProductManagement.ModelViews.PromotionModelViews;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Base;
namespace UI.Pages.Promotion
{
    public class PromotionModel : PageModel
    {
        private readonly ApiResponseHelper _apiHelper;

        public PromotionModel(ApiResponseHelper apiHelper)
        {
            _apiHelper = apiHelper;
        }

        public List<PromotionDto> Promotions { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ErrorDetail { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 2;
        public bool HasNextPage { get; set; } = true;

        public async Task<IActionResult> OnGetAsync(int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                PageNumber = pageNumber;
                PageSize = pageSize;

                var response = await _apiHelper.GetAsync<List<PromotionDto>>($"{Constants.ApiBaseUrl}/api/promotions?pageNumber={PageNumber}&pageSize={PageSize}");
                if (response != null && response.Data != null)
                {
                    Promotions = response.Data;
                    HasNextPage = Promotions.Count == PageSize;
                }
                else
                {
                    Promotions = new List<PromotionDto>();
                }
            }
            catch (BaseException.ErrorException ex)
            {
                ErrorMessage = ex.ErrorDetail.ErrorCode;
                ErrorDetail = ex.ErrorDetail.ErrorMessage?.ToString();
                if (ErrorMessage == "unauthorized") return RedirectToPage("/Login");
            }
            catch (Exception ex)
            {
                ErrorMessage = "An unexpected error occurred.";
            }
            return Page();
        }
    }
}
