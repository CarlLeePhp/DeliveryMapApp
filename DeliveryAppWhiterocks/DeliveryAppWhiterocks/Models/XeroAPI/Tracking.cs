using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryAppWhiterocks.Models.XeroAPI
{
    public class Tracking
    {
        public string Name { get; set; }
        public string Option { get; set; }
        public string TrackingCategoryID { get; set; }
        public string TrackingOptionID { get; set; }
    }
}
