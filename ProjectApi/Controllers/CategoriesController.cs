using Microsoft.AspNetCore.Mvc;
using ProjectApi.Models;
using System.Collections.Generic;

namespace ProjectApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        DBAccess db = new DBAccess();

        [HttpGet("GetAll")]
        public ActionResult<IEnumerable<Category>> GetCategories()
        {
            List<Category> list = new List<Category>();
            db.OpenConnection();
            var reader = db.GetData("SELECT * FROM Categories");

            while (reader.Read())
            {
                list.Add(new Category
                {
                    category_id = int.Parse(reader["category_id"].ToString()),
                    category_name = reader["category_name"].ToString(),
                    description = reader["description"].ToString()
                });
            }

            reader.Close();
            db.CloseConnection();
            return Ok(list);
        }

        [HttpPost("Add")]
        public IActionResult AddCategory([FromBody] Category c)
        {
            string q = $"INSERT INTO Categories (category_name, description) VALUES ('{c.category_name}', '{c.description}')";
            db.OpenConnection();
            db.InsertUpdateDelete(q);
            db.CloseConnection();
            return Ok("Category Added.");
        }
    }
}
