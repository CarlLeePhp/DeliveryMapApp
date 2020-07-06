using DeliveryAppWhiterocks.Data;
using DeliveryAppWhiterocks.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DeliveryAppWhiterocks
{
    public partial class App : Application
    {
        static TokenDatabaseController tokenDatabase;
        static UserDatabaseController userDatabase;

        public static int screenHeight { get; set; }
        public static int screenWidth { get; set; }

        private static Label labelScreen;
        private static bool hasInternet;
        private static Page currentPage;
        public static Timer timer;
        private static bool noInterShow;

        public App()
        {
            InitializeComponent();
            Init();
            MainPage = new NavigationPage(new MainPage());
        }

        private void Init()
        {
            
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

        public static UserDatabaseController UserDatabase
        {
            get
            {
                if (userDatabase == null)
                {
                    userDatabase = new UserDatabaseController();
                }
                return userDatabase;
            }
        }

        public static void CheckInternetIfConnected(Label label, Page page)
        {
            labelScreen = label;
            label.Text = Constants.noInternetText;
            label.IsVisible = false;
            hasInternet = true;
            currentPage = page;
            if(timer == null)
            {
                timer = new Timer((e) =>
                {
                    CheckInternetOverTime();
                },null,10,(int)TimeSpan.FromSeconds(3).TotalMilliseconds);
            }
        }

        private static void CheckInternetOverTime()
        {
            var networkConnection = DependencyService.Get<INetworkConnection>();
            networkConnection.CheckInternetConnection();
            if (!networkConnection.isConnected)
            {
                if (hasInternet)
                {
                    if (!noInterShow)
                    {
                        hasInternet = false;
                        labelScreen.IsVisible = true;
                    }
                }
            }
            else
            {
                Device.BeginInvokeOnMainThread(() => {
                    hasInternet = true;
                    labelScreen.IsVisible = false;
                });
            }
        }

        public static async Task<bool> CheckIfInternet()
        {
            var networkConnection = DependencyService.Get<INetworkConnection>();
            networkConnection.CheckInternetConnection();
            return networkConnection.isConnected;
        }

        public static async Task<bool> CheckIfInternetAlert()
        {
            var networkConnection = DependencyService.Get<INetworkConnection>();
            networkConnection.CheckInternetConnection();
            if (!networkConnection.isConnected)
            {
                if (!noInterShow)
                {
                    await ShowDisplayAlert();
                }
            }
            return false;
        }

        private static async Task ShowDisplayAlert()
        {
            noInterShow = false;
            await currentPage.DisplayAlert("Internet", "Device did not connected to internet, please activate your connection", "OK");
            noInterShow = false;
        }
    }
}
