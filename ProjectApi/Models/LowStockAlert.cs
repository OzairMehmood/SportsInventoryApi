using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjectApi.Models
{
   
        public class LowStockAlert
        {
            public int alert_id { get; set; }
            public int item_id { get; set; }
            public string generated_on { get; set; }
            public int threshold_level { get; set; }
            public int current_quantity { get; set; }
            public bool resolved { get; set; }
        }
    

}