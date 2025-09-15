using Microsoft.AspNetCore.Mvc;
using ProjectApi.Models;

namespace ProjectApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LowStockAlertsController : ControllerBase
    {
        DBAccess db = new DBAccess();

        [HttpGet("GetAll")]
        public ActionResult<IEnumerable<LowStockAlert>> GetAlerts()
        {
            List<LowStockAlert> list = new List<LowStockAlert>();
            db.OpenConnection();
            var reader = db.GetData("SELECT * FROM Low_Stock_Alerts");

            while (reader.Read())
            {
                list.Add(new LowStockAlert
                {
                    alert_id = int.Parse(reader["alert_id"].ToString()),
                    item_id = int.Parse(reader["item_id"].ToString()),
                    generated_on = reader["generated_on"].ToString(),
                    threshold_level = int.Parse(reader["threshold_level"].ToString()),
                    current_quantity = int.Parse(reader["current_quantity"].ToString()),
                    resolved = Convert.ToBoolean(reader["resolved"])
                });
            }

            reader.Close();
            db.CloseConnection();
            return Ok(list);
        }

        [HttpPost("Add")]
        public IActionResult AddAlert([FromBody] LowStockAlert a)
        {
            string q = $@"
                INSERT INTO Low_Stock_Alerts (item_id, generated_on, threshold_level, current_quantity, resolved)
                VALUES ({a.item_id}, GETDATE(), {a.threshold_level}, {a.current_quantity}, {(a.resolved ? 1 : 0)})
            ";

            db.OpenConnection();
            db.InsertUpdateDelete(q);
            db.CloseConnection();

            return Ok("Alert Added.");
        }
    }
}
