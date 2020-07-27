using System;
using System.Collections.Generic;
using System.Text;
using DeliveryAppWhiterocks.Models;
using DeliveryAppWhiterocks.Models.XeroAPI;

namespace DeliveryAppWhiterocks.Models
{
    public static class TestData
    {
        const int CUST_QUANTITY = 2;
        const int ITEM_QUANTITY = 2;
        public static void CreateInvoice()
        {
            //XeroAPI.XeroAPI._InvoiceResponse = new InvoiceResponse() {  };
            //XeroAPI.XeroAPI._InvoiceResponse.Invoices = new List<Invoice>();
            
            //for(int i = 0; i < CUST_QUANTITY; i++)
            //{
            //    Contact contact = new Contact()
            //    {
            //        Name = "Marvin K",
            //        Addresses = new List<Address>(),
            //    };
            //    contact.Addresses.Add(new Address() { AddressLine1 = "Kappa rd", AddressLine2 = "Gore", City = "Gore" });
            //    contact.Addresses.Add(new Address() { AddressLine1 = "Kappa rd", AddressLine2 = "Gore", City = "Gore" });
            //    List<LineItem> items = itemFactory();
            //    XeroAPI.XeroAPI._InvoiceResponse.Invoices.Add(new Invoice(i.ToString(),"INV-"+i, contact,items));
            //}
        }

        public static List<LineItem> itemFactory()
        {
            List<LineItem> items = new List<LineItem>();

            for(int i = 0; i < ITEM_QUANTITY; i++) { 
                if(i%2 == 0) { 
                    items.Add(new LineItem() { Description = "Horse Weed", ItemCode = "13", Quantity = 4, UnitAmount = 15 });
                } else { 
                    items.Add(new LineItem() { Description = "Kapper", ItemCode = "10", Quantity = 2, UnitAmount = 10 });
                }
            }
            return items;
        }
    }
}
