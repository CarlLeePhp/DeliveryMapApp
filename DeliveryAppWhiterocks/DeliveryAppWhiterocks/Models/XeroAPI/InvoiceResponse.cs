using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryAppWhiterocks.Models.XeroAPI
{
    public class InvoiceResponse
    {
        public string Id { get; set; }
        public string Status { get; set; }
        public string ProviderName { get; set; }
        public DateTime DateTimeUTC { get; set; }
        public List<Invoice> Invoices { get; set; }
    }
}
