using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryAppWhiterocks.Models.XeroAPI
{
    public class Prepayment
    {
        public Contact Contact { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public string LineAmountTypes { get; set; }
        public string SubTotal { get; set; }
        public string TotalTax { get; set; }
        public string Total { get; set; }
        public DateTime UpdatedDateUTC { get; set; }
        public string CurrencyCode { get; set; }
        public DateTime FullyPaidOnDate { get; set; }
        public string Type { get; set; }
        public string PrepaymentID { get; set; }
        public string CurrencyRate { get; set; }
        public string RemainingCredit { get; set; }
        public List<Allocation> Allocations { get; set; }
        public string HasAttachments { get; set; }
    }
}
