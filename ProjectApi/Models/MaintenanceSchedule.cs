using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjectApi.Models
{
    
        public class MaintenanceSchedule
        {
            public int maintenance_id { get; set; }
            public int item_id { get; set; }
            public string scheduled_date { get; set; }
            public string status { get; set; } // "Pending", "In Progress", "Done"
            public string notes { get; set; }
            public string assigned_to { get; set; }
            public string completed_date { get; set; }
        
    }

}