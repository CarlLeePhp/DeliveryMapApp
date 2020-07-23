using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryAppWhiterocks.Models.XeroAPI
{
    public class Tenant
    {
        public string id { get; set; }
        public string authEventId { get; set; }
        public string tenantId { get; set; }
        public string tenantType { get; set; }
        public string tenantName { get; set; }
        public DateTime createdDateUtc { get; set; }
        public DateTime updatedDateUtc { get; set; }
    }
}
