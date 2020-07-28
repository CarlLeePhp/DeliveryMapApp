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
        public void Test1()
        {
            string result = XeroAPI.Hello();
            Assert.AreEqual("Hello", result);
        }

        [Test]
        public void GetWeightTest()
        {
            double weight = XeroAPI.GetWeight("Apple Pie 23kg");
            Assert.AreEqual(23, weight);
            weight = XeroAPI.GetWeight("Apple Pie 23 kg");
            Assert.AreEqual(23, weight);
            weight = XeroAPI.GetWeight("Apple Pie 23 kg new");
            Assert.AreEqual(23, weight);

            // failed cases
            //weight = XeroAPI.GetWeight("Apple Packge 23 kg new");
            //Assert.AreEqual(23, weight);
        }
    }
}