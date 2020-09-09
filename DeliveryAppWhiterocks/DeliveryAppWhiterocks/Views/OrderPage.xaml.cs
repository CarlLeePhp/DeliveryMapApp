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
        Invoice _currentInvoice;

        public OrderPage()
        {
            InitializeComponent();
            //App.Current.MainPage = this;
            Init();
            _deliveryOrders = new ObservableCollection<Invoice>();
            
        }

        public string Hello()
        {
            return "Hello";
        }
        protected override void OnAppearing()
        {
            _childPageLoaded = false;
            if (_currentInvoice != null)
            {
                int collectionIndex = _deliveryOrders.IndexOf(_currentInvoice);

                InvoiceSQLite invoiceSQLite = App.InvoiceDatabase.GetInvoiceByInvoiceID(_currentInvoice.InvoiceID);

                if (invoiceSQLite.CompletedDeliveryStatus)
                {
                    _deliveryOrders.Remove(_deliveryOrders[collectionIndex]);
                    DeliveryInvoice.ItemsSource = _deliveryOrders;
                }
                _currentInvoice = null;
            } else
            {
                SupplyOrder();
            }
            
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
            foreach (InvoiceSQLite invoiceSqlite in App.InvoiceDatabase.GetAllIncompleteInvoices())
            {
                if (Constants.TenantID != "" && invoiceSqlite.TenantID != Constants.TenantID) continue;
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
                    Type = invoiceSqlite.InvoiceType,
                    InvoiceID = invoiceSqlite.InvoiceID, 
                    InvoiceNumber =invoiceSqlite.InvoiceNumber, 
                    Contact = contact,
                    TypeColor = invoiceSqlite.InvoiceType == "ACCREC" ? Constants.IsDropOffColor : Constants.IsPickUpColor
                };
                _deliveryOrders.Add(invoice);
            }
            DeliveryInvoice.ItemsSource = _deliveryOrders;
            CheckHasDataLabel();
        }

        private void ImgMenu_Tapped(object sender, EventArgs e)
        {
            GridOverlay.IsVisible = true;
        }

        private void TapCloseMenu_Tapped(object sender, EventArgs e)
        {
            GridOverlay.IsVisible = false;
        }

        
        // Get data from XERO API.
        private async void LoadDeliveryBtn_Clicked(object sender, EventArgs e)
        {
            
            if (!App.CheckIfInternet())
            {
                await DisplayAlert("Oops", "No internet connection, couldn't load data from XERO", "OK");
                return;
            }
            spinnerActivity.IsVisible = true;
            // no access token in Preferences: first run -> login
            // more than 30 days -> login
            // more than 30 mins -> get new access token
            // otherwise -> get data directly
            XeroAPI.DecodeAccessToken();
            long currentUnixTimeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            try { 
                if(XeroAPI._accessToken == null || currentUnixTimeStamp - XeroAPI._accessToken.nbf >= 30 * 24 * 3600)
                {
                    await Navigation.PushModalAsync(new XEROWebPage());
                    GridOverlay.IsVisible = false;
                }
                else if (currentUnixTimeStamp - XeroAPI._accessToken.nbf > 1800)
                {
                    // get a new access token
                    await XeroAPI.RefreshToken();
                    await XeroAPI.GetTenantID();
                    await XeroAPI.GetInvoices();
                    await XeroAPI.FillData();
                    SupplyOrder();
                    //await DisplayAlert("Xero API", "You got a new access Token", "OK");
                    await DisplayAlert("Xero API", "Data has been loaded", "OK");

                }
                else
                {
                    // get the data by the access token;
                    await XeroAPI.GetTenantID();
                    await XeroAPI.GetInvoices();
                    await XeroAPI.FillData();
                    SupplyOrder();
                    await DisplayAlert("Xero API", "Data has been loaded", "OK");
                }
            } catch
            {
                await DisplayAlert("Xero API", "Failure in loading data from XERO", "OK");
            }


            spinnerActivity.IsVisible = false;
        }

        private void GetDirectionBtn_Clicked(object sender, EventArgs e)
        {
            if (App.CheckIfInternet() && !_childPageLoaded) {
                _childPageLoaded = true;
                List<Invoice> invoices = _deliveryOrders.ToList();
                Navigation.PushAsync(new MapsPage(invoices),true);
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

        private void TapPickup_Tapped(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new AddPickupPage());
        }

        private void TapSetting_Tapped(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new SettingPage(Navigation));
        }

        private void DeliveryInvoice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var currentSelection = e.CurrentSelection.FirstOrDefault() as Invoice;
            if (currentSelection == null || _childPageLoaded) return;
            _childPageLoaded = true;
            Navigation.PushModalAsync(new OrderDetailPage(currentSelection));
            _currentInvoice = currentSelection;
            ((CollectionView)sender).SelectedItem = null;
        }
    }
}