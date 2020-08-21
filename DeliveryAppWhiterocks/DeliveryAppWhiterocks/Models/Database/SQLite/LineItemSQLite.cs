using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryAppWhiterocks.Models.Database.SQLite
{
    public class LineItemSQLite
    {
        [PrimaryKey]
        public int ItemLineID { get; set; }
        public string InvoiceID { get; set; }
        public string ItemCode { get; set; }
        public int Quantity { get; set; }
        public double UnitAmount { get; set; }
    }
}
