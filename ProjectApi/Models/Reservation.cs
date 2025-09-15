using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjectApi.Models
{
    public class Reservation
    {
        public int reservation_id { get; set; }
        public int user_id { get; set; }
        public int item_id { get; set; }
        public int duration_value { get; set; }
        public string duration_unit { get; set; }
        public DateTime requested_at { get; set; }
        public DateTime? approved_at { get; set; }
        public bool is_approved { get; set; }
        public bool is_late { get; set; }

        // These are optional — for display only (GET only)
        public string? user_name { get; set; } = null;
        public string? item_name { get; set; } = null;
    }


}