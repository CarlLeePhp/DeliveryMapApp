using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryAppWhiterocks.Data
{
    public interface INetworkConnection
    {
        bool isConnected { get; set; }
        void CheckInternetConnection();
    }
}
