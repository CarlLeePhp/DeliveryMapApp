using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryAppWhiterocks.Models.XeroAPI
{
    public class ContactPerson
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public bool IncludeInEmails { get; set; }
    }
}
