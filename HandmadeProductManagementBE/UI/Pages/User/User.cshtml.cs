    using HandmadeProductManagement.Core.Common;
    using HandmadeProductManagement.Core.Constants;
    using HandmadeProductManagement.Core.Store;
    using HandmadeProductManagement.ModelViews.UserModelViews;
    using HotChocolate.Language;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;

    namespace UI.Pages.User
    {
        public class UserModel : PageModel
        {
            private readonly ApiResponseHelper _apiResponseHelper;

            public UserModel(ApiResponseHelper apiResponseHelper)
            {
                _apiResponseHelper = apiResponseHelper;
            }
            
            public int PageNumber { get; set; } = 1;
            public List<UserResponseModel> users { get; set; }
            public int PageSize { get; set; } = 12;
            public string SearchName { get; set; }
            public string SearchPhone { get; set; }
            public async Task OnGet(int pageNumber = 1, int pageSize = 10, string? searchName = null, string? searchPhone = null )
            {
            PageNumber = pageNumber;
            PageSize = pageSize;
            SearchName = searchName;
            SearchPhone = searchPhone;
            users = new List<UserResponseModel>();

            var queryParams = new Dictionary<string, string>
            {
                {"PageNumber", PageNumber.ToString() },
                {"PageSize", PageSize.ToString() },
                {"userName", SearchName },
                {"phoneNumber", SearchPhone }
            };

            var queryString = string.Join("&", queryParams.Where(x => !string.IsNullOrEmpty(x.Value)).Select(x=> $"{x.Key}={x.Value}"));
            var url = $"{Constants.ApiBaseUrl}/api/users?{queryString}";
            var response = await _apiResponseHelper.GetAsync<List<UserResponseModel>>(url);

                 if (response.StatusCode == StatusCodeHelper.OK && response.Data != null)
                {
                    users = response.Data;
                }
                 else
                {
                    ModelState.AddModelError(string.Empty, response.Message ?? "An error occurred while fetching users.");
                }
            }
        }
    }
