using DeliveryAppWhiterocks.Models.Database.SQLite;
using DeliveryAppWhiterocks.Models.XeroAPI;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace DeliveryAppWhiterocks.Data.SQLite
{
    public class InvoiceDatabaseController
    {
        //created an object to act as a locker, so an object could only be accessed one at a time
        static object locker = new object();

        SQLiteConnection database;

        public InvoiceDatabaseController()
        {
            database = DependencyService.Get<ISQLite>().GetConnection();
            database.CreateTable<InvoiceSQLite>();
        }

        public List<InvoiceSQLite> GetAllIncompleteInvoices()
        {
            lock (locker)
            {
                return database.Table<InvoiceSQLite>().Where(invoice => invoice.CompletedDeliveryStatus == false).ToList();
            }
        }

        public List<InvoiceSQLite> GetAllInvoices()
        {
            lock (locker)
            {
                return database.Table<InvoiceSQLite>().ToList();
            }
        }

        public void InsertInvoice(InvoiceSQLite invoice,List<LineItem> lineitem)
        {
            lock (locker)
            {
                database.Insert(invoice);
                //create LineItem to associate it with the invoice

                foreach(LineItem item in lineitem) {
                    //sort desc by ID, get the first one (biggest id number)
                    var maxItemLineID = database.Table<LineItemSQLite>().OrderByDescending(lineItemX => lineItemX.ItemLineID).FirstOrDefault();
                    //create the id by referencing lineitemtable
                    LineItemSQLite lineItemSQL = new LineItemSQLite()
                    {
                        // if it's not set set the itemline id to 1 else increment 1 from the biggest value
                        ItemLineID = (maxItemLineID == null ? 1 : maxItemLineID.ItemLineID +1),
                        InvoiceID = invoice.InvoiceID,
                        ItemCode = item.ItemCode,
                        Quantity = (int)item.Quantity
                    };
                    //Save to db
                    App.LineItemDatabase.InsertLineItem(lineItemSQL);

                    //check if item already exist, if not add it into database
                    if(App.ItemDatabase.CheckIfExisted(item.ItemCode) == false)
                    {
                        ItemSQLite newItem = new ItemSQLite()
                        {
                            ItemCode = item.ItemCode,
                            Description = item.Description,
                            UnitAmount = item.UnitAmount,
                            Weight = item.Weight
                        };
                        App.ItemDatabase.InsertItem(newItem);
                    }
                }
            }
        }

        public void DeleteAllInvoices()
        {
            lock (locker)
            {
                database.Table<InvoiceSQLite>().Delete();
            }
        }
    }
}
