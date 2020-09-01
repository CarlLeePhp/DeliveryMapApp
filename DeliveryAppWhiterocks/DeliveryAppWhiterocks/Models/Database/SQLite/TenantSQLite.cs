using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace DeliveryAppWhiterocks.Models.Database.SQLite
{
    public class TenantSQLite
    {
        [PrimaryKey, AutoIncrement]
        public int TenantKey { get; set; }
        public string TenantID { get; set; }
        public int TenantIndex { get; set; }
        public string TenantName { get; set; }
    }
}
