using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms.Maps;

namespace DeliveryAppWhiterocks.Models
{
    class PlaceLocation
    {
        public string PlaceName { get; set; }
        public string Address { get; set; }
        public Position Position { get; set; }
        public Location Location { get; set; }

        public PlaceLocation(string PlaceName,string Address, Position Position, Location Location)
        {
            this.PlaceName = PlaceName;
            this.Address = Address;
            this.Position = Position;
            this.Location = Location;
        }
    }
}
