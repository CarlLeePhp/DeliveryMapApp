using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryAppWhiterocks.Models.Database.SQLite
{
    public class InvoiceSQLite
    {
        [PrimaryKey,Unique]
        public string InvoiceID { get; set; }
        public string InvoiceNumber { get; set; }
        public bool CompletedDeliveryStatus { get; set; }
        public string ContactID { get; set; }
        public double Subtotal { get; set; }

    }
}
