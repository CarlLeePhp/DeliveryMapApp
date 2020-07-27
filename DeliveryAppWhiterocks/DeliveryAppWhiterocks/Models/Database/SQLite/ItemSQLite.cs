using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryAppWhiterocks.Models.Database.SQLite
{
    public class ItemSQLite
    {
        [PrimaryKey, Unique]
        public string ItemCode { get; set; }
        public string Description { get; set; }
        public double UnitAmount { get; set; }
        public double Weight { get; set; }
    }
}
