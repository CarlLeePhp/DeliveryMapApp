using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using DeliveryAppWhiterocks.Data;
using DeliveryAppWhiterocks.Droid.Data;

[assembly: Xamarin.Forms.Dependency(typeof(NetworkConnection))]
namespace DeliveryAppWhiterocks.Droid.Data
{
    public class NetworkConnection : INetworkConnection
    {
        public bool isConnected { get; set; }

        public void CheckInternetConnection()
        {
            var connectivityManager = (ConnectivityManager)Application.Context.GetSystemService(Context.ConnectivityService);
            var activeNetworkInfo = connectivityManager.ActiveNetwork;

            if(activeNetworkInfo != null)
            {
                isConnected = true;
            } else
            {
                isConnected = false;
            }
        }
    }
}