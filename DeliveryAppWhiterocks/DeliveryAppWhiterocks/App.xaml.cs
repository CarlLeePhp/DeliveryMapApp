using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DeliveryAppWhiterocks
{
    public partial class App : Application
    {
        public static int screenHeight { get; set; }
        public static int screenWidth { get; set; }

        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new MainPage());
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
