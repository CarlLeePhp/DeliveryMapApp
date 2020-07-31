using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
using IdentityModel.Client;
using DeliveryAppWhiterocks.Models.Helper;
using DeliveryAppWhiterocks.Models.GoogleGeocodingAPI;
using Xamarin.Forms.GoogleMaps;

namespace DeliveryAppWhiterocks.Models.GoogleDirectionAPI
{
    public class GoogleMapsAPI
    {
        public static async Task<GoogleDirection> MapDirections(Position initialLocation,params string[] waypoints)
        {
            /*https://maps.googleapis.com/maps/api/directions/json?origin=Boston,MA&destination=Concord,MA&waypoints=via:enc:Charlestown,MA|Lexington,MA&key=YOUR_API_KEY&mode=driving*/
            
            var httpClient = new HttpClient();
            //have to turn it into something for API to understand initial location
            //PolylineHelper.Encode(initialLocation);
            string formattedWaypoints = string.Join("|", waypoints);
            //WITH via: OR NOT?
            var response = await httpClient.GetAsync($"{Constants.GoogleDirectionBaseUri}origin={initialLocation.Latitude},{initialLocation.Longitude}&destination={initialLocation.Latitude},{initialLocation.Longitude}&waypoints=optimize:true|{formattedWaypoints}&key={Constants.GoogleAPIKEY}&mode=driving");
            var responseBody = await response.Content.ReadAsStringAsync();
            var googleDirection = JsonConvert.DeserializeObject<GoogleDirection>(responseBody);

            return googleDirection;
        }

        public static async Task<string> GetAdressFromKnownPosition(Position position)
        {
            string address = "";
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"{Constants.GoogleGeocodingBaseUri}latlng={position.Latitude},{position.Longitude}&key={Constants.GoogleAPIKEY}");

            if (!response.IsSuccessStatusCode) return address;

            var responseBody = await response.Content.ReadAsStringAsync();
            var googleGeocoding = JsonConvert.DeserializeObject<GoogleGeocoding>(responseBody);

            return googleGeocoding.results[0].formatted_address;
        }

        public static async Task<Position> GetPositionFromKnownAddress(string locationString)
        {
            /*https://maps.googleapis.com/maps/api/geocode/json?address=1600+Amphitheatre+Parkway,+Mountain+View,+CA&key=YOUR_API_KEY*/

            Position pos = new Position(0,0);

            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"{@"https://maps.googleapis.com/maps/api/geocode/json?"}address={locationString}&key={Constants.GoogleAPIKEY}");

            if (!response.IsSuccessStatusCode) return pos;
            var responseBody = await response.Content.ReadAsStringAsync();
            var googleGeocoding = JsonConvert.DeserializeObject<GoogleGeocoding>(responseBody);

            double latitude = googleGeocoding.results.First().geometry.location.lat;
            double longitude = googleGeocoding.results.First().geometry.location.lng;
            pos = new Position(latitude, longitude);

            return pos;
        }

        
    }
}
