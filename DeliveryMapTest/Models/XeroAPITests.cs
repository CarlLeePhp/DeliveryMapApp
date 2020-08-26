using NUnit.Framework;
using DeliveryAppWhiterocks.Models.XeroAPI;

namespace DeliveryMapTest.Models
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void GetWeightTest()
        {
            double weight = XeroAPI.GetWeight("Apple Pie 23kg");
            Assert.AreEqual(23, weight);
            weight = XeroAPI.GetWeight("Apple Pie 24 kg");
            Assert.AreEqual(24, weight);
            weight = XeroAPI.GetWeight("Apple Pie 25 kg new");
            Assert.AreEqual(25, weight);

            // failed cases
            //weight = XeroAPI.GetWeight("Apple Package 26-kg) new");
            //Assert.AreEqual(26, weight);
            //weight = XeroAPI.GetWeight("Apple Packge 27 kg new");
            //Assert.AreEqual(27, weight);
        }

        [Test]
        public void testK()
        {
            string sub1 = "20 kg()";
            string result = sub1.Substring(0, sub1.IndexOf("kg")+2);
            Assert.AreEqual("20 kg", result);
        }
    }
}