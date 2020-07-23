using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryAppWhiterocks.Models.XeroAPI 
{ 
    public class LineItem
    {
        public string ItemCode { get; set; }
        public string Description { get; set; }
        public double UnitAmount { get; set; }
        public string TaxType { get; set; }
        public double TaxAmount { get; set; }
        public double LineAmount { get; set; }
        public string AccountCode { get; set; }
        public List<Tracking> Tracking { get; set; }
        public double Quantity { get; set; }
        public string LineItemID { get; set; }
        public List<ValidationError> ValidationErrors { get; set; }

        public double Weight { get; set; }
    }
}
