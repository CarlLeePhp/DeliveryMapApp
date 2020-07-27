using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryAppWhiterocks.Data
{
    //An interface to check network connection,
    //Implemented on the Platform specific android, DeliveryAppWhiterocks.Android.Data.NetworkConnection
    public interface INetworkConnection
    {
        bool isConnected { get; set; }
        void CheckInternetConnection();
    }
}
