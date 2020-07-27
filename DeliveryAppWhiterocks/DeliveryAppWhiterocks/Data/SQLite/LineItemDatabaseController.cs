using DeliveryAppWhiterocks.Models.Database.SQLite;
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
                database.Table<LineItemSQLite>().Delete();
            }
        }
    }
}
