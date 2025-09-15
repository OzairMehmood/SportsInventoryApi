using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjectApi.Models
{
    public class User
    {
        public int user_id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string role { get; set; }
    }

}