using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryAppWhiterocks.Models.XeroAPI
{
    public static class XeroSettings
    {
        public static string clientID = "AAC7D0CCD2604AB5964F39D7EB7B4560";
        public static string redirectURI = @"https://www.xero.com/nz/";
        public static string scope = "openid profile email accounting.transactions offline_access accounting.contacts accounting.settings";
    }
}
