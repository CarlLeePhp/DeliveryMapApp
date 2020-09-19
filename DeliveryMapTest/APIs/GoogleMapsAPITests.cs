using NUnit.Framework;
using DeliveryAppWhiterocks.Models.GoogleDirectionAPI;
using Xamarin.Forms.GoogleMaps;
using System.Threading.Tasks;

namespace DeliveryMapTest.APIs
{
    public class GoogleMapsAPITests
    {
        [SetUp]
        public void Setup()
        {

        }

        
        [Test]
        public async Task GetPositionFromKnownAddressTest()
        {
            string addressString = "3 Russel Street, GlandStone, Invercargill, New Zealand";
            Position position = await GoogleMapsAPI.GetPositionFromKnownAddress(addressString);
            Assert.AreEqual(-46.3, position.Latitude, 0.1);
            //addressString = "140 Main Street, Gore, New Zealand";
            //position = await GoogleMapsAPI.GetPositionFromKnownAddress(addressString);
            //Assert.AreEqual(-46.1, position.Latitude, 0.1);
            addressString = "Invercargill, Russel st 3 ";
            position = await GoogleMapsAPI.GetPositionFromKnownAddress(addressString);
            Assert.AreEqual(0, position.Latitude, 0.5);
        }

        [Test]
        public void SortWaypointsTest()
        {
            string[] waypoints =
            {
                $"45.1%2C30.1",
                $"45.3%2C30.3",
                $"45.3%2C30.1"
            };
            foreach(string line in waypoints)
            {
                System.Console.WriteLine(line);
            }
            GoogleMapsAPI.SortWaypoints(waypoints);
            Assert.AreEqual("45.3%2C30.1", waypoints[1]);
        }
    }
}
