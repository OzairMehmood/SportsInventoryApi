using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjectApi.Models
{
    public class ReportSummary
    {
        public int TotalItems { get; set; }
        public int PendingRequests { get; set; }
        public int ApprovedRequests { get; set; }
        public int TotalUsageRecords { get; set; }
        public int PendingMaintenance { get; set; }
        public int LowStockAlerts { get; set; }
    }
}