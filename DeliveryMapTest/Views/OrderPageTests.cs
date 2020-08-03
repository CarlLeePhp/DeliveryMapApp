using NUnit.Framework;
using DeliveryAppWhiterocks.Models.XeroAPI;
using DeliveryAppWhiterocks.Views;

namespace DeliveryMapTest.Views
{
    public class OrderPageTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            
            OrderPage ord = new OrderPage();

            Assert.AreEqual("Hello", ord.Hello());
        }
    }
}