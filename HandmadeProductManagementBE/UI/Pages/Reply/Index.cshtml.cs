using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Store;
using HandmadeProductManagement.ModelViews.ReplyModelViews;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UI.Pages.Reply
{
    public class IndexModel : PageModel
    {
        private readonly ApiResponseHelper _apiResponseHelper;

        public IndexModel(ApiResponseHelper apiResponseHelper)
        {
            _apiResponseHelper = apiResponseHelper;
        }

        public IList<ReplyModel> Replies { get; set; } = new List<ReplyModel>();
        public int PageNumber { get; set; } = 1;
        public int TotalPages { get; set; }
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        public async Task<IActionResult> OnGetAsync(int pageNumber = 1, int pageSize = 10)
        {
            PageNumber = pageNumber;

            var response = await _apiResponseHelper.GetAsync<IList<ReplyModel>>($"{Constants.ApiBaseUrl}/api/reply?pageNumber={pageNumber}&pageSize={pageSize}");

            if (response.StatusCode == StatusCodeHelper.OK && response.Data != null)
            {
                Replies = response.Data;
                var totalPagesResponse = await _apiResponseHelper.GetAsync<int>($"{Constants.ApiBaseUrl}/api/reply/totalpages?pageSize={pageSize}");
                if (totalPagesResponse.StatusCode == StatusCodeHelper.OK)
                {
                    TotalPages = totalPagesResponse.Data;
                }
            }

            return Page();
        }
    }
}
