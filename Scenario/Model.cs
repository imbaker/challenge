using System;
using System.Collections.Generic;
using System.Linq;


namespace Hitachi
{
    public class Model
    {
        /// <summary>
        /// Used to store a list of all valid routes in the network.
        /// </summary>
        private IList<Route> m_validRoutes = new List<Route>();

        /// <summary>
        /// Used to store which ports have been stopped at in the current journey
        /// </summary>
        private IList<Port> m_stopsMade = new List<Port>();

        /// <summary>
        /// Used to store a list of all valid journeys for the current Start and End ports.
        /// </summary>
        private IList<Journey> m_validJourneys = new List<Journey>();

        /// <summary>
        /// Used to store the route of the current journey.
        /// </summary>
        private Journey m_currentJourney = new Journey();

        #region Constructors

        public Model()
        {
            m_validRoutes.Add(new Route() { Start = Port.BuenosAires, End = Port.NewYork, Duration = 6 });
            m_validRoutes.Add(new Route() { Start = Port.BuenosAires, End = Port.Casablanca, Duration = 5 });
            m_validRoutes.Add(new Route() { Start = Port.BuenosAires, End = Port.CapeTown, Duration = 4 });
            m_validRoutes.Add(new Route() { Start = Port.NewYork, End = Port.Liverpool, Duration = 4 });
            m_validRoutes.Add(new Route() { Start = Port.Liverpool, End = Port.Casablanca, Duration = 3 });
            m_validRoutes.Add(new Route() { Start = Port.Liverpool, End = Port.CapeTown, Duration = 6 });
            m_validRoutes.Add(new Route() { Start = Port.Casablanca, End = Port.Liverpool, Duration = 3 });
            m_validRoutes.Add(new Route() { Start = Port.Casablanca, End = Port.CapeTown, Duration = 6 });
            m_validRoutes.Add(new Route() { Start = Port.CapeTown, End = Port.NewYork, Duration = 8 });
        }

        #endregion

        #region Public Methods

        public IList<Route> ListValidRoutes()
        {
            return m_validRoutes;
        }

        /// <summary>
        /// Calculates the total journey time for a given list of Ports.
        /// </summary>
        /// <param name="ports">A List containing the ports to be visited</param>
        /// <returns>An integer representing the total journey time</returns>
        /// <exception cref="ArgumentException">If no valid route exists between two ports specified then an ArgumentException is thrown</exception>
        public int TotalJourneyTime(List<Port> ports)
        {
            var duration = 0;
            for (int i = 0; i < ports.Count()-1; i++)
            {
                var start = ports[i];
                var end = ports[i + 1];

                try
                {
                    Route currentRoute = m_validRoutes.Where(s => s.Start == start).Where(e => e.End == end).Single();
                    duration += currentRoute.Duration;
                }
                catch (InvalidOperationException e)
                {
                    throw new ArgumentException("Invalid route: " + start + " to " + end + " is not a valid route");
                }
            }

            return duration;
        }

        /// <summary>
        /// Calculates the shortest journey between two ports
        /// </summary>
        /// <param name="start">The Port the journey starts from</param>
        /// <param name="end">The Port the journey finishes at</param>
        /// <returns>An integer representing the shortest journey duration (in days)</returns>
        public int CalculateShortestJourney(Port start, Port end)
        {
            CalculateValidJourneys(start, end);
            
            return m_validJourneys.Min(p => p.Duration);
        }

        /// <summary>
        /// Calculates the number of journeys with a maximum number of stops.
        /// </summary>
        /// <param name="start">The Port the journey starts from</param>
        /// <param name="end">The Port the journey finishes at</param>
        /// <param name="maxStops">An integer indicating the maximum number of stops allowed</param>
        /// <returns>An integer indicating the number of journeys that have maxStops or fewer stops</returns>
        public int GetJourneysWithMaxStops(Port start, Port end, int maxStops)
        {
            CalculateValidJourneys(start, end);
            return m_validJourneys.Where(m => m.Stops() <= maxStops).Count();
        }

        /// <summary>
        /// Calculates the number of journeys with an exact number of stops.
        /// </summary>
        /// <param name="start">The Port the journey starts from</param>
        /// <param name="end">The Port the journey finishes at</param>
        /// <param name="totalStops">An integer indicating the number of stops allowed</param>
        /// <returns>An integer indicating the number of journeys that have precisely totalStops</returns>
        public object GetJourneysWithExactStops(Port start, Port end, int totalStops)
        {
            CalculateValidJourneys(start, end);
            return m_validJourneys.Where(m => m.Stops() == totalStops).Count();
        }

        /// <summary>
        /// Calculates the number of journeys within a specified duration.
        /// </summary>
        /// <param name="start">The Port the journey starts from</param>
        /// <param name="end">The Port the journey finishes at</param>
        /// <param name="duration">An integer indicating the maximum duration allowed</param>
        /// <returns>An integer indicating the number of journeys that are within the specified duration</returns>
        public object GetJourneysWithinDuration(Port start, Port end, int duration)
        {
            CalculateValidJourneys(start, end);
            return m_validJourneys.Where(m => m.Duration <= duration).Count();
        }

        public void PrintJourneyDetails()
        {
            foreach (Journey current in m_validJourneys)
            {
                Console.WriteLine("Stops: {0}", current.ToString());
                Console.WriteLine("Count: {0}\tStops: {1}\tDuration: {2}", current.Count(), current.Stops(), current.Duration);
            }
        }

        #endregion

        #region Private Methods
        private void CalculateValidJourneys(Port start, Port end)
        {
            m_validJourneys.Clear();

            // Only work with routes that start from the starting port
            foreach (Route currentRoute in m_validRoutes.Where(s => s.Start == start))
            {
                Console.WriteLine("----------------Looking for a new route---------------");
                int currentTotalDuration = 0;
                ClearJourneyDetails();
                m_stopsMade.Clear();

                Console.WriteLine("Looking for: {0} -> {1}", start, end);
                Console.WriteLine("Found: {0} -> {1} - {2}", currentRoute.Start, currentRoute.End, currentRoute.Duration);
                
                // Store the first destination for subsequent circular references checks.
                m_stopsMade.Add(currentRoute.End);

                // If the route we've identified is not a direct route then...
                if (currentRoute.End != end)
                {
                    try
                    {
                        // ... recurse looking for a journey that starts with the current end point and ends with the original end point
                        currentTotalDuration += this.CalculateSubJourneys(currentRoute.End, end);
                        // Add the initial route's duration onto CurrentTotalDuration
                        currentTotalDuration += currentRoute.Duration;
                    }
                    catch (InvalidOperationException e)
                    {
                        Console.WriteLine(e.Message);
                        Console.WriteLine("Continuing with next route");
                    }
                }
                else
                {
                    // ... otherwise store the currentRoute.Duration
                    currentTotalDuration = currentRoute.Duration;
                }
                StoreJourneyDetails(currentRoute, currentTotalDuration);
            }
        }

        private int CalculateSubJourneys(Port start, Port end)
        {
            int currentTotalDuration = 0;

            foreach (Route currentRoute in m_validRoutes.Where(s => s.Start == start))
            {
                Console.WriteLine("Looking for: {0} -> {1}", start, end);
                Console.WriteLine("Found: {0} -> {1} - {2}", currentRoute.Start, currentRoute.End, currentRoute.Duration);
                // Check that the current destination has not been visited before (to prevent circular references).
                if (!m_stopsMade.Contains(currentRoute.End))
                {
                    m_stopsMade.Add(currentRoute.End);
                }
                else
                {
                    throw new InvalidOperationException("Circular Reference - " + currentRoute.Start + " -> " + currentRoute.End);
                }

                // If the route we've identified is the final route then...
                if (currentRoute.End == end)
                {
                    // ... Store this route into the journey, and start to return.
                    StoreJourneyDetails(currentRoute);
                    return currentRoute.Duration;
                }
                else
                {
                    try
                    {
                        // ... recurse looking for a journey that starts with the current end point and ends with the original end point
                        currentTotalDuration += this.CalculateSubJourneys(currentRoute.End, end);
                        currentTotalDuration += currentRoute.Duration;
                        StoreJourneyDetails(currentRoute);
                        return currentTotalDuration;
                    }
                    catch (InvalidOperationException e)
                    {
                        Console.WriteLine(e.Message);
                        Console.WriteLine("Continuing with next route");
                    }
                }
            }

            // Can only reach this point if there are no routes for the current start port.
            throw new ArgumentException("Invalid route: " + start + " to " + end + " is not a valid route");
        }

        private void ClearJourneyDetails()
        {
            m_currentJourney = new Journey();
        }

        private void StoreJourneyDetails(Route route)
        {
            m_currentJourney.Add(route);
        }

        private void StoreJourneyDetails(Route route, int duration)
        {
            m_currentJourney.Add(route);
            m_currentJourney.Duration = duration;

            m_validJourneys.Add(m_currentJourney);
        }
        #endregion
    }
    
    /// <summary>
    /// Stores valid port names
    /// </summary>
    public enum Port
    {
        BuenosAires = 1,
        CapeTown = 2,
        Casablanca = 3,
        Liverpool = 4,
        NewYork = 5
    }

    /// <summary>
    /// Represents a portion of a journey, that has a defined start port and end port.
    /// </summary>
    public class Route
    {
        public Port Start {get; set; }
        public Port End { get; set; }
        public int Duration { get; set; }
    }

    /// <summary>
    /// Represents the routes that combine to make up a valid journey.
    /// </summary>
    public class Journey
    {
        private IList<Route> routes = new List<Route>();

        public int Duration { get; set; }

        public void Add(Route route)
        {
            routes.Add(route);
        }

        public void Clear()
        {
            routes = new List<Route>();
            Duration = 0;
        }

        public int Count()
        {
            return routes.Count();
        }

        public int Stops()
        {
            return this.Count() - 1;
        }

        public override string ToString()
        {
            string output = "";
            // Need to reverse the routes list as they will have been added in backwards due to recursion
            foreach (Route route in routes.Reverse())
                if (output == "")
                {
                    output += route.Start;
                    output += " -> ";
                    output += route.End;
                }
                else
                    output += (" -> " + route.End);

            return output;
        }
    }
}
