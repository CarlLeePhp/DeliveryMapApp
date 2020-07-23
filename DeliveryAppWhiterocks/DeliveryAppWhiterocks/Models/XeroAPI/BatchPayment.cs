using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryAppWhiterocks.Models.XeroAPI
{
    public class BatchPayment
    {
        public string BankAccountNumber { get; set; }
        public string BankAccountName { get; set; }
        public string Details { get; set; }
        public string Code { get; set; }
        public string Reference { get; set; }
    }
}
