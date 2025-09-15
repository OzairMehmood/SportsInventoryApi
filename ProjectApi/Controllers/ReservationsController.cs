using Microsoft.AspNetCore.Mvc;
using ProjectApi.Models;
using System.Data.SqlClient;

namespace ProjectApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationsController : ControllerBase
    {
        DBAccess db = new DBAccess();
        [HttpPost("CreateReservation")]
        public IActionResult CreateReservation([FromBody] Reservation reservation)
        {
            try
            {
                db.OpenConnection();

                string query = $@"
            INSERT INTO Item_Reservations 
            (user_id, item_id, duration_value, duration_unit, requested_at, is_approved, is_late)
            VALUES
            ({reservation.user_id}, {reservation.item_id}, {reservation.duration_value}, 
             '{reservation.duration_unit}', GETDATE(), 0, 0)";

                db.InsertUpdateDelete(query);
                db.CloseConnection();

                return Ok("Reservation created successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest("Error: " + ex.Message);
            }
        }

        [HttpGet("GetAllReservations")]
        public IActionResult GetAllReservations()
        {
            List<Reservation> list = new List<Reservation>();
            db.OpenConnection();

            string query = @"Select * from Item_Reservations";

            var reader = db.GetData(query);

            while (reader.Read())
            {
                list.Add(new Reservation
                {
                    reservation_id = Convert.ToInt32(reader["reservation_id"]),
                    user_id = Convert.ToInt32(reader["user_id"]),
                    
                    item_id = Convert.ToInt32(reader["item_id"]),
                    
                    duration_value = Convert.ToInt32(reader["duration_value"]),
                    duration_unit = reader["duration_unit"].ToString(),
                    requested_at = Convert.ToDateTime(reader["requested_at"]),
                    approved_at = reader["approved_at"] == DBNull.Value ? null : Convert.ToDateTime(reader["approved_at"]),
                    is_approved = Convert.ToBoolean(reader["is_approved"]),
                    is_late = Convert.ToBoolean(reader["is_late"])
                });
            }

            reader.Close();
            db.CloseConnection();

            return Ok(list);
        }

        [HttpPost("ApproveReservation")]
        public IActionResult ApproveReservation([FromForm] int reservationId)
        {
            db.OpenConnection();

            string query = $@"
        UPDATE Item_Reservations 
        SET is_approved = 1, approved_at = GETDATE()
        WHERE reservation_id = {reservationId}";

            db.InsertUpdateDelete(query);
            db.CloseConnection();

            return Ok("Reservation approved.");
        }


        [HttpPut("CheckLateReservations")]
        public IActionResult CheckLateReservations()
        {
            db.OpenConnection();

            string selectQuery = "SELECT reservation_id, approved_at, duration_value, duration_unit FROM Item_Reservations WHERE is_approved = 1 AND is_late = 0";
            var reader = db.GetData(selectQuery);

            var lateIds = new List<int>();

            while (reader.Read())
            {
                var approvedAt = Convert.ToDateTime(reader["approved_at"]);
                var durationValue = Convert.ToInt32(reader["duration_value"]);
                var durationUnit = reader["duration_unit"].ToString();
                var reservationId = Convert.ToInt32(reader["reservation_id"]);

                DateTime dueTime = approvedAt;

                switch (durationUnit)
                {
                    case "Minutes": dueTime = approvedAt.AddMinutes(durationValue); break;
                    case "Hours": dueTime = approvedAt.AddHours(durationValue); break;
                    case "Days": dueTime = approvedAt.AddDays(durationValue); break;
                }

                if (DateTime.Now > dueTime)
                {
                    lateIds.Add(reservationId);
                }
            }

            reader.Close();

            foreach (var id in lateIds)
            {
                string updateQuery = $"UPDATE Item_Reservations SET is_late = 1 WHERE reservation_id = {id}";
                db.InsertUpdateDelete(updateQuery);
            }

            db.CloseConnection();
            return Ok($"{lateIds.Count} reservations marked as late.");
        }

        [HttpGet("GetApprovedReservationsByUser")]
        public IActionResult GetApprovedReservationsByUser([FromQuery] int userId)
        {
            List<Reservation> list = new List<Reservation>();
            db.OpenConnection();

            string query = $@"
        SELECT 
            r.reservation_id,
            r.user_id,
            u.full_name AS user_name,
            r.item_id,
            i.name AS item_name,
            r.duration_value,
            r.duration_unit,
            r.requested_at,
            r.approved_at,
            r.is_approved,
            r.is_late
        FROM 
            Item_Reservations r
        JOIN 
            Users u ON r.user_id = u.user_id
        JOIN 
            Inventory_Items i ON r.item_id = i.item_id
        WHERE 
            r.user_id = {userId} AND r.is_approved = 1";

            var reader = db.GetData(query);

            while (reader.Read())
            {
                list.Add(new Reservation
                {
                    reservation_id = Convert.ToInt32(reader["reservation_id"]),
                    user_id = Convert.ToInt32(reader["user_id"]),
                    user_name = reader["user_name"].ToString(),
                    item_id = Convert.ToInt32(reader["item_id"]),
                    item_name = reader["item_name"].ToString(),
                    duration_value = Convert.ToInt32(reader["duration_value"]),
                    duration_unit = reader["duration_unit"].ToString(),
                    requested_at = Convert.ToDateTime(reader["requested_at"]),
                    approved_at = reader["approved_at"] == DBNull.Value ? null : Convert.ToDateTime(reader["approved_at"]),
                    is_approved = Convert.ToBoolean(reader["is_approved"]),
                    is_late = Convert.ToBoolean(reader["is_late"])
                });
            }

            reader.Close();
            db.CloseConnection();

            return Ok(list);
        }

        [HttpPost("ReturnReservation")]
        public IActionResult ReturnReservation([FromQuery] int reservationId)
        {
            try
            {
                db.OpenConnection();

                string selectQuery = $@"
            SELECT approved_at, duration_value, duration_unit 
            FROM Item_Reservations 
            WHERE reservation_id = {reservationId}";

                var reader = db.GetData(selectQuery);

                DateTime approvedAt = DateTime.MinValue;
                int durationValue = 0;
                string durationUnit = "Minutes";

                if (reader.Read())
                {
                    approvedAt = Convert.ToDateTime(reader["approved_at"]);
                    durationValue = Convert.ToInt32(reader["duration_value"]);
                    durationUnit = reader["duration_unit"].ToString();
                }
                reader.Close();

                // Calculate due time
                DateTime dueTime = approvedAt;
                switch (durationUnit)
                {
                    case "Minutes": dueTime = approvedAt.AddMinutes(durationValue); break;
                    case "Hours": dueTime = approvedAt.AddHours(durationValue); break;
                    case "Days": dueTime = approvedAt.AddDays(durationValue); break;
                }

                bool isLate = DateTime.Now > dueTime;

                // Just update is_late to reflect return time
                string updateQuery = $@"
            UPDATE Item_Reservations 
            SET is_late = {(isLate ? 1 : 0)} 
            WHERE reservation_id = {reservationId}";

                db.InsertUpdateDelete(updateQuery);
                db.CloseConnection();

                return Ok("Item returned and lateness status updated.");
            }
            catch (Exception ex)
            {
                db.CloseConnection();
                return BadRequest("Error: " + ex.Message);
            }
        }
        [HttpGet("GetLateReturnedReservations")]
        public IActionResult GetLateReturnedReservations()
        {
            List<Reservation> list = new List<Reservation>();
            db.OpenConnection();

            string query = @"
    SELECT 
        r.reservation_id,
        r.user_id,
        u.full_name AS user_name,
        r.item_id,
        i.name AS item_name,
        r.duration_value,
        r.duration_unit,
        r.requested_at,
        r.approved_at,
        r.is_approved,
        r.is_late
    FROM 
        Item_Reservations r
    JOIN 
        Users u ON r.user_id = u.user_id
    JOIN 
        Inventory_Items i ON r.item_id = i.item_id
    WHERE 
        r.is_late = 1";

            var reader = db.GetData(query);

            while (reader.Read())
            {
                list.Add(new Reservation
                {
                    reservation_id = Convert.ToInt32(reader["reservation_id"]),
                    user_id = Convert.ToInt32(reader["user_id"]),
                    user_name = reader["user_name"].ToString(),
                    item_id = Convert.ToInt32(reader["item_id"]),
                    item_name = reader["item_name"].ToString(),
                    duration_value = Convert.ToInt32(reader["duration_value"]),
                    duration_unit = reader["duration_unit"].ToString(),
                    requested_at = Convert.ToDateTime(reader["requested_at"]),
                    approved_at = reader["approved_at"] == DBNull.Value ? null : Convert.ToDateTime(reader["approved_at"]),
                    is_approved = Convert.ToBoolean(reader["is_approved"]),
                    is_late = Convert.ToBoolean(reader["is_late"])
                });
            }

            reader.Close();
            db.CloseConnection();

            return Ok(list);
        }


    }
}
