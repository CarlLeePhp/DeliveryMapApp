using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryAppWhiterocks.Models.XeroAPI
{
    public class Address
    {
        public string AddressType { get; set; }
        public string AddressLine1 { get; set; } = "";
        public string AddressLine2 { get; set; } = "";
        public string AddressLine3 { get; set; } = "";
        public string AddressLine4 { get; set; } = "";
        public string City { get; set; } = "";
        public string Region { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; } = "New Zealand";
        public string AttentionTo { get; set; }

        
    }
}
