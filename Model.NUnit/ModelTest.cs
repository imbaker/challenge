using System;
using System.Collections.Generic;

using NUnit.Framework;

namespace Hitachi.NUnit
{
    [TestFixture]
    public class ModelTest
    {
        Model model = null;

        private const string BuenosAires = "Buenos Aires";
        private const string Casablanca = "Casablanca";
        private const string CapeTown = "Cape Town";
        private const string Liverpool = "Liverpool";
        private const string NewYork = "New York";

        [SetUp]
        public void Init()
        {
            model = new Model();
            model.AddValidRoute(new Route() { Start = BuenosAires, End = NewYork, Duration = 6 });
            model.AddValidRoute(new Route() { Start = BuenosAires, End = Casablanca, Duration = 5 });
            model.AddValidRoute(new Route() { Start = BuenosAires, End = CapeTown, Duration = 4 });
            model.AddValidRoute(new Route() { Start = NewYork, End = Liverpool, Duration = 4 });
            model.AddValidRoute(new Route() { Start = Liverpool, End = Casablanca, Duration = 3 });
            model.AddValidRoute(new Route() { Start = Liverpool, End = CapeTown, Duration = 6 });
            model.AddValidRoute(new Route() { Start = Casablanca, End = Liverpool, Duration = 3 });
            model.AddValidRoute(new Route() { Start = Casablanca, End = CapeTown, Duration = 6 });
            model.AddValidRoute(new Route() { Start = CapeTown, End = NewYork, Duration = 8 });
        }

        [Test]
        public void AddValidRoute_StartEndValuesTheSame_ThrowsException()
        {
            ArgumentException e = Assert.Throws<ArgumentException>(() => { model.AddValidRoute(new Route() { Start = BuenosAires, End = BuenosAires, Duration = 1 }); });
            Assert.That(e.Message, Is.EqualTo("Route Start and End cannot be the same."));
        }

        [Test]
        public void AddValidRoute_DurationIsLEZero_ThrowsException()
        {
            ArgumentException e = Assert.Throws<ArgumentException>(() => { model.AddValidRoute(new Route() { Start = BuenosAires, End = NewYork, Duration = 0 }); });
            Assert.That(e.Message, Is.EqualTo("Route Duration must be greater than zero."));
        }

        [Test]
        public void ListValidRoutes_ReturnsNine()
        {
            Assert.AreEqual(9, model.CountValidRoutes());
        }

        [Test]
        public void TotalJourneyTime_InvalidPort_ThrowsException()
        {
            List<string> ports = new List<string>() { CapeTown, "Southampton" };
            ArgumentOutOfRangeException e = Assert.Throws<ArgumentOutOfRangeException>(() => { model.TotalJourneyTime(ports); });
            Assert.That(e.Message, Is.EqualTo("Invalid port\r\nParameter name: ports\r\nActual value was Southampton."));
        }

        [Test]
        public void TotalJourneyTime_InvalidRoute_ThrowsException()
        {
            List<string> ports = new List<string>() { CapeTown, Casablanca };
            ArgumentException e = Assert.Throws<ArgumentException>(() => { model.TotalJourneyTime(ports); });
            Assert.That(e.Message, Is.EqualTo("Invalid route: Cape Town to Casablanca is not a valid route."));
        }

        [Test]
        public void TotalJourneyTime_Buenos_NY_Liverpool_Is_10_days()
        {
            List<string> ports = new List<string>() { BuenosAires, NewYork, Liverpool };
            Assert.AreEqual(10, model.TotalJourneyTime(ports));
        }


        [Test]
        public void TotalJourneyTime_Buenos_Casablanca_Liverpool_Is_8_days()
        {
            List<string> ports = new List<string>() { BuenosAires, Casablanca, Liverpool };
            Assert.AreEqual(8, model.TotalJourneyTime(ports));
        }

        [Test]
        public void TotalJourneyTime_Buenos_Capetown_NY_Liverpool_Casablanca_Is_19_days()
        {
            List<string> ports = new List<string>() { BuenosAires, CapeTown, NewYork, Liverpool, Casablanca };
            Assert.AreEqual(19, model.TotalJourneyTime(ports));
        }

        [Test]
        public void CalculateShortestJourney_InvalidPort_ThrowsException()
        {
            ArgumentOutOfRangeException e = Assert.Throws<ArgumentOutOfRangeException>(() => { model.CalculateShortestJourney(CapeTown, "Southampton"); });
            Assert.That(e.Message, Is.EqualTo("Invalid port\r\nParameter name: ports\r\nActual value was Southampton."));
        }

        [Test]
        [TestCase(BuenosAires, NewYork, Result=6, Description="Test how the method handles a direct route")]
        [TestCase(BuenosAires, Liverpool, Result=8, Description="Test how the method handles an indirect route")]
        [TestCase(NewYork, NewYork, Result = 18, Description="Test how the method handles a circular reference")]
        public int CalculateShortestJourney_Tests(string start, string end)
        {
            return model.CalculateShortestJourney(start, end);
        }

        [Test]
        public void GetJourneysWithMaxStops_InvalidPort_ThrowsException()
        {
            ArgumentOutOfRangeException e = Assert.Throws<ArgumentOutOfRangeException>(() => { model.GetJourneysWithMaxStops(CapeTown, "Southampton", 3); });
            Assert.That(e.Message, Is.EqualTo("Invalid port\r\nParameter name: ports\r\nActual value was Southampton."));
            Assert.That(e.ParamName, Is.EqualTo("ports"));
            Assert.That(e.ActualValue, Is.EqualTo("Southampton"));
        }

        [Test]
        public void GetJourneysWithMaxStops_MaxStopsLE0_ThrowsException()
        {
            ArgumentOutOfRangeException e = Assert.Throws<ArgumentOutOfRangeException>(() => { model.GetJourneysWithMaxStops(CapeTown, NewYork, 0); });
            Assert.That(e.Message, Is.EqualTo("Value must be greater than zero.\r\nParameter name: maxStops\r\nActual value was 0."));
            Assert.That(e.ParamName, Is.EqualTo("maxStops"));
            Assert.That(e.ActualValue, Is.EqualTo(0));
        }

        [Test]
        public void GetJourneysWithMaxStops_LiverpoolLiverpoolWhereStopsLE3_Is2()
        {
            Assert.AreEqual(2, model.GetJourneysWithMaxStops(Liverpool, Liverpool, 3));
        }

        [Test]
        public void GetJourneysWithExactStops_InvalidPort_ThrowsException()
        {
            ArgumentOutOfRangeException e = Assert.Throws<ArgumentOutOfRangeException>(() => { model.GetJourneysWithExactStops(CapeTown, "Southampton", 3); });
            Assert.That(e.Message, Is.EqualTo("Invalid port\r\nParameter name: ports\r\nActual value was Southampton."));
        }

        [Test]
        public void GetJourneysWithExactStops_BuenosLiverpoolWhereStopsIs4_Is0()
        {
            Assert.AreEqual(0, model.GetJourneysWithExactStops(BuenosAires, Liverpool, 4));
        }

        [Test]
        public void GetJourneysWithinDuration_InvalidPort_ThrowsException()
        {
            ArgumentOutOfRangeException e = Assert.Throws<ArgumentOutOfRangeException>(() => { model.GetJourneysWithinDuration(CapeTown, "Southampton", 3); });
            Assert.That(e.Message, Is.EqualTo("Invalid port\r\nParameter name: ports\r\nActual value was Southampton."));
        }

        [Test]
        public void GetJourneysWithinDuration_LiverpoolLiverpoolWhereDuration25_Is2()
        {
            Assert.AreEqual(2, model.GetJourneysWithinDuration(Liverpool, Liverpool, 25));
        }
    }
}
