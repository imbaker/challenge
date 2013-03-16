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
            Port[] ports = { Port.BuenosAires, Port.NewYork, Port.Liverpool };
            var distance = scenario.Route(ports);
            var duration = scenario.ShortestJourney(Port.BuenosAires, Port.NewYork);
            Console.WriteLine("Duration: {0}", duration);
            duration = scenario.ShortestJourney(Port.BuenosAires, Port.Liverpool);
            Console.WriteLine("Duration: {0}", duration);
            duration = scenario.ShortestJourney(Port.NewYork, Port.NewYork);
            Console.WriteLine("Duration: {0}", duration);
            Console.ReadLine();
        }
    }
}
