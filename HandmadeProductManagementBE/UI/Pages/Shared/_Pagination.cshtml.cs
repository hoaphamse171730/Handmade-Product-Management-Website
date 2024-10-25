using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UI.Pages.Shared
{        
    public class PaginationModel
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
