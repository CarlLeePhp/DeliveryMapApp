using DeliveryAppWhiterocks.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;

namespace DeliveryAppWhiterocks.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Maps : ContentPage
    {
        public double targetLocation { get; set; }
        public int MyProperty { get; set; }

        public Maps()
        {
            InitializeComponent();
            InitMaps();
        }

        private void InitMaps()
        {
            //Location loc = await Xamarin.Essentials.Geolocation.GetLocationAsync();

            //PlaceLocation x = new PlaceLocation("Dunedin", "Dunedin", new Position(45.8788, 170.5028), new Location(45.8788, 170.5028));
            //List<PlaceLocation> loc = new List<PlaceLocation>();
            //loc.Add(x);
            //MyMap.ItemsSource = loc;

           
            MyMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(-46.411944, 168.366916), Distance.FromKilometers(100)));
        }
    }
}