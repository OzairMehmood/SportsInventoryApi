using Microsoft.AspNetCore.Mvc;
using ProjectApi.Models;
using System.Data.SqlClient;

namespace ProjectApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemRequestsController : ControllerBase
    {
        DBAccess db = new DBAccess();

        [HttpGet("GetAllRequests")]
        public IActionResult GetAllRequests()
        {
            List<ItemRequest> requests = new List<ItemRequest>();
            db.OpenConnection();

            string query = @"SELECT request_id, user_id, item_id, quantity, purpose, status, request_date FROM Item_Requests";
            var sdr = db.GetData(query);
            while (sdr.Read())
            {
                requests.Add(new ItemRequest
                {
                    request_id = Convert.ToInt32(sdr["request_id"]),
                    user_id = sdr["user_id"].ToString(),
                    item_id = Convert.ToInt32(sdr["item_id"]),
                    quantity = Convert.ToInt32(sdr["quantity"]),
                    purpose = sdr["purpose"].ToString(),
                    status = sdr["status"].ToString(),
                    request_date = Convert.ToDateTime(sdr["request_date"]).ToString("yyyy-MM-dd HH:mm:ss")
                });
            }

            sdr.Close();
            db.CloseConnection();
            return Ok(requests);
        }
        [HttpPost("RequestItem")]
        public IActionResult RequestItem([FromBody] ItemRequest req)
        {
            try
            {
                db.OpenConnection();

                // Handle missing fields with default values
                string status = string.IsNullOrWhiteSpace(req.status) ? "Pending" : req.status;
                string requestDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                string query = $@"
            INSERT INTO Item_Requests (user_id, item_id, quantity, purpose, status, request_date)
            VALUES ('{req.user_id}', {req.item_id}, {req.quantity}, '{req.purpose}', '{status}', '{requestDate}')";

                db.InsertUpdateDelete(query);
                db.CloseConnection();

                return Ok("Item requested successfully.");
            }
            catch (Exception ex)
            {
                db.CloseConnection();
                return BadRequest($"Error: {ex.Message}");
            }
        }



        [HttpPost("ApproveRequest/{id}")]
        public IActionResult ApproveRequest(int id)
        {
            db.OpenConnection();

            string getReqQuery = $"SELECT * FROM Item_Requests WHERE request_id = {id}";
            var reader = db.GetData(getReqQuery);

            if (!reader.Read())
            {
                db.CloseConnection();
                return NotFound();
            }

            int itemId = Convert.ToInt32(reader["item_id"]);
            int quantity = Convert.ToInt32(reader["quantity"]);
            string userId = reader["user_id"].ToString();
            reader.Close();

            string updateReqQuery = $"UPDATE Item_Requests SET status = 'Approved', approval_date = GETDATE() WHERE request_id = {id}";
            db.InsertUpdateDelete(updateReqQuery);

            string updateQtyQuery = $"UPDATE Inventory_Items SET quantity_available = quantity_available - {quantity} WHERE item_id = {itemId}";
            db.InsertUpdateDelete(updateQtyQuery);

            string insertUsageQuery = $@"
                INSERT INTO Usage_History (user_id, item_id, quantity, start_date)
                VALUES ('{userId}', {itemId}, {quantity}, GETDATE())";
            db.InsertUpdateDelete(insertUsageQuery);

            string checkQtyQuery = $"SELECT quantity_available FROM Inventory_Items WHERE item_id = {itemId}";
            var qtyReader = db.GetData(checkQtyQuery);
            if (qtyReader.Read())
            {
                int currentQty = Convert.ToInt32(qtyReader["quantity_available"]);
                int threshold = 5;

                if (currentQty <= threshold)
                {
                    string insertAlert = $@"
                        INSERT INTO Low_Stock_Alerts (item_id, threshold_level, current_quantity, resolved)
                        VALUES ({itemId}, {threshold}, {currentQty}, 0)";
                    db.InsertUpdateDelete(insertAlert);
                }
            }
            qtyReader.Close();
            db.CloseConnection();

            return Ok("Request Approved and processed.");
        }

        [HttpPost("DenyRequest/{id}")]
        public IActionResult DenyRequest(int id)
        {
            db.OpenConnection();

            string updateReqQuery = $"UPDATE Item_Requests SET status = 'Denied', approval_date = GETDATE() WHERE request_id = {id}";
            db.InsertUpdateDelete(updateReqQuery);

            db.CloseConnection();
            return Ok("Request Denied.");
        }

        [HttpGet("GetRequestsByUser/{userId}")]
        public IActionResult GetRequestsByUser(string userId)
        {
            List<ItemRequest> requests = new List<ItemRequest>();
            db.OpenConnection();
            string query = $@"
                SELECT * FROM Item_Requests 
                WHERE user_id = '{userId}' 
                ORDER BY request_date DESC";

            var sdr = db.GetData(query);
            while (sdr.Read())
            {
                requests.Add(new ItemRequest
                {
                    request_id = Convert.ToInt32(sdr["request_id"]),
                    user_id = sdr["user_id"].ToString(),
                    item_id = Convert.ToInt32(sdr["item_id"]),
                    quantity = Convert.ToInt32(sdr["quantity"]),
                    request_date = sdr["request_date"].ToString(),
                    status = sdr["status"].ToString(),
                    purpose = sdr["purpose"].ToString()
                });
            }

            sdr.Close();
            db.CloseConnection();
            return Ok(requests);
        }

        [HttpGet("/api/Usage/GetUsageByUser/{userId}")]
        public IActionResult GetUsageByUser(string userId)
        {
            List<UsageHistory> usageList = new List<UsageHistory>();
            db.OpenConnection();

            string query = $@"
                SELECT * FROM Usage_History 
                WHERE user_id = '{userId}' 
                ORDER BY start_date DESC";

            var sdr = db.GetData(query);
            while (sdr.Read())
            {
                usageList.Add(new UsageHistory
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
            return Ok(usageList);
        }
    }
}
