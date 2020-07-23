using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryAppWhiterocks.Models.XeroAPI
{
    public class CreditNote
    {
        public Contact Contact { get; set; }
        public DateTime DateString { get; set; }
        public DateTime Date { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; }
        public string LineAmountTypes { get; set; }
        public List<LineItem> LineItems { get; set; }
        public double SubTotal { get; set; }
        public double TotalTax { get; set; }
        public double Total { get; set; }
        public DateTime UpdatedDateUTC { get; set; }
        public string CurrencyCode { get; set; }
        public DateTime FullyPaidOnDate { get; set; }
        public string Type { get; set; }
        public double RemainingCredit { get; set; }
        public List<Allocation> Allocations { get; set; }
        public bool HasAttachments { get; set; }
        public string CreditNoteID { get; set; }
        public string CreditNoteNumber { get; set; }
    }
}
