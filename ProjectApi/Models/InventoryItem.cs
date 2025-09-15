using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjectApi.Models
{
    public class InventoryItem
    {
        public int item_id { get; set; }
        public string name { get; set; }
        public int category_id { get; set; }
        public int quantity_total { get; set; }
        public int quantity_available { get; set; }
        public string status { get; set; }
        public bool maintenance_required { get; set; }
        public string image_url { get; set; }
    }
}
