using DeliveryAppWhiterocks.Models.Database.SQLite;
using DeliveryAppWhiterocks.Models.XeroAPI;
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

        public List<ContactSQLite> GetAllContact()
        {
            lock (locker)
            {
                return database.Table<ContactSQLite>().ToList();
            }
        }

        public ContactSQLite GetContactByID(string contactID)
        {
            lock (locker)
            {
                return database.Table<ContactSQLite>().Where(contact => contact.ContactID == contactID).FirstOrDefault();
            }
        }

        public ContactSQLite GetContactByName(string fullName)
        {
            lock (locker)
            {
                return database.Table<ContactSQLite>().Where(contact => contact.Fullname == fullName).FirstOrDefault();
            }
        }

        public void UpdateContactPosition(ContactSQLite contact)
        {
            lock (locker)
            {
                database.Update(contact);
            }
        }

        public void DeleteAllContacts()
        {
            lock (locker)
            {
                database.DeleteAll<ContactSQLite>();
            }
        }

        public ContactSQLite PrepareContactSQLite(Contact contact)
        {
            Address address;
            if (contact.Addresses.Count > 1)
            {
                address = contact.Addresses[1];
            } else
            {
                address = new Address()
                {
                    AddressLine1 = "",
                    AddressLine2 = "",
                    AddressLine3 = "",
                    AddressLine4 = "",
                    City = "",
                    PostalCode = "",
                };
                contact.Addresses.Add(new Address());
                contact.Addresses.Add(address);
            }
            
            string phoneNumber = "";

            foreach(Phone phone in contact.Phones)
            {
                if(phone.PhoneNumber != "")
                {
                    phoneNumber = phone.PhoneCountryCode+phone.PhoneNumber;
                    break;
                }
            }

            ContactSQLite contactSQLite = new ContactSQLite()
            {
                ContactID = contact.ContactID,
                Fullname = contact.Name,
                Address = (address.AddressLine1.Trim() + " " + address.AddressLine2.Trim() + " " + address.AddressLine3.Trim() + " " + address.AddressLine4.Trim()).Trim(),
                City = contact.Addresses[1].City,
                PostalCode = contact.Addresses[1].PostalCode,
                PhoneNumber = phoneNumber,
                Type = contact.IsCustomer ? ContactType.Customer : ContactType.Supplier
            };

            return contactSQLite;
        }
    }
}
