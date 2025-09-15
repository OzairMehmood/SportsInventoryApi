using Microsoft.AspNetCore.Mvc;
using ProjectApi.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace ProjectApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MaintenanceScheduleController : ControllerBase
    {
        DBAccess db = new DBAccess();

        [HttpGet("GetAll")]
        public ActionResult<IEnumerable<MaintenanceSchedule>> GetMaintenance()
        {
            List<MaintenanceSchedule> list = new List<MaintenanceSchedule>();
            db.OpenConnection();
            var reader = db.GetData("SELECT * FROM Maintenance_Schedule");

            while (reader.Read())
            {
                list.Add(new MaintenanceSchedule
                {
                    maintenance_id = int.Parse(reader["maintenance_id"].ToString()),
                    item_id = int.Parse(reader["item_id"].ToString()),
                    scheduled_date = reader["scheduled_date"].ToString(),
                    status = reader["status"].ToString(),
                    notes = reader["notes"]?.ToString(),
                    assigned_to = reader["assigned_to"]?.ToString(),
                    completed_date = reader["completed_date"]?.ToString()
                });
            }

            reader.Close();
            db.CloseConnection();
            return Ok(list);
        }

        [HttpPost("Add")]
        public IActionResult AddMaintenance([FromBody] MaintenanceSchedule m)
        {
            string q = $@"
                INSERT INTO Maintenance_Schedule 
                (item_id, scheduled_date, status, notes, assigned_to, completed_date)
                VALUES 
                ('{m.item_id}', '{m.scheduled_date}', '{m.status}', 
                 {(m.notes != null ? $"'{m.notes}'" : "NULL")},
                 {(m.assigned_to != null ? $"'{m.assigned_to}'" : "NULL")},
                 {(m.completed_date != null ? $"'{m.completed_date}'" : "NULL")})
            ";

            db.OpenConnection();
            db.InsertUpdateDelete(q);
            db.CloseConnection();
            return Ok("Maintenance Added.");
        }

        [HttpPost("ScheduleMaintenance")]
        public IActionResult ScheduleMaintenance([FromBody] MaintenanceSchedule m)
        {
            try
            {
                db.OpenConnection();

                string query = @"
                    INSERT INTO Maintenance_Schedule 
                    (item_id, scheduled_date, status, notes, assigned_to) 
                    VALUES 
                    (@item_id, @scheduled_date, @status, @notes, @assigned_to)
                ";

                using (SqlCommand cmd = new SqlCommand(query, db.con))
                {
                    cmd.Parameters.AddWithValue("@item_id", m.item_id);
                    cmd.Parameters.AddWithValue("@scheduled_date", DateTime.Parse(m.scheduled_date));
                    cmd.Parameters.AddWithValue("@status", m.status ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@notes", string.IsNullOrEmpty(m.notes) ? (object)DBNull.Value : m.notes);
                    cmd.Parameters.AddWithValue("@assigned_to", string.IsNullOrEmpty(m.assigned_to) ? (object)DBNull.Value : m.assigned_to);

                    cmd.ExecuteNonQuery();
                }

                db.CloseConnection();
                return Ok("Maintenance scheduled successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
