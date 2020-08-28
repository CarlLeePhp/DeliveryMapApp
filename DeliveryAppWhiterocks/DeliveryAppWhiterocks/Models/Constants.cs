using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace DeliveryAppWhiterocks.Models
{
    class Constants
    {
        public static string GoogleAPIKEY = "AIzaSyA6xf-nZQIXqgRrP8zrhRbbkJtq0D3ew4Y";
        public static string GoogleDirectionBaseUri = @"https://maps.googleapis.com/maps/api/directions/json?";
        public static string GoogleGeocodingBaseUri = @"https://maps.googleapis.com/maps/api/geocode/json?";
        public static string endPoint = "133 Tay Street, Invercargill 9810, New Zealand";
        
        public static Color mapShapeColor = Color.FromHex("#3895D3");

        public static Color backgroundColor = Color.FromHex("1a1a1b");
        public static Color mainTextColor = Color.FromHex("d7dadc");
        public static Color secondaryTextColor = Color.FromHex("ffffff");
        public static Color contrastColor = Color.FromHex("000000");

        public static Color IsDropOffColor = Color.FromHex("2b3b46");
        public static Color IsPickUpColor = Color.FromHex("2c4b3d");

        public static double taxAmount = 0.15;

        public static string noInternetText="No internet connection, Application is running in offline mode";
    }

    public enum RequestType
    {
        Invoices,
        Invoice,
        Contact,
    }
}
