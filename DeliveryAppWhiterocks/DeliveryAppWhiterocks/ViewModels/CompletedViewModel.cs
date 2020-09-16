using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

using DeliveryAppWhiterocks.Models.XeroAPI;
using DeliveryAppWhiterocks.Models.Database.SQLite;
using Xamarin.Forms;
using DeliveryAppWhiterocks.Models;
using System.Linq;

namespace DeliveryAppWhiterocks.ViewModels
{
    public class CompletedViewModel : BaseViewModel
    {
        INavigation _navigation;
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

        /**
         * Commands
         */

        public Command CloseCommand { get; set; }
        public CompletedViewModel(INavigation navigation)
        {
            _navigation = navigation;
            // Register Commands
            CloseCommand = new Command(Close);
            _deliveryOrders.Clear();

            List<InvoiceSQLite> invoices = App.InvoiceDatabase.GetAllIncompleteInvoices();
            invoices = invoices.OrderByDescending(invoiceX => invoiceX.UpdateTimeTicksApp).ToList();

            foreach (InvoiceSQLite invoiceSqlite in invoices)
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
                    Type = invoiceSqlite.InvoiceType,
                    InvoiceID = invoiceSqlite.InvoiceID,
                    InvoiceNumber = invoiceSqlite.InvoiceNumber,
                    Contact = contact,
                    Status = "Completed",
                    TypeColor = invoiceSqlite.InvoiceType == "ACCREC" ? Constants.IsDropOffColor : Constants.IsPickUpColor
                };
                _deliveryOrders.Add(invoice);
            }
        } // Constructor

        private async void Close()
        {
            await _navigation.PopModalAsync();
        }

    }
}
