using HandmadeProductManagement.Core.Store;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HandmadeProductManagement.ModelViews.PromotionModelViews;
using HandmadeProductManagement.Core.Common;
using System.Net;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Constants;

namespace UI.Pages.Promotion
{
    public class DeletedPromotionModel : PageModel
    {
        private readonly ApiResponseHelper _apiHelper;

        public DeletedPromotionModel(ApiResponseHelper apiHelper)
        {
            _apiHelper = apiHelper;
        }

        public List<PromotionDto> DeletedPromotions { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        public async Task OnGetAsync(int pageNumber = 1, int pageSize = 10)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;

            var response = await _apiHelper.GetAsync<List<PromotionDto>>($"{Constants.ApiBaseUrl}/api/promotions/GetDeletedPromotions?pageNumber={PageNumber}&pageSize={PageSize}");
            if (response != null && response.Data != null)
            {
                DeletedPromotions = response.Data;
            }
            else
            {
                DeletedPromotions = new List<PromotionDto>();
            }
        }

        [BindProperty]
        public string promotionIdToRecover { get; set; }

        public async Task<IActionResult> OnPostRecoverAsync()
        {
            var url = $"{Constants.ApiBaseUrl}/api/promotions/{promotionIdToRecover}/recover";
            var response = await _apiHelper.PutAsync<bool>(url);

            if (response == null || response.StatusCode != StatusCodeHelper.OK)
            {
                ModelState.AddModelError(string.Empty, "Unable to activate the promotion. Please try again.");
            }

            return RedirectToPage("/Promotion/DeletedPromotions");
        }
    }
}
