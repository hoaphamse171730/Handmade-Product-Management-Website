using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandmadeProductManagement.ModelViews.NotificationModelViews
{
    public class NotificationModel
    {
        public string Id { get; set; }
        public string? Message { get; set; }

        public string? Tag { get; set; }
        public string? URL { get; set; }
    }
}
