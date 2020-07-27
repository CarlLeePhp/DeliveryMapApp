using DeliveryAppWhiterocks.Models.Database.SQLite;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace DeliveryAppWhiterocks.Data.SQLite
{
    public class ContactDatabaseController
    {
        static object locker = new object();

        SQLiteConnection database;

        public ContactDatabaseController()
        {
            database = DependencyService.Get<ISQLite>().GetConnection();
            database.CreateTable<ContactSQLite>();
        }

        public void InsertContact(ContactSQLite contact)
        {
            lock (locker)
            {
                database.Insert(contact);
            }
        }

        public bool CheckContactIfExisted(string contactID)
        {
            lock (locker)
            {
                var contactSQLite = database.Table<ContactSQLite>().Where(contact => contact.ContactID == contactID).FirstOrDefault();
                if(contactSQLite == null)
                {
                    return false;
                }
                return true;
            }
        }

        public ContactSQLite GetContactByID(string contactID)
        {
            lock (locker)
            {
                return database.Table<ContactSQLite>().Where(contact => contact.ContactID == contactID).FirstOrDefault();
            }
        }

        public void DeleteAllContacts()
        {
            lock (locker)
            {
                database.DeleteAll<ContactSQLite>();
            }
        }
    }
}
