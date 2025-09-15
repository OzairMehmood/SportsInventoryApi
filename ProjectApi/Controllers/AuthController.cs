using Microsoft.AspNetCore.Mvc;
using ProjectApi.Models;
using System.Data.SqlClient;

namespace ProjectApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        DBAccess db = new DBAccess();

        [HttpPost("signup")]
        public IActionResult Signup([FromBody] User u)
        {
            db.OpenConnection();
            SqlDataReader reader = db.GetData($"SELECT * FROM Users WHERE email = '{u.email}'");
            if (reader.HasRows)
            {
                db.CloseConnection();
                return BadRequest("User already exists");
            }
            db.CloseConnection();

            db.OpenConnection();
            string query = $"INSERT INTO Users (full_name, email, password, role, created_at) VALUES " +
                           $"('{u.name}', '{u.email}', '{u.password}', '{u.role}', GETDATE())";
            db.InsertUpdateDelete(query);
            db.CloseConnection();

            return Ok("User registered successfully");
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel u)
        {
            db.OpenConnection();
            SqlDataReader reader = db.GetData($"SELECT * FROM Users WHERE email = '{u.email}' AND password = '{u.password}'");
            if (reader.Read())
            {
                var loggedUser = new User
                {
                    user_id = Convert.ToInt32(reader["user_id"]),
                    name = reader["full_name"].ToString(),
                    email = reader["email"].ToString(),
                    role = reader["role"].ToString()
                };
                db.CloseConnection();
                return Ok(loggedUser);
            }
            reader.Close();
            db.CloseConnection();
            return Unauthorized();
        }

        [HttpGet("GetAllUsers")]
        public IActionResult GetAllUsers()
        {
            List<User> users = new List<User>();
            db.OpenConnection();
            string query = "SELECT * FROM Users";
            SqlDataReader sdr = db.GetData(query);
            while (sdr.Read())
            {
                users.Add(new User
                {
                    user_id = Convert.ToInt32(sdr["user_id"]),
                    name = sdr["full_name"].ToString(),
                    email = sdr["email"].ToString(),
                    password = sdr["password"].ToString(),
                    role = sdr["role"].ToString()
                });
            }
            sdr.Close();
            db.CloseConnection();
            return Ok(users);
        }

        [HttpDelete("DeleteUser/{id}")]
        public IActionResult DeleteUser(int id)
        {
            db.OpenConnection();
            string query = $"DELETE FROM Users WHERE user_id = {id}";
            db.InsertUpdateDelete(query);
            db.CloseConnection();
            return Ok("User deleted");
        }
    }
}
