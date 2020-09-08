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
        public string InvoiceType { get; set; }
        public bool CompletedDeliveryStatus { get; set; }
        public string ContactID { get; set; }
        public double Subtotal { get; set; }
        public string TenantID { get; set; }
        public long UpdateTimeTicks { get; set; }
    }
}
