using DeliveryAppWhiterocks.Models;
using DeliveryAppWhiterocks.Models.Database.SQLite;
using DeliveryAppWhiterocks.Models.XeroAPI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DeliveryAppWhiterocks.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OrderPage : ContentPage
    {
        ObservableCollection<Invoice> _deliveryOrders;

        public OrderPage()
        {
            InitializeComponent();
            //App.Current.MainPage = this;
            Init();
            _deliveryOrders = new ObservableCollection<Invoice>();
            //SupplyOrder();

            //these 2 lines are for testing, remove later
            TestData.CreateInvoice();
            SupplyOrder();
        }

        private void Init()
        {
            NavigationPage.SetHasNavigationBar(this, false);
            App.CheckInternetIfConnected(noInternetLbl, this);

            CheckHasDataLabel();
        }

        private void CheckHasDataLabel()
        {
            if (App.InvoiceDatabase.CountIncompleteInvoices() == 0)
            {
                noDataLabel.IsVisible = true;
            }
            else
            {
                noDataLabel.IsVisible = false;
            }
        }

        //get data from database when the order page started
        public void SupplyOrder()
        {
            _deliveryOrders.Clear();
            //load data from database
            //do foreach
            //orderTemp.Add(new OrderTemp("INV-011", "Kappa smith", "Morning rd 132, Otago","30", "BLackstuff", 3, 3.5));
            foreach (InvoiceSQLite invoiceSqlite in App.InvoiceDatabase.GetAllIncompleteInvoices())
            {
                ContactSQLite contactSqlite = App.ContactDatabase.GetContactByID(invoiceSqlite.ContactID);
                List<Address> address = new List<Address>();
                //add emtpy one to mimic the structure of our client
                address.Add(new Address());
                if (contactSqlite.City != "") contactSqlite.City = string.Format(", {0}", contactSqlite.City);
                address.Add(new Address() { AddressLine1 = contactSqlite.Address, City = contactSqlite.City});
                
                Contact contact = new Contact() { 
                    ContactID = contactSqlite.ContactID, 
                    Name = contactSqlite.Fullname, 
                    Addresses = address 
                };

                Invoice invoice = new Invoice() {
                    InvoiceID = invoiceSqlite.InvoiceID, 
                    InvoiceNumber =invoiceSqlite.InvoiceNumber, 
                    Contact = contact
                };
                _deliveryOrders.Add(invoice);
            }
            DeliveryInvoice.ItemsSource = _deliveryOrders;
        }

        private void ImgMenu_Tapped(object sender, EventArgs e)
        {
            GridOverlay.IsVisible = true;
        }

        private void TapCloseMenu_Tapped(object sender, EventArgs e)
        {
            GridOverlay.IsVisible = false;
        }

        //Get data from XERO API
        private async void LoadDeliveryBtn_Clicked(object sender, EventArgs e)
        {
            if (App.CheckIfInternet()) {
                await Navigation.PushModalAsync(new XEROWebPage(this));
                GridOverlay.IsVisible = false;
                CheckHasDataLabel();
            } else
            {
                await DisplayAlert("Oops", "No internet connection, couldn't load data from XERO", "OK");
            }
        }

        private void GetDirectionBtn_Clicked(object sender, EventArgs e)
        {
            if (App.CheckIfInternet()) { 
                List<Invoice> invoices = _deliveryOrders.ToList();
                Navigation.PushModalAsync(new MapsPage(invoices));
            } else
            {
                DisplayAlert("Oops", "No internet connection, Google Maps requires an internet connection", "OK");
            }
        }

        private void TapInfo_Tapped(object sender, EventArgs e)
        {
            GridOverlay.IsVisible = false;
            Navigation.PushModalAsync(new DeliveryInfoPage());
        }


        private void DeliveryInvoice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var currentSelection = e.CurrentSelection.FirstOrDefault() as Invoice;
            if (currentSelection == null) return;
            
            Navigation.PushModalAsync(new OrderDetailPage(currentSelection));

            ((CollectionView)sender).SelectedItem = null;
        }
    }
}