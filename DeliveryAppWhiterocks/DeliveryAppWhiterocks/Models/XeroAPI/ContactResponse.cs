using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryAppWhiterocks.Models.XeroAPI
{
    public class ContactResponse
    {
        public string Id { get; set; }
        public string Status { get; set; }
        public string ProviderName { get; set; }
        public DateTime DateTimeUTC { get; set; }
        public List<Contact> Contacts { get; set; }
    }
}
