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
using System.Security.Cryptography;

namespace DeliveryAppWhiterocks.Models.GoogleDirectionAPI
{
    public class GoogleMapsAPI
    {
        public static List<int> _waypointsOrder = new List<int>();

        //DEST CONSTANT ONLY FOR TESTING REMOVE LATER
        public static Position destination = new Position(-46.4134, 168.3556);
        public static Position DestinationPosition { get; set; }
        public static string DestinationAddress { get; set; }


        public static async Task<GoogleDirection> MapDirectionsWithWaypoints(Position initialLocation,string endLocation,params string[] waypoints)
        {
            
            //GET REQUEST URI
            /*https://maps.googleapis.com/maps/api/directions/json?origin=Boston,MA&destination=Concord,MA&waypoints=via:enc:Charlestown,MA|Lexington,MA&key=YOUR_API_KEY&mode=driving*/
            
            _waypointsOrder = new List<int>();
            var httpClient = new HttpClient();
            
            string formattedWaypoints = string.Join("|", waypoints);
            
            var response = await httpClient.GetAsync($"{Constants.GoogleDirectionBaseUri}origin={initialLocation.Latitude},{initialLocation.Longitude}&destination={endLocation}&waypoints=optimize:true|{formattedWaypoints}&key={Constants.GoogleAPIKEY}&mode=driving");
            
            if (!response.IsSuccessStatusCode) return null;

            var responseBody = await response.Content.ReadAsStringAsync();
            var googleDirection = JsonConvert.DeserializeObject<GoogleDirection>(responseBody);


            //ERROR could happened when google couldnt map the location, because it's in different continent / island etc
            try { 
                _waypointsOrder = googleDirection.Routes[0].WaypointOrder.ToList();
            } catch
            {
                
            }
            return googleDirection;
        }

        public static void SortWaypoints(Position initialLocation, params string[] waypoints)
        {
            List<Position> positions = new List<Position>();
            double lat;
            double lon;
            string[] parts;
            for(int i=0;i<waypoints.Length; i++)
            {
                string[] stringSeparators = new string[] { "%2C" };
                parts = waypoints[i].Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
                lat = double.Parse(parts[0]);
                lon = double.Parse(parts[1]);
                positions.Add(new Position(lat, lon));
                _waypointsOrder.Add(i);
            }
            double minDistance;
            for(int i=0; i < waypoints.Length - 1; i++)
            {
                minDistance = Math.Sqrt(
                    Math.Pow(positions[i].Latitude - initialLocation.Latitude, 2) +
                    Math.Pow(positions[i].Longitude - initialLocation.Longitude, 2)
                    );
                for(int j = i+1; j < waypoints.Length; j++)
                {
                    double newDistance = Math.Sqrt(
                    Math.Pow(initialLocation.Latitude - positions[j].Latitude, 2) +
                    Math.Pow(initialLocation.Longitude - positions[j].Longitude, 2)
                    );
                    if(newDistance < minDistance)
                    {
                        minDistance = newDistance;
                        Position tmpPosition = positions[i];
                        positions[i] = positions[j];
                        positions[j] = tmpPosition;
                        string tmpWaypoint = waypoints[i];
                        waypoints[i] = waypoints[j];
                        waypoints[j] = tmpWaypoint;
                        int tmpOrder = _waypointsOrder[i];
                        _waypointsOrder[i] = _waypointsOrder[j];
                        _waypointsOrder[j] = tmpOrder;
                    }
                }
                initialLocation = positions[i];
            }
        }

        internal static async Task<GoogleDirection> MapDirectionsNoWaypoints(Position initialLocation,string destinationPoint)
        {
            var httpClient = new HttpClient();

            var response = await httpClient.GetAsync($"{Constants.GoogleDirectionBaseUri}origin={initialLocation.Latitude},{initialLocation.Longitude}&destination={destinationPoint}&key={Constants.GoogleAPIKEY}&mode=driving");

            if (!response.IsSuccessStatusCode) return null;

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

            try { 
                double latitude = googleGeocoding.results.First().geometry.location.lat;
                double longitude = googleGeocoding.results.First().geometry.location.lng;
                pos = new Position(latitude, longitude);
            } catch
            {
                Console.WriteLine($"{locationString} Not Found");
            }
            return pos;
        }
    }
}
