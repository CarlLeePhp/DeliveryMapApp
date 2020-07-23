using DeliveryAppWhiterocks.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DeliveryAppWhiterocks.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OrderPage : ContentPage
    {
        Color backgroundColor = Constants.backgroundColor;

        ObservableCollection<OrderTemp> orderTemp;

        public OrderPage()
        {
            InitializeComponent();
            //App.Current.MainPage = this;
            Init();
            orderTemp = new ObservableCollection<OrderTemp>();
            SupplyOrder();
        }

        private void Init()
        {
            NavigationPage.SetHasNavigationBar(this, false);

            //App.CheckInternetIfConnected(noInternetLbl, this);
        
        }

        //get data from database when the application started
        private void SupplyOrder()
        {
            //load data from database
            //do foreach
            //orderTemp.Add(new OrderTemp("INV-011", "Kappa smith", "Morning rd 132, Otago","30", "BLackstuff", 3, 3.5));
            
            DeliveryInvoice.ItemsSource = orderTemp;
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
        private void LoadDeliveryBtn_Clicked(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new XEROWebPage());
        }
    }
}