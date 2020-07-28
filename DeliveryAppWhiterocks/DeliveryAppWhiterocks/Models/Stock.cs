using System;
using System.Collections.Generic;
using System.Text;
using Xero.NetStandard.OAuth2.Model.Accounting;

namespace DeliveryAppWhiterocks.Models
{
    public class Stock
    {
        public string ItemCode { get; set; }
        public string ItemDescription { get; set; }
        public double Weight { get; set; }
        public int Quantity { get; set; }


        public Stock(string ItemCode,string ItemDescription,double Weight,double Quantity)
        {
            this.ItemCode = ItemCode;
            this.ItemDescription = ItemDescription;
            this.Weight = Weight;
            this.Quantity = Convert.ToInt32(Quantity);
        }

        public void AddStockQuantity(int Quantity)
        {
            this.Quantity += Quantity;
        }

        public void ReduceStockQuantity(int Quantity)
        {
            this.Quantity -= Quantity;
        }

        public void AddStockWeight(double Weight)
        {
            this.Weight += Weight;
        }

        public void ReduceStockWeight(double Weight)
        {
            this.Weight -= Weight;
        }
    }
}
