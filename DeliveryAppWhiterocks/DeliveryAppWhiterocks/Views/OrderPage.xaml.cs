using DeliveryAppWhiterocks.Models;
using System;
using System.Collections.Generic;
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
        List<OrderTemp> items;

        public OrderPage()
        {
            InitializeComponent();
            Init();
            SupplyOrder();
        }

        private void Init()
        {
            NavigationPage.SetHasNavigationBar(this, false);

            App.CheckInternetIfConnected(noInternetLbl, this);
        
        }

        private void SupplyOrder()
        {
        }

        private async void btnLocation_Clicked(object sender, EventArgs e)
        {
            await this.Navigation.PushAsync(new Maps());
        }
    }
}