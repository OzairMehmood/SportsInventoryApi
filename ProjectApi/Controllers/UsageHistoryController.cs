using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProjectApi.Models;
using System.Data.SqlClient;

namespace ProjectApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsageHistoryController : ControllerBase
    {
        DBAccess db = new DBAccess();

        [HttpGet("GetAll")]
        public ActionResult<IEnumerable<UsageHistory>> GetHistory()
        {
            List<UsageHistory> list = new List<UsageHistory>();
            db.OpenConnection();
            var reader = db.GetData("SELECT * FROM Usage_History");

            while (reader.Read())
            {
                list.Add(new UsageHistory
                {
                    usage_id = int.Parse(reader["usage_id"].ToString()),
                    user_id = reader["user_id"].ToString(),
                    item_id = int.Parse(reader["item_id"].ToString()),
                    quantity = int.Parse(reader["quantity"].ToString()),
                    start_date = reader["start_date"]?.ToString(),
                    end_date = reader["end_date"]?.ToString(),
                    condition_on_return = reader["condition_on_return"]?.ToString()
                });
            }

            reader.Close();
            db.CloseConnection();
            return Ok(list);
        }

        [HttpPost("Add")]
        public IActionResult AddUsage([FromBody] UsageHistory u)
        {
            string q = $@"
                INSERT INTO Usage_History (user_id, item_id, quantity, start_date, end_date, condition_on_return)
                VALUES ('{u.user_id}', '{u.item_id}', '{u.quantity}', 
                        {(u.start_date != null ? $"'{u.start_date}'" : "NULL")},
                        {(u.end_date != null ? $"'{u.end_date}'" : "NULL")},
                        {(u.condition_on_return != null ? $"'{u.condition_on_return}'" : "NULL")})";

            db.OpenConnection();
            db.InsertUpdateDelete(q);
            db.CloseConnection();
            return Ok("Usage Added.");
        }

        [HttpGet("GetUsageByUser/{userId}")]
        public IActionResult GetUsageByUser(string userId)
        {
            List<UsageHistory> usageList = new List<UsageHistory>();
            db.OpenConnection();

            string query = @"
                SELECT uh.*, ii.name AS item_name, ir.status
                FROM Usage_History uh
                JOIN Item_Requests ir ON uh.user_id = ir.user_id AND uh.item_id = ir.item_id
                JOIN Inventory_Items ii ON uh.item_id = ii.item_id
                WHERE uh.user_id = @userId AND ir.status = 'Approved'";

            var command = new SqlCommand(query, db.con);
            command.Parameters.AddWithValue("@userId", userId);

            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                usageList.Add(new UsageHistory
                {
                    usage_id = Convert.ToInt32(reader["usage_id"]),
                    user_id = reader["user_id"].ToString(),
                    item_id = Convert.ToInt32(reader["item_id"]),
                    item_name = reader["item_name"].ToString(),
                    quantity = Convert.ToInt32(reader["quantity"]),
                    start_date = reader["start_date"].ToString(),
                    end_date = reader["end_date"].ToString(),
                    condition_on_return = reader["condition_on_return"].ToString(),
                    status = reader["status"].ToString()
                });
            }

            reader.Close();
            db.CloseConnection();
            return Ok(usageList);
        }

        [HttpPost("ReturnItem")]
        public async Task<IActionResult> ReturnItem([FromForm] IFormCollection form)
        {
            if (!form.ContainsKey("usage_id") || !form.ContainsKey("condition"))
                return BadRequest("Missing data");

            int usage_id = int.Parse(form["usage_id"]);
            string condition = form["condition"];
            DateTime now = DateTime.Now;

            db.OpenConnection();

            // ✅ 1. Update Usage History
            string updateUsageQuery = $@"
                UPDATE Usage_History 
                SET end_date = '{now}', condition_on_return = '{condition}' 
                WHERE usage_id = {usage_id}";
            db.InsertUpdateDelete(updateUsageQuery);

            // ✅ 2. Get item ID and quantity
            string fetchQuery = $"SELECT item_id, quantity FROM Usage_History WHERE usage_id = {usage_id}";
            var reader = db.GetData(fetchQuery);
            int item_id = 0, quantity = 0;

            if (reader.Read())
            {
                item_id = Convert.ToInt32(reader["item_id"]);
                quantity = Convert.ToInt32(reader["quantity"]);
            }
            reader.Close();

            // ✅ 3. Update quantity_available
            string updateInventory = $@"
                UPDATE Inventory_Items 
                SET quantity_available = quantity_available + {quantity} 
                WHERE item_id = {item_id}";
            db.InsertUpdateDelete(updateInventory);

            // ✅ 4. Add maintenance if required
            if (condition.ToLower().Contains("broken") || condition.ToLower().Contains("repair"))
            {
                string insertMaintenance = $@"
                    INSERT INTO Maintenance_Schedule 
                    (item_id, scheduled_date, status) 
                    VALUES ({item_id}, '{now}', 'Pending')";
                db.InsertUpdateDelete(insertMaintenance);
            }

            db.CloseConnection();
            return Ok("Item returned and inventory updated.");
        }
    }
}
