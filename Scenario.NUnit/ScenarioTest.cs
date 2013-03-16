using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

namespace Hitachi.NUnit
{
    [TestFixture]
    public class ScenarioTest
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
            Port[] ports = { Port.BuenosAires, Port.NewYork, Port.Liverpool };
            var duration = scenario.Route(ports);
            Assert.AreEqual(10, duration);
        }

        [Test]
        public void Total_Journey_Time_Buenos_Casablanca_Liverpool_Is_8_days()
        {
            Port[] ports = { Port.BuenosAires, Port.Casablanca, Port.Liverpool };
            var duration = scenario.Route(ports);
            Assert.AreEqual(8, duration);
        }

        [Test]
        public void Total_Journey_Time_Buenos_Capetown_NY_Liverpool_Casablanca_Is_19_days()
        {
            Port[] ports = { Port.BuenosAires, Port.CapeTown, Port.NewYork, Port.Liverpool, Port.Casablanca };
            var duration = scenario.Route(ports);
            Assert.AreEqual(19, duration);
        }

        [Test]
        public void Journey_Buenos_Capetown_Casablanca_Is_Invalid()
        {
            Port[] ports = { Port.CapeTown, Port.Casablanca };
            Assert.Throws<ArgumentException>( () => { scenario.Route(ports); } );
        }

        [Test]
        public void Find_Shortest_Journey_Buenos_NY_Is_6_days()
        {
            var duration = scenario.ShortestJourney(Port.BuenosAires, Port.NewYork);
            Assert.AreEqual(6, duration);
        }

        [Test]
        public void Find_Shortest_Journey_Buenos_Liverpool_Is_8_days()
        {
            var duration = scenario.ShortestJourney(Port.BuenosAires, Port.Liverpool);
            Assert.AreEqual(8, duration);
        }

        [Test]
        public void Find_Shortest_Journey_NewYork_NewYork_Is_18_days()
        {
            var duration = scenario.ShortestJourney(Port.NewYork, Port.NewYork);
            Assert.AreEqual(18, duration);
        }

        [Test]
        public void Find_Number_Of_Routes_Liverpool_Liverpool_Where_Stops_LE_3()
        {
            const int maxStops = 3;
            var count = scenario.GetRoutes(Port.Liverpool, Port.Liverpool, maxStops);
            Assert.AreEqual(2, count);
        }
    }
}
