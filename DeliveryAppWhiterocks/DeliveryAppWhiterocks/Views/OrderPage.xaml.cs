using DeliveryAppWhiterocks.Models;
using DeliveryAppWhiterocks.Models.Database.SQLite;
using DeliveryAppWhiterocks.Models.XeroAPI;
using IdentityModel.Client;
using RestSharp;
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
        bool _childPageLoaded = false;

        public OrderPage()
        {
            InitializeComponent();
            //App.Current.MainPage = this;
            Init();
            _deliveryOrders = new ObservableCollection<Invoice>();
            //SupplyOrder();

            
        }

        public string Hello()
        {
            return "Hello";
        }
        protected override void OnAppearing()
        {
            _childPageLoaded = false;
            SupplyOrder(); // Moved from Constructor
            CheckHasDataLabel();
        }
        private void Init()
        {
            NavigationPage.SetHasNavigationBar(this, false);
            App.CheckInternetIfConnected(noInternetLbl, this);
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
            // no access token in Preferences: first run -> login
            // more than 30 days -> login
            // more than 30 mins -> get new access token
            // otherwise -> get data directly
            XeroAPI.DecodeAccessToken();
            if (XeroAPI._accessToken != null)
            {
                long currentUnixTimeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                if(currentUnixTimeStamp - XeroAPI._accessToken.nbf > 1800 && currentUnixTimeStamp - XeroAPI._accessToken.nbf < 30 * 24 * 3600)
                {
                    // get a new access token
                }

                await DisplayAlert("Login Time", XeroAPI._accessToken.nbf.ToString(), "OK");
            }
            
            
            
            //if (App.CheckIfInternet()) {
            //    await Navigation.PushModalAsync(new XEROWebPage());
            //    GridOverlay.IsVisible = false;
                
            //} else
            //{
            //    await DisplayAlert("Oops", "No internet connection, couldn't load data from XERO", "OK");
            //}
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

        private void TapFinish_Tapped(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new CompletedPage());
        }

        private void DeliveryInvoice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var currentSelection = e.CurrentSelection.FirstOrDefault() as Invoice;
            if (currentSelection == null || _childPageLoaded) return;
            _childPageLoaded = true;
            Navigation.PushModalAsync(new OrderDetailPage(currentSelection));

            ((CollectionView)sender).SelectedItem = null;
        }
    }
}