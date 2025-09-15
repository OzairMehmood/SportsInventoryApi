using Microsoft.AspNetCore.Mvc;
using ProjectApi.Models;
using System.Data.SqlClient;

namespace ProjectApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryItemsController : ControllerBase
    {
        DBAccess db = new DBAccess();

        [HttpGet("GetAllItems")]
        public IActionResult GetAllItems()
        {
            List<InventoryItem> items = new List<InventoryItem>();
            db.OpenConnection();
            string query = "SELECT * FROM Inventory_Items";
            var sdr = db.GetData(query);

            while (sdr.Read())
            {
                items.Add(new InventoryItem
                {
                    item_id = Convert.ToInt32(sdr["item_id"]),
                    name = sdr["name"].ToString(),
                    category_id = Convert.ToInt32(sdr["category_id"]),
                    quantity_total = Convert.ToInt32(sdr["quantity_total"]),
                    quantity_available = Convert.ToInt32(sdr["quantity_available"]),
                    status = sdr["status"].ToString(),
                    maintenance_required = Convert.ToBoolean(sdr["maintenance_required"]),
                    image_url = sdr["image_url"].ToString()
                });
            }

            sdr.Close();
            db.CloseConnection();
            return Ok(items);
        }

        [HttpPost("AddItemByCategoryName")]
        public IActionResult AddItemByCategoryName([FromForm] string name,
                                             [FromForm] string category_name,
                                             [FromForm] int quantity_total,
                                             [FromForm] IFormFile ImageFile)
        {
            try
            {
                DBAccess db = new DBAccess();
                db.OpenConnection();

                // Get category_id
                string getCategoryQuery = $"SELECT category_id FROM Categories WHERE category_name = '{category_name}'";
                var reader = db.GetData(getCategoryQuery);

                int category_id = 0;
                if (reader.Read())
                    category_id = Convert.ToInt32(reader["category_id"]);
                reader.Close();

                if (category_id == 0)
                    return BadRequest("Invalid category name.");

                // Save image to physical path
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    ImageFile.CopyTo(stream);
                }

                string image_url = "/images/" + fileName;

                // Insert into DB
                string insertQuery = $@"
        INSERT INTO Inventory_Items (name, category_id, quantity_total, quantity_available, status, maintenance_required, image_url)
        VALUES ('{name}', {category_id}, {quantity_total}, {quantity_total}, 'Available', 0, '{image_url}')";

                db.InsertUpdateDelete(insertQuery);
                db.CloseConnection();

                return Ok("Item added successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }


        [HttpPut("UpdateItem/{id}")]
        public IActionResult UpdateItem(int id, [FromBody] InventoryItem item)
        {
            string q = $@"
                UPDATE Inventory_Items SET
                    name = '{item.name}',
                    category_id = '{item.category_id}',
                    quantity_total = '{item.quantity_total}',
                    quantity_available = '{item.quantity_available}',
                    status = '{item.status}',
                    maintenance_required = '{(item.maintenance_required ? 1 : 0)}',
                    image_url = '{item.image_url}'
                WHERE item_id = '{id}'";

            db.OpenConnection();
            db.InsertUpdateDelete(q);
            db.CloseConnection();

            return Ok("Item updated.");
        }

        [HttpDelete("DeleteItem/{id}")]
        public IActionResult DeleteItem(int id)
        {
            db.OpenConnection();
            string q = $"DELETE FROM Inventory_Items WHERE item_id = {id}";
            db.InsertUpdateDelete(q);
            db.CloseConnection();
            return Ok("Deleted");
        }

        [HttpGet("/api/UsageHistory/GetByUser/{userId}")]
        public IActionResult GetUsageHistoryByUser(string userId)
        {
            List<UsageHistory> history = new List<UsageHistory>();
            db.OpenConnection();

            string query = $"SELECT * FROM Usage_History WHERE user_id = '{userId}'";
            var sdr = db.GetData(query);
            while (sdr.Read())
            {
                history.Add(new UsageHistory
                {
                    usage_id = Convert.ToInt32(sdr["usage_id"]),
                    user_id = sdr["user_id"].ToString(),
                    item_id = Convert.ToInt32(sdr["item_id"]),
                    quantity = Convert.ToInt32(sdr["quantity"]),
                    start_date = sdr["start_date"].ToString(),
                    end_date = sdr["end_date"].ToString(),
                    condition_on_return = sdr["condition_on_return"].ToString()
                });
            }

            sdr.Close();
            db.CloseConnection();
            return Ok(history);
        }

        [HttpGet("/api/Maintenance/GetAll")]
        public IActionResult GetAllMaintenance()
        {
            List<MaintenanceSchedule> list = new List<MaintenanceSchedule>();
            db.OpenConnection();

            string query = "SELECT * FROM Maintenance_Schedule";
            var sdr = db.GetData(query);
            while (sdr.Read())
            {
                list.Add(new MaintenanceSchedule
                {
                    maintenance_id = Convert.ToInt32(sdr["maintenance_id"]),
                    item_id = Convert.ToInt32(sdr["item_id"]),
                    scheduled_date = sdr["scheduled_date"].ToString(),
                    status = sdr["status"].ToString(),
                    notes = sdr["notes"].ToString(),
                    assigned_to = sdr["assigned_to"].ToString(),
                    completed_date = sdr["completed_date"].ToString()
                });
            }

            sdr.Close();
            db.CloseConnection();
            return Ok(list);
        }

        [HttpGet("/api/LowStock/GetAll")]
        public IActionResult GetAllLowStock()
        {
            List<LowStockAlert> alerts = new List<LowStockAlert>();
            db.OpenConnection();

            string query = "SELECT * FROM Low_Stock_Alerts";
            var sdr = db.GetData(query);
            while (sdr.Read())
            {
                alerts.Add(new LowStockAlert
                {
                    alert_id = Convert.ToInt32(sdr["alert_id"]),
                    item_id = Convert.ToInt32(sdr["item_id"]),
                    generated_on = sdr["generated_on"].ToString(),
                    threshold_level = Convert.ToInt32(sdr["threshold_level"]),
                    current_quantity = Convert.ToInt32(sdr["current_quantity"]),
                    resolved = Convert.ToBoolean(sdr["resolved"])
                });
            }

            sdr.Close();
            db.CloseConnection();
            return Ok(alerts);
        }
    }
}
