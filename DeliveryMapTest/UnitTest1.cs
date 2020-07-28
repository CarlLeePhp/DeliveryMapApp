using NUnit.Framework;
using DeliveryAppWhiterocks.Models.XeroAPI;

namespace DeliveryMapTest
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
    }
}