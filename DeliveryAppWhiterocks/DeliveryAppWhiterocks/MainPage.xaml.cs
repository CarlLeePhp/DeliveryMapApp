using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using DeliveryAppWhiterocks.Models;
using DeliveryAppWhiterocks.Views;
using DeliveryAppWhiterocks.Models.XeroAPI;
using DeliveryAppWhiterocks.Models.Database.SQLite;
using Xamarin.Essentials;

namespace DeliveryAppWhiterocks
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            SetAppMargin();
            Init();
        }

        private void SetAppMargin()
        {
            //DisplayAlert("Test", $"{App.screenWidth.ToString()} {App.screenHeight.ToString()}", "OK");
            topRowMargin.Height = App.screenHeight / 4;
            botRowMargin.Height = App.screenHeight / 4;
            leftColMargin.Width = App.screenWidth / 4;
            rightColMargin.Width = App.screenWidth / 4;
        }

        private void Init()
        {
            BackgroundColor = Constants.backgroundColor;
        }

        protected async override void OnAppearing()
        {
            NavigationPage.SetHasNavigationBar(this, false);
            base.OnAppearing();
            loadLocalData();
            
            var location = await Geolocation.GetLastKnownLocationAsync();
            if(location == null)
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Medium);
                Geolocation.GetLocationAsync(request);
            }

            await Task.Delay(5000);
            Application.Current.MainPage = new NavigationPage(new OrderPage());
        }

        public void loadLocalData()
        {
            //App.InvoiceDatabase.DeleteAllInvoices();
            //App.LineItemDatabase.DeleteAllLineItems();
            //App.ContactDatabase.DeleteAllContacts();
            //App.ItemDatabase.DeleteAllItems();
        }
    }
}