using DeliveryAppWhiterocks.Models;
using DeliveryAppWhiterocks.Models.XeroAPI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

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
        }

        private void Init()
        {
            NavigationPage.SetHasNavigationBar(this, false);

            App.CheckInternetIfConnected(noInternetLbl, this);
        
        }

        //get data from database when the application started
        public void SupplyOrder()
        {
            //load data from database
            //do foreach
            //orderTemp.Add(new OrderTemp("INV-011", "Kappa smith", "Morning rd 132, Otago","30", "BLackstuff", 3, 3.5));
            foreach(Invoice invoice in XeroAPI._InvoiceResponse.Invoices)
            {
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
            } else
            {
                await DisplayAlert("Oops", "No internet connection, couldn't load data from XERO", "OK");
            }
        }

        private void TapInfo_Tapped(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new DeliveryInfo());
        }
    }
}