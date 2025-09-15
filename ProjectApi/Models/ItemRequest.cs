using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjectApi.Models
{
    public class ItemRequest
    {
        public int request_id { get; set; }
        public string user_id { get; set; }
        public int item_id { get; set; }
        public int quantity { get; set; }
        public string purpose { get; set; }

        // Optional fields — use nullables
        public string? status { get; set; }
        public string? request_date { get; set; }
        public string? approval_date { get; set; }
        public string? return_date { get; set; }
    }


}
