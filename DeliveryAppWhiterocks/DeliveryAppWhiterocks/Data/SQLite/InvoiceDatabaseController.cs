using DeliveryAppWhiterocks.Models.Database.SQLite;
using DeliveryAppWhiterocks.Models.XeroAPI;
using SQLite;
using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Text;
using Xamarin.Essentials;
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
                string currentTenant = Preferences.Get("TenantID", string.Empty);
                List<InvoiceSQLite> invoiceList = database.Table<InvoiceSQLite>().Where(invoice => invoice.TenantID == currentTenant && invoice.CompletedDeliveryStatus == false ).ToList();
                //invoiceList.Reverse();
                return invoiceList;
            }
        }

        public List<InvoiceSQLite> GetAllIncompletePickupInvoice()
        {
            lock (locker)
            {
                string currentTenant = Preferences.Get("TenantID",string.Empty);
                return database.Table<InvoiceSQLite>().Where(invoice => invoice.TenantID == currentTenant && invoice.CompletedDeliveryStatus == false && invoice.InvoiceType == "ACCREC" ).ToList();
            }
        }

        public List<InvoiceSQLite> GetAllCompletedInvoices()
        {
            lock (locker)
            {
                string currentTenant = Preferences.Get("TenantID", string.Empty);
                return database.Table<InvoiceSQLite>().Where(invoice => invoice.TenantID == currentTenant && invoice.CompletedDeliveryStatus == true).ToList();
            }
        }

      

        public InvoiceSQLite GetInvoiceByInvoiceNumber(string invoiceNumber)
        {
            lock (locker)
            {
                return database.Table<InvoiceSQLite>().Where(invoiceX => invoiceX.InvoiceNumber == invoiceNumber).FirstOrDefault();
            }
        }

        public InvoiceSQLite GetInvoiceByInvoiceID(string invoiceID)
        {
            lock (locker)
            {
                return database.Table<InvoiceSQLite>().Where(invoiceX => invoiceX.InvoiceID == invoiceID).FirstOrDefault();
            }
        }

        public int CountIncompleteInvoices()
        {
            lock (locker)
            {
                return database.Table<InvoiceSQLite>().Where(invoiceX => invoiceX.CompletedDeliveryStatus == false).Count();
            }
        }

        public int CountPickupInvoice()
        {
            lock (locker)
            {
                string currentTenant = Preferences.Get("TenantID", string.Empty);
                return database.Table<InvoiceSQLite>().Count(invoiceX => invoiceX.TenantID == currentTenant  && invoiceX.InvoiceNumber.StartsWith("OPC-"));
            }
        }

        public void InsertInvoice(InvoiceSQLite invoice)
        {
            lock (locker)
            {
                database.Insert(invoice);
            }
        }

        public void InsertInvoice(InvoiceSQLite invoice,List<LineItem> lineitem,Contact contact)
        {
            lock (locker)
            {
                database.Insert(invoice);
                //create LineItem to associate it with the invoice
                
                if (!App.ContactDatabase.CheckContactIfExisted(contact.ContactID))
                {
                    ContactSQLite contactSQLite = App.ContactDatabase.PrepareContactSQLite(contact);
                    App.ContactDatabase.InsertContact(contactSQLite);
                } 

                foreach(LineItem item in lineitem) {
                    //sort desc by ID, get the first one (biggest id number)
                    var maxItemLineID = App.LineItemDatabase.GetLastLineItem();
                    //create the id by referencing lineitemtable
                    LineItemSQLite lineItemSQLite = new LineItemSQLite()
                    {
                        // if it's not set set the itemline id to 1 else increment 1 from the biggest value
                        ItemLineID = (maxItemLineID == null ? 1 : maxItemLineID.ItemLineID + 1),
                        InvoiceID = invoice.InvoiceID,
                        ItemCode = item.ItemCode,
                        Quantity = (int)item.Quantity,
                        UnitAmount = item.UnitAmount,
                    };
                    //Save to db
                    App.LineItemDatabase.InsertLineItem(lineItemSQLite);

                    ItemSQLite itemSQLite = App.ItemDatabase.GetItemByID(item.ItemCode);
                    //check if item already exist, if not add it into database
                    if (itemSQLite == null)
                    {
                        ItemSQLite newItem = new ItemSQLite()
                        {
                            ItemCode = item.ItemCode,
                            Description = item.Description,
                            Weight = item.Weight,
                            UnitCost = invoice.InvoiceType == "ACCPAY" ? item.UnitAmount : 0,
                            UpdateTimeTicks = invoice.UpdateTimeTicksXERO,
                        };
                        App.ItemDatabase.InsertItem(newItem);
                    } else
                    {
                        if(invoice.UpdateTimeTicksXERO > itemSQLite.UpdateTimeTicks)
                        {
                            itemSQLite.Weight = item.Weight;
                            itemSQLite.Description = item.Description;
                            itemSQLite.UnitCost = invoice.InvoiceType == "ACCPAY" ? item.UnitAmount : 0;
                            itemSQLite.UpdateTimeTicks = invoice.UpdateTimeTicksXERO;
                            App.ItemDatabase.UpdateItem(itemSQLite);
                        }
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
        public void DeleteInvoiceByInvoice(InvoiceSQLite invoice)
        {
            lock (locker)
            {
                database.Delete(invoice);
            }
        }
    }
}
