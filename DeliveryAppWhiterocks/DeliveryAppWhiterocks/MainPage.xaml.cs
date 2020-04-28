using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using DeliveryAppWhiterocks.Models;
using DeliveryAppWhiterocks.Views;

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
            await Task.Delay(15000);
            await this.Navigation.PushAsync(new LoginPage());
        }
    }
}
