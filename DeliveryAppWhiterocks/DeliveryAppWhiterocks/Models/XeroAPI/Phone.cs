using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryAppWhiterocks.Models.XeroAPI
{
    public class Phone
    {
        public string PhoneType { get; set; }
        public string PhoneNumber { get; set; }
        public string PhoneAreaCode { get; set; }
        public string PhoneCountryCode { get; set; }
    }
}
