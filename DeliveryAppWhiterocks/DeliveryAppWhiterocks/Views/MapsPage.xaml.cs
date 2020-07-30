using DeliveryAppWhiterocks.Models;
using DeliveryAppWhiterocks.Models.Database.SQLite;
using DeliveryAppWhiterocks.Models.GoogleDirectionAPI;
using DeliveryAppWhiterocks.Models.Helper;
using DeliveryAppWhiterocks.Models.XeroAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;
using Xamarin.Forms.Xaml;

namespace DeliveryAppWhiterocks.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapsPage : ContentPage
    {
        List<string> _waypoints = new List<string>();
        Position _lastKnownPosition;
        public MapsPage()
        {
            InitializeComponent();
            //Enable the blue circle that mark the current location of user
            map.MyLocationEnabled = true;
            InitMap();
        }

        public async void InitMap()
        {
            CenterMapToCurrentLocation();
            await InitPins();
            MapWaypoints();
            //The Geocoder.GetPositionsForAddressAsync() doesnt work, it shows GRPC error,


            //Position position = await GoogleMapsAPI.GetPositionFromKnownAddress("84 Lithgow st, Invercargill ,New Zealand");

            //if (position.Latitude == 0 && position.Longitude == 0)
            //{
            //    await DisplayAlert("Alert", "Not Found", "OK");
            //}
            //else
            //{
            //    map.MyLocationEnabled = true;
            //    map.MoveToRegion(MapSpan.FromCenterAndRadius(position, Distance.FromKilometers(3)), true);

            //    string reverseGeo = await GoogleMapsAPI.GetAdressFromKnownPosition(position);
            //    await DisplayAlert("Map", $"{reverseGeo}", "OK");
            //}
        }

        private async void MapWaypoints()
        {
            GoogleDirection direction = await GoogleMapsAPI.MapDirections(_lastKnownPosition, _waypoints.ToArray());
            List<Position> directionPolylines = PolylineHelper.Decode(direction.Routes[0].OverviewPolyline.Points).ToList();
            for(int i = 0; i < directionPolylines.Count-1; i++)
            {
                Xamarin.Forms.GoogleMaps.Polyline polyline = new Xamarin.Forms.GoogleMaps.Polyline() {
                    StrokeColor = Constants.mapShapeColor,
                    StrokeWidth = 8,
                    
                };

                polyline.Positions.Add(directionPolylines[i]);
                polyline.Positions.Add(directionPolylines[i+1]);
                map.Polylines.Add(polyline);
            }
        }

        private async Task<bool> InitPins()
        {
            foreach(InvoiceSQLite invoice in App.InvoiceDatabase.GetAllIncompleteInvoices())
            {
                ContactSQLite customerContact = App.ContactDatabase.GetContactByID(invoice.ContactID);
                string fullAddress = customerContact.Address;
                
                if(customerContact.City != "")
                {
                    fullAddress += $", {customerContact.City}";
                }
                fullAddress += $", New Zealand";

                

                Position position = await GoogleMapsAPI.GetPositionFromKnownAddress(fullAddress);
                //separate waypoints by comma
                _waypoints.Add($"{position.Latitude}%2C{position.Longitude}");
                var pin = new Pin()
                {
                    Position = position,
                    Label = $"{invoice.InvoiceNumber}",
                    //set tag so we can reference it when a pin is clicked
                    Tag = invoice
                };
                map.Pins.Add(pin);
            }
            return true;
        }


        private async void CenterMapToCurrentLocation()
        {
            Location lastKnownLocation = await Geolocation.GetLastKnownLocationAsync();
            _lastKnownPosition = new Position(lastKnownLocation.Latitude, lastKnownLocation.Longitude);
            map.MoveToRegion(new MapSpan(_lastKnownPosition, 0.05, 0.05));
            
        }
    }
}