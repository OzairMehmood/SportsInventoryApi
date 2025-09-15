using Microsoft.AspNetCore.Mvc;
using ProjectApi.Models;
using System.Data.SqlClient;

namespace ProjectApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        [HttpGet("GetSummary")]
        public ActionResult<ReportSummary> GetSummary()
        {
            var summary = new ReportSummary();
            DBAccess db = new DBAccess();
            db.OpenConnection();

            // Inventory count
            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Inventory_Items", db.con))
                summary.TotalItems = (int)cmd.ExecuteScalar();

            // Requests count by status
            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Item_Requests WHERE status = 'Pending'", db.con))
                summary.PendingRequests = (int)cmd.ExecuteScalar();

            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Item_Requests WHERE status = 'Approved'", db.con))
                summary.ApprovedRequests = (int)cmd.ExecuteScalar();

            // Usage count
            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Usage_History", db.con))
                summary.TotalUsageRecords = (int)cmd.ExecuteScalar();

            // Maintenance
            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Maintenance_Schedule WHERE status = 'Pending'", db.con))
                summary.PendingMaintenance = (int)cmd.ExecuteScalar();

            // Low Stock
            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Low_Stock_Alerts WHERE resolved = 0", db.con))
                summary.LowStockAlerts = (int)cmd.ExecuteScalar();

            db.CloseConnection();
            return Ok(summary);
        }
    }
}
