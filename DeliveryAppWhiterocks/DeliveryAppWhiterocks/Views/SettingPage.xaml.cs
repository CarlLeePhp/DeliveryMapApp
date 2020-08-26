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
    public partial class SettingPage : ContentPage
    {
        public SettingPage()
        {
            InitializeComponent();
            Initial();
        }
        private void Initial()
        {
            App.CheckInternetIfConnected(noInternetLbl, this);
            //endPoint.Text = Constants.endPoint;
            //gst.Text = Constants.taxAmount.ToString();
            
        }
        private async void TapClose_Tapped(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
        private async void Button_Clicked(object sender, EventArgs e)
        {
            await DisplayAlert("Testing", "Button was clicked", "Yes");
           
        }
    }
}