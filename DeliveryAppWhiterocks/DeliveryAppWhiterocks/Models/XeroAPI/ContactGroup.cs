using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryAppWhiterocks.Models.XeroAPI
{
    public class ContactGroup
    {
        public string ContactGroupID { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public List<Contact> Contacts { get; set; }
    }
}
