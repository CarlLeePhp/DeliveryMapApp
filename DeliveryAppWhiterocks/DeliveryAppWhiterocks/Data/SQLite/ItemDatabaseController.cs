using DeliveryAppWhiterocks.Models.Database.SQLite;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace DeliveryAppWhiterocks.Data.SQLite
{
    public class ItemDatabaseController
    {
        static object locker = new object();

        SQLiteConnection database;
        
        public ItemDatabaseController()
        {
            database = DependencyService.Get<ISQLite>().GetConnection();
            database.CreateTable<ItemSQLite>();
        }

        public bool CheckIfExisted(string itemCode)
        {
            lock (locker)
            {
                var ItemSQLite = database.Table<ItemSQLite>().Where(item => item.ItemCode == itemCode).FirstOrDefault();
                if(ItemSQLite == null)
                {
                    return false;
                }
                return true;
            }
        }

        public ItemSQLite GetItemByID(string itemCode)
        {
            lock (locker)
            {
                return database.Table<ItemSQLite>().Where(item => item.ItemCode == itemCode).FirstOrDefault();
            }
        }

        public void InsertItem(ItemSQLite item)
        {
            lock (locker)
            {
                database.Insert(item);
            }
        }

        public void DeleteAllItems()
        {
            lock (locker)
            {
                database.DeleteAll<ItemSQLite>();
            }
        }
    }
}
