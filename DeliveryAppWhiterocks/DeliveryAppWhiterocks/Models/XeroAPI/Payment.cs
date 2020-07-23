using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryAppWhiterocks.Models.XeroAPI
{
    public class Payment
    {
        public string PaymentID { get; set; }
        public DateTime Date { get; set; }
        public double Amount { get; set; }
        public string Reference { get; set; }
        public double CurrencyRate { get; set; }
        public bool HasAccount { get; set; }
        public bool HasValidationErrors { get; set; }
    }
}
