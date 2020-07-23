using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryAppWhiterocks.Models.XeroAPI
{
    public class Allocation
    {
        public double Amount { get; set; }
        public DateTime Date { get; set; }
        public Invoice Invoice { get; set; }
    }
}
