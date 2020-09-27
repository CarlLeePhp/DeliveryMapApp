using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace DeliveryAppWhiterocks.Models.XeroAPI
{
    public class Invoice
    {
        public string Type { get; set; }
        public string InvoiceID { get; set; }
        public string InvoiceNumber { get; set; }
        public Contact Contact { get; set; }
        public string Status { get; set; } // empty, completed?
        public List<LineItem> LineItems { get; set; } = new List<LineItem>();
        public double SubTotal { get; set; }
        public double TotalTax { get; set; }
        public double Total { get; set; }
        public DateTime UpdatedDateUTC { get; set; }
        public Color TypeColor { get; set; }
        //Important
        public long UpdateAppTick { get; set; }
    }
}
