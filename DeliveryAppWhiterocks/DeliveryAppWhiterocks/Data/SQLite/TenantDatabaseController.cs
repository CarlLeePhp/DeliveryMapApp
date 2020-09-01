using System;
using System.Collections.Generic;
using System.Text;

using DeliveryAppWhiterocks.Models.Database.SQLite;
using SQLite;
using Xamarin.Forms;

namespace DeliveryAppWhiterocks.Data.SQLite
{
    public class TenantDatabaseController
    {
        static object locker = new object();

        SQLiteConnection database;

        public TenantDatabaseController()
        {
            database = DependencyService.Get<ISQLite>().GetConnection();
            database.CreateTable<TenantSQLite>();
        }
        public List<TenantSQLite> GetAllTenants()
        {
            lock (locker)
            {
                return database.Table<TenantSQLite>().ToList();
            }
        }
        public void InsertTenant(TenantSQLite tenant)
        {
            lock (locker)
            {
                database.Insert(tenant);
            }
        }

        public void DeleteAllTenants()
        {
            lock (locker)
            {
                database.DeleteAll<TenantSQLite>();
            }
        }
    }
}
