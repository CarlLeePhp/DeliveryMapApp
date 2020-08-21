using DeliveryAppWhiterocks.Models.Database.SQLite;
using DeliveryAppWhiterocks.Models.XeroAPI;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace DeliveryAppWhiterocks.Data.SQLite
{
    public class LineItemDatabaseController
    {
        //created an object to act as a locker, so an object could only be accessed one at a time
        static object locker = new object();

        SQLiteConnection database;

        public LineItemDatabaseController()
        {
            database = DependencyService.Get<ISQLite>().GetConnection();
            database.CreateTable<LineItemSQLite>();
        }

        public void InsertLineItem(LineItemSQLite lineItem)
        {
            lock (locker)
            {
                database.Insert(lineItem);
            }
        }

        public void DeleteAllLineItems()
        {
            lock (locker)
            {
                database.DeleteAll<LineItemSQLite>();
            }
        }

        public List<LineItemSQLite> GetLineItemByInvoiceID(string invoiceID)
        {
            lock (locker)
            {
                return database.Table<LineItemSQLite>().Where(lineItem => lineItem.InvoiceID == invoiceID).ToList();
            }
        }

        public LineItemSQLite GetLastLineItem()
        {
            lock (locker)
            {
                return database.Table<LineItemSQLite>().OrderByDescending(lineItemX => lineItemX.ItemLineID).FirstOrDefault();
            }
        }

        public void UpdateLineItem(LineItemSQLite lineItemSQLite)
        {
            lock (locker)
            {
                database.Update(lineItemSQLite);
            }
        }
    }
}
