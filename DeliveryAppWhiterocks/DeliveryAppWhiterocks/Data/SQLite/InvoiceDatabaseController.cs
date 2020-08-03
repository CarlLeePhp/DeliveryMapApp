using DeliveryAppWhiterocks.Models.Database.SQLite;
using DeliveryAppWhiterocks.Models.XeroAPI;
using SQLite;
using System;
using System.Collections.Generic;
using System.Security.Authentication;
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

        public List<InvoiceSQLite> GetAllCompletedInvoices()
        {
            lock (locker)
            {
                return database.Table<InvoiceSQLite>().Where(invoice => invoice.CompletedDeliveryStatus == true).ToList();
            }
        }

        public List<InvoiceSQLite> GetAllInvoices()
        {
            lock (locker)
            {
                return database.Table<InvoiceSQLite>().ToList();
            }
        }

        public int CountIncompleteInvoices()
        {
            lock (locker)
            {
                return database.Table<InvoiceSQLite>().Where(invoiceX => invoiceX.CompletedDeliveryStatus == false).Count();
            }
        }

        public bool CheckIfExisted(string InvoiceID)
        {
            lock (locker)
            {
                var invoice = database.Table<InvoiceSQLite>().Where(invoiceX => invoiceX.InvoiceID == InvoiceID).FirstOrDefault();
                if(invoice == null)
                {
                    return false;
                } else
                {
                    return true;
                }
            }
        }

        public void InsertInvoice(InvoiceSQLite invoice,List<LineItem> lineitem,Contact contact)
        {
            lock (locker)
            {
                database.Insert(invoice);
                //create LineItem to associate it with the invoice

                if (App.ContactDatabase.CheckContactIfExisted(invoice.ContactID) == false)
                {
                    Address address = contact.Addresses[1];
                    ContactSQLite contactSQLite = new ContactSQLite()
                    {
                        ContactID = contact.ContactID,
                        Fullname = contact.Name,
                        Address = (address.AddressLine1.Trim()+" "+address.AddressLine2.Trim()+" "+address.AddressLine3.Trim()+" "+address.AddressLine4.Trim()).Trim(),
                        City = contact.Addresses[1].City,
                        PostalCode = contact.Addresses[1].PostalCode,
                    };
                    App.ContactDatabase.InsertContact(contactSQLite);
                }

                foreach(LineItem item in lineitem) {
                    //sort desc by ID, get the first one (biggest id number)
                    var maxItemLineID = App.LineItemDatabase.GetLastLineItem();
                    //create the id by referencing lineitemtable
                    LineItemSQLite lineItemSQLite = new LineItemSQLite()
                    {
                        // if it's not set set the itemline id to 1 else increment 1 from the biggest value
                        ItemLineID = (maxItemLineID == null ? 1 : maxItemLineID.ItemLineID +1),
                        InvoiceID = invoice.InvoiceID,
                        ItemCode = item.ItemCode,
                        Quantity = (int)item.Quantity
                    };
                    //Save to db
                    App.LineItemDatabase.InsertLineItem(lineItemSQLite);

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
        public void UpdateInvoiceStatus(InvoiceSQLite invoice)
        {
            lock (locker)
            {
                database.Update(invoice);
            }
            
        }
        public void DeleteAllInvoices()
        {
            lock (locker)
            {
                database.DeleteAll<InvoiceSQLite>();
            }
        }
    }
}
