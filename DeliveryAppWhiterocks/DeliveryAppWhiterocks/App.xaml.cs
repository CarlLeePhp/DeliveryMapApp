using DeliveryAppWhiterocks.Data.SQLite;
using DeliveryAppWhiterocks.Data;
using DeliveryAppWhiterocks.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using Xamarin.Forms.Xaml;
using Application = Xamarin.Forms.Application;
using DeliveryAppWhiterocks.Models.XeroAPI;

namespace DeliveryAppWhiterocks
{
    public partial class App : Application
    {
        //static TokenDatabaseController tokenDatabase;
        static UserDatabaseController userDatabase;
        static InvoiceDatabaseController invoiceDatabase;
        static LineItemDatabaseController lineItemDatabase;
        static ItemDatabaseController itemDatabase;
        static ContactDatabaseController contactDatabase;
        static TenantDatabaseController tenantDatabase;

        //could be obsolete in the future, when i have time i will update this
        //using grid might be more efficient, i stopped using this
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
            //this line here is necessary to stop the keyboard for blocking the entry/input field in webview
            this.On<Android>().UseWindowSoftInputModeAdjust(WindowSoftInputModeAdjust.Resize);
            
            MainPage = new MainPage();
        }

        private void Init()
        {
            Device.SetFlags(new string[] { "Expander_Experimental" });
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

        #region SQLITEDATABASE
        //initialize sqlite database if it is not exist
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

        public static InvoiceDatabaseController InvoiceDatabase
        {
            get
            {
                if (invoiceDatabase == null)
                {
                    invoiceDatabase = new InvoiceDatabaseController();
                }
                return invoiceDatabase;
            }
        }

        public static LineItemDatabaseController LineItemDatabase
        {
            get 
            { 
                if (lineItemDatabase == null)
                {
                    lineItemDatabase = new LineItemDatabaseController();
                }
                return lineItemDatabase;
            }
        }


        public static ItemDatabaseController ItemDatabase
        {
            get
            {
                if (itemDatabase == null)
                {
                    itemDatabase = new ItemDatabaseController();
                }
                return itemDatabase;
            }
        }

        public static ContactDatabaseController ContactDatabase
        {
            get
            {
                if (contactDatabase == null)
                {
                    contactDatabase = new ContactDatabaseController();
                }
                return contactDatabase;
            }
        }
        public static TenantDatabaseController TenantDatabase
        {
            get
            {
                if(tenantDatabase == null)
                {
                    tenantDatabase = new TenantDatabaseController();
                }
                return tenantDatabase;
            }
        }
        #endregion

        #region INTERNETCHECKING
        //set nointernetlabel in certain page, by periodically checking the internet connection using timer
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
            //get the network connection status from device itself
            if (!networkConnection.isConnected)
            {
                //no internet
                if (hasInternet)
                {
                    if (!noInterShow)
                    {
                        Device.BeginInvokeOnMainThread(() => {
                            hasInternet = false;
                            labelScreen.IsVisible = true;
                        });
                    }
                }
            }
            else
            {
                //has internet
                Device.BeginInvokeOnMainThread(() => {
                    hasInternet = true;
                    labelScreen.IsVisible = false;
                });
            }
        }

        //Check internet connection immediately
        public static bool CheckIfInternet()
        {
            var networkConnection = DependencyService.Get<INetworkConnection>();
            networkConnection.CheckInternetConnection();
            return networkConnection.isConnected;
        }

        //alert the user if there is no internet
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
        #endregion
    }
}
