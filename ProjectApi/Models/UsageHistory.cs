using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjectApi.Models
{
    public class UsageHistory
    {
        public int usage_id { get; set; }
        public string user_id { get; set; }
        public int item_id { get; set; }
        public string item_name { get; set; }
        public string status { get; set; }
        public int quantity { get; set; }
        public string start_date { get; set; }
        public string end_date { get; set; }
        public string condition_on_return { get; set; }
    }
}
