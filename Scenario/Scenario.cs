using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections.Specialized;

namespace Hitachi
{
    public class Model
    {
        private IList<Route> m_ValidRoutes = new List<Route>();
        private IList<Port> m_stops = new List<Port>();
        private IList<Journey> m_ValidJourneys = new List<Journey>();
        private int m_currentStackDepth = 0;

        public Model()
        {
            m_ValidRoutes.Add(new Route() { Start = Port.BuenosAires, End = Port.NewYork, Duration = 6 });
            m_ValidRoutes.Add(new Route() { Start = Port.BuenosAires, End = Port.Casablanca, Duration = 5 });
            m_ValidRoutes.Add(new Route() { Start = Port.BuenosAires, End = Port.CapeTown, Duration = 4 });
            m_ValidRoutes.Add(new Route() { Start = Port.NewYork, End = Port.Liverpool, Duration = 4 });
            m_ValidRoutes.Add(new Route() { Start = Port.Liverpool, End = Port.Casablanca, Duration = 3 });
            m_ValidRoutes.Add(new Route() { Start = Port.Liverpool, End = Port.CapeTown, Duration = 6 });
            m_ValidRoutes.Add(new Route() { Start = Port.Casablanca, End = Port.Liverpool, Duration = 3 });
            m_ValidRoutes.Add(new Route() { Start = Port.Casablanca, End = Port.CapeTown, Duration = 6 });
            m_ValidRoutes.Add(new Route() { Start = Port.CapeTown, End = Port.NewYork, Duration = 8 });
        }

        public IList<Route> ListValidRoutes()
        {
            return m_ValidRoutes;
        }

        public int Route(Port[] ports)
        {
            var duration = 0;
            for (int i = 0; i < ports.GetUpperBound(0); i++)
            {

                var start = ports[i];
                var end = ports[i + 1];

                try
                {
                    Route currentRoute = m_ValidRoutes.Where(s => s.Start == start).Where(e => e.End == end).Single();
                    duration += currentRoute.Duration;
                }
                catch (InvalidOperationException e)
                {
                    throw new ArgumentException("Invalid route: " + start + " to " + end + " is not a valid route");
                }
            }

            return duration;
        }

        public int ShortestJourney(Port start, Port end)
        {
            CalculateJourneys(start, end);
            
            return m_ValidJourneys.Min(p => p.Duration);
        }

        private int CalculateJourneys(Port start, Port end)
        {
            int currentTotalDuration = 0;

            m_currentStackDepth++;

            foreach (Route currentRoute in m_ValidRoutes.Where(s => s.Start == start))
            {
                // If this is the top level call then reset the working variables.
                if (m_currentStackDepth == 1)
                {
                    Console.WriteLine("----------------Looking for a new route---------------");
                    currentTotalDuration = 0;
                    m_stops.Clear();
                }

                Console.WriteLine("{0}: Looking for: {1} -> {2}", m_currentStackDepth, start, end);
                Console.WriteLine("{0}: Found: {1} -> {2} - {3}", m_currentStackDepth, currentRoute.Start, currentRoute.End, currentRoute.Duration);
                // Check that the current destination has not been visited before (to prevent circular references).
                if (!m_stops.Contains(currentRoute.End))
                {
                    Console.WriteLine("Adding {0} to portsVisited", currentRoute.End);
                    m_stops.Add(currentRoute.End);
                }
                else
                {
                    m_currentStackDepth--;
                    throw new InvalidOperationException("Circular Reference");
                }

                // Add the current route's duration onto the total count
                currentTotalDuration += currentRoute.Duration;
                Console.WriteLine("{1}: CurrentTotalDuration[1]: {0}", currentTotalDuration, m_currentStackDepth);

                // If the route we've identified is not the final route then...
                if (currentRoute.End != end)
                {
                    try
                    {
                        // ... recurse looking for a journey that starts with the current end point and ends with the original end point
                        currentTotalDuration += this.CalculateJourneys(currentRoute.End, end);
                        // Only store a shortestDuration if we're fully unwound
                        if (m_currentStackDepth == 1)
                        {
                            StoreJourneyDetails(start, m_stops, currentTotalDuration);
                            // TODO: Remove next line?
                            currentTotalDuration = 0;
                        }
                        else
                        {
                            Console.WriteLine("{1}: CurrentTotalDuration[2]: {0}", currentTotalDuration, m_currentStackDepth);
                            Console.WriteLine("pop stack");
                            m_currentStackDepth--;
                            return currentTotalDuration;
                        }
                    }
                    catch (InvalidOperationException e)
                    {
                        Console.WriteLine("{0}: Circular reference, continuing with next route", m_currentStackDepth);
                        currentTotalDuration -= currentRoute.Duration;
                    }
                }
                else
                {
                    // Only store journeydetails if we're fully unwound
                    if (m_currentStackDepth == 1)
                    {
                        StoreJourneyDetails(start, m_stops, currentTotalDuration);
                        Console.WriteLine("Ports visited: {0}", m_stops.Count);
                        Console.WriteLine("{1}: TotalDuration for route: {0}", currentTotalDuration, m_currentStackDepth);
                        //TODO: Remove next line?
                        currentTotalDuration = 0;
                    }
                    else
                    {
                        Console.WriteLine("{1}: CurrentTotalDuration[3]: {0}", currentTotalDuration, m_currentStackDepth);
                        Console.WriteLine("pop stack");
                        m_currentStackDepth--;
                        return currentTotalDuration;
                    }
                }
            }

            m_currentStackDepth--;
            return 0;
        }

        private int CalculateSubJourneys(Port start, Port end)
        {
            int currentTotalDuration = 0;

            foreach (Route currentRoute in m_ValidRoutes.Where(s => s.Start == start))
            {
                Console.WriteLine("{0}: Looking for: {1} -> {2}", m_currentStackDepth, start, end);
                Console.WriteLine("{0}: Found: {1} -> {2} - {3}", m_currentStackDepth, currentRoute.Start, currentRoute.End, currentRoute.Duration);
                // Check that the current destination has not been visited before (to prevent circular references).
                if (!m_stops.Contains(currentRoute.End))
                {
                    Console.WriteLine("Adding {0} to portsVisited", currentRoute.End);
                    m_stops.Add(currentRoute.End);
                }
                else
                {
                    m_currentStackDepth--;
                    throw new InvalidOperationException("Circular Reference");
                }

                // Add the current route's duration onto the total count
                currentTotalDuration += currentRoute.Duration;
                Console.WriteLine("{1}: CurrentTotalDuration[1]: {0}", currentTotalDuration, m_currentStackDepth);

                // If the route we've identified is the final route then...
                if (currentRoute.End == end)
                {
                    Console.WriteLine("{1}: CurrentTotalDuration[3]: {0}", currentTotalDuration, m_currentStackDepth);
                    return currentTotalDuration;
                }
                else
                {
                    try
                    {
                        // ... recurse looking for a journey that starts with the current end point and ends with the original end point
                        currentTotalDuration += this.CalculateSubJourneys(currentRoute.End, end);
                        // Only store a shortestDuration if we're fully unwound
                        Console.WriteLine("{1}: CurrentTotalDuration[2]: {0}", currentTotalDuration, m_currentStackDepth);
                        return currentTotalDuration;
                    }
                    catch (InvalidOperationException e)
                    {
                        Console.WriteLine("{0}: Circular reference, continuing with next route", m_currentStackDepth);
                        currentTotalDuration -= currentRoute.Duration;
                    }
                }
            }

            return 0;
        }

        private void StoreJourneyDetails(Port startPort, IList<Port> stops, int duration)
        {
            var currentJourney = new Journey();

            currentJourney.Add(startPort);

            foreach (Port stop in stops)
                currentJourney.Add(stop);

            currentJourney.Duration = duration;

            m_ValidJourneys.Add(currentJourney);
        }

        public void PrintJourneyDetails()
        {
            foreach (Journey current in m_ValidJourneys)
            {
                Console.WriteLine("Stops: {0}", current.ToString());
                Console.WriteLine("Count: {0}\tStops: {1}\tDuration: {2}", current.Count(),current.Stops(), current.Duration);
            }
        }

        public int GetRoutes(Port start, Port end, int maxStops)
        {
            CalculateJourneys(start, end);

            return m_ValidJourneys.Where(m => m.Stops() <= maxStops).Count() ;
        }
    }

    public enum Port
    {
        BuenosAires = 1,
        CapeTown = 2,
        Casablanca = 3,
        Liverpool = 4,
        NewYork = 5
    }

    public class Route
    {
        public Port Start {get; set; }
        public Port End { get; set; }
        public int Duration { get; set; }
    }

    public class Journey
    {
        private IList<Port> stops = new List<Port>();

        public int Duration { get; set; }

        public void Add(Port stop)
        {
            stops.Add(stop);
        }

        public int Count()
        {
            return stops.Count();
        }

        public int Stops()
        {
            return this.Count() - 1;
        }

        public override string ToString()
        {
            string output = "";
            foreach (Port stop in stops)
                if (output == "")
                    output += stop;
                else
                    output += (" -> " + stop);

            return output;
        }
    }
}
