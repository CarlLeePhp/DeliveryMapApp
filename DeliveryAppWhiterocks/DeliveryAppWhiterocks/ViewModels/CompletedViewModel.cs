using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

using DeliveryAppWhiterocks.Models.XeroAPI;
using DeliveryAppWhiterocks.Models.Database.SQLite;

namespace DeliveryAppWhiterocks.ViewModels
{
    public class CompletedViewModel : BaseViewModel
    {
        ObservableCollection<Invoice> _deliveryOrders = new ObservableCollection<Invoice>();
        public ObservableCollection<Invoice> DeliveryOrders
        {
            get => _deliveryOrders;
            set
            {
                _deliveryOrders = value;
                OnPropertyChanged();
            }
        }

        public CompletedViewModel()
        {
            _deliveryOrders.Clear();
            //load data from database
            //do foreach
            //orderTemp.Add(new OrderTemp("INV-011", "Kappa smith", "Morning rd 132, Otago","30", "BLackstuff", 3, 3.5));
            foreach (InvoiceSQLite invoiceSqlite in App.InvoiceDatabase.GetAllCompletedInvoices())
            {
                ContactSQLite contactSqlite = App.ContactDatabase.GetContactByID(invoiceSqlite.ContactID);
                List<Address> address = new List<Address>();
                //add emtpy one to mimic the structure of our client
                address.Add(new Address());
                if (contactSqlite.City != "") contactSqlite.City = string.Format(", {0}", contactSqlite.City);
                address.Add(new Address() { AddressLine1 = contactSqlite.Address, City = contactSqlite.City });

                Contact contact = new Contact()
                {
                    ContactID = contactSqlite.ContactID,
                    Name = contactSqlite.Fullname,
                    Addresses = address
                };

                Invoice invoice = new Invoice()
                {
                    InvoiceID = invoiceSqlite.InvoiceID,
                    InvoiceNumber = invoiceSqlite.InvoiceNumber,
                    Contact = contact
                };
                _deliveryOrders.Add(invoice);
            }
            
        }
    }
}
