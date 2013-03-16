using System;
using System.Collections.Generic;

using NUnit.Framework;

namespace Hitachi.NUnit
{
    [TestFixture]
    public class ModelTest
    {
        Model scenario = null;

        [SetUp]
        public void Init()
        {
            scenario = new Model();
        }

        [Test]
        public void Check_Number_Of_Routes_Is_Nine()
        {
            var routeCount = scenario.ListValidRoutes().Count;
            Assert.AreEqual(9, routeCount);
        }

        [Test]
        public void Total_Journey_Time_Buenos_NY_Liverpool_Is_10_days()
        {
            List<Port> ports = new List<Port>(){ Port.BuenosAires, Port.NewYork, Port.Liverpool };
            var duration = scenario.TotalJourneyTime(ports);
            Assert.AreEqual(10, duration);
        }

        [Test]
        public void Total_Journey_Time_Buenos_Casablanca_Liverpool_Is_8_days()
        {
            List<Port> ports = new List<Port>() { Port.BuenosAires, Port.Casablanca, Port.Liverpool };
            var duration = scenario.TotalJourneyTime(ports);
            Assert.AreEqual(8, duration);
        }

        [Test]
        public void Total_Journey_Time_Buenos_Capetown_NY_Liverpool_Casablanca_Is_19_days()
        {
            List<Port> ports = new List<Port>() { Port.BuenosAires, Port.CapeTown, Port.NewYork, Port.Liverpool, Port.Casablanca };
            var duration = scenario.TotalJourneyTime(ports);
            Assert.AreEqual(19, duration);
        }

        [Test]
        public void Journey_Buenos_Capetown_Casablanca_Is_Invalid()
        {
            List<Port> ports = new List<Port>() { Port.CapeTown, Port.Casablanca };
            Assert.Throws<ArgumentException>( () => { scenario.TotalJourneyTime(ports); } );
        }

        [Test]
        public void Find_Shortest_Journey_Buenos_NY_Is_6_days()
        {
            var duration = scenario.CalculateShortestJourney(Port.BuenosAires, Port.NewYork);
            Assert.AreEqual(6, duration);
        }

        [Test]
        public void Find_Shortest_Journey_Buenos_Liverpool_Is_8_days()
        {
            var duration = scenario.CalculateShortestJourney(Port.BuenosAires, Port.Liverpool);
            Assert.AreEqual(8, duration);
        }

        [Test]
        public void Find_Shortest_Journey_NewYork_NewYork_Is_18_days()
        {
            var duration = scenario.CalculateShortestJourney(Port.NewYork, Port.NewYork);
            Assert.AreEqual(18, duration);
        }

        [Test]
        public void Find_Number_Of_Routes_Liverpool_Liverpool_Where_Stops_LE_3()
        {
            const int maxStops = 3;
            var count = scenario.GetJourneysWithMaxStops(Port.Liverpool, Port.Liverpool, maxStops);
            Assert.AreEqual(2, count);
        }

        [Test]
        public void Find_Number_Of_Routes_Buenos_Liverpool_Where_Stops_Is_4()
        {
            const int totalStops = 4;
            var count = scenario.GetJourneysWithExactStops(Port.BuenosAires, Port.Liverpool, totalStops);
            Assert.AreEqual(0, count);
        }

        [Test]
        public void Find_Number_Of_Routes_Liverpool_Liverpool_Where_Duration_LE_25()
        {
            const int duration = 25;
            var count = scenario.GetJourneysWithinDuration(Port.Liverpool, Port.Liverpool, duration);
            Assert.AreEqual(2, count);
        }
    }
}
