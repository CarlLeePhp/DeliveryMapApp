using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryAppWhiterocks.Models
{
    class OrderTemp
    {
        public string InvoiceID { get; set; }
        public string CustomerName { get; set; }
        public string ItemCode { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public double UnitPrice { get; set; }

        public string Address { get; set; }

        public OrderTemp(string InvoiceID,string CustomerName, string Address, string ItemCode, string Description,int Quantity,double UnitPrice)
        {
            this.InvoiceID = InvoiceID;
            this.CustomerName = CustomerName;
            this.ItemCode = ItemCode;
            this.Description = Description;
            this.Quantity = Quantity;
            this.UnitPrice = UnitPrice;
            this.Address = Address;
        }
    }
}
