using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Hitachi;

namespace Runner
{
    class Program
    {
        static void Main(string[] args)
        {
            var scenario = new Hitachi.Model();
            Console.WriteLine(scenario.ListValidRoutes());
            List<Port> ports = new List<Port>() { Port.BuenosAires, Port.NewYork, Port.Liverpool };
            var distance = scenario.TotalJourneyTime(ports);
            var duration = scenario.CalculateShortestJourney(Port.BuenosAires, Port.NewYork);
            Console.WriteLine("Duration: {0}", duration);
            duration = scenario.CalculateShortestJourney(Port.BuenosAires, Port.Liverpool);
            Console.WriteLine("Duration: {0}", duration);
            duration = scenario.CalculateShortestJourney(Port.NewYork, Port.NewYork);
            scenario.PrintJourneyDetails();
            Console.WriteLine("Duration: {0}", duration);
            Console.ReadLine();
        }
    }
}
