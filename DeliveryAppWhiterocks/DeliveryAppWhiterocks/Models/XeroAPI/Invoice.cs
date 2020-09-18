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
        public string Reference { get; set; }
        public List<Payment> Payments { get; set; } = new List<Payment>();
        public List<CreditNote> CreditNotes { get; set; } = new List<CreditNote>();
        public List<Prepayment> Prepayments { get; set; } = new List<Prepayment>();
        public List<Overpayment> Overpayments { get; set; } = new List<Overpayment>();
        public double AmountDue { get; set; }
        public double AmountPaid { get; set; }
        public double AmountCredited { get; set; }
        public double CurrencyRate { get; set; }
        public bool IsDiscounted { get; set; }
        public bool HasAttachments { get; set; }
        public bool HasErrors { get; set; }
        public Contact Contact { get; set; }
        public DateTime DateString { get; set; }
        public DateTime Date { get; set; }
        public DateTime DueDateString { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } // empty, completed?
        public string LineAmountTypes { get; set; }
        public List<LineItem> LineItems { get; set; } = new List<LineItem>();
        public double SubTotal { get; set; }
        public double TotalTax { get; set; }
        public double Total { get; set; }
        public DateTime UpdatedDateUTC { get; set; }
        public string CurrencyCode { get; set; }
        public DateTime FullyPaidOnDate { get; set; }
        public bool SentToContact { get; set; }
        public string BrandingThemeID { get; set; }

        public Color TypeColor { get; set; }
        //Important
        public long UpdateAppTick { get; set; }
    }
}
