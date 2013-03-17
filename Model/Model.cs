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
        /// Used to store a list of all valid ports in the network.
        /// </summary>
        private HashSet<string> m_validPorts = new HashSet<string>();

        /// <summary>
        /// Used to store which ports have been stopped at in the current journey
        /// </summary>
        private IList<string> m_stopsMade = new List<string>();

        /// <summary>
        /// Used to store a list of all valid journeys for the current Start and End ports.
        /// </summary>
        private IList<Journey> m_validJourneys = new List<Journey>();

        /// <summary>
        /// Used to store the route of the current journey.
        /// </summary>
        private Journey m_currentJourney = new Journey();

        #region Public Methods

        /// <summary>
        /// Adds a new route to the Model's network
        /// </summary>
        /// <param name="route">A Route object containing a valid route</param>
        public void AddValidRoute(Route route)
        {
            if (route.Start == route.End)
                throw new ArgumentException("Route Start and End cannot be the same.");

            if (route.Duration <= 0)
                throw new ArgumentException("Route Duration must be greater than zero.");

            if (!m_validRoutes.Contains(route))
                m_validRoutes.Add(route);

            m_validPorts.Add(route.Start);
            m_validPorts.Add(route.End);
        }

        /// <summary>
        /// Returns a count of the number of routes in the Model's network
        /// </summary>
        /// <returns>An integer indicating the number of routes</returns>
        public int CountValidRoutes()
        {
            return m_validRoutes.Count();
        }

        /// <summary>
        /// Calculates the total journey time for a given list of Ports.
        /// </summary>
        /// <param name="ports">A List containing the ports to be visited</param>
        /// <returns>An integer representing the total journey time</returns>
        /// <exception cref="ArgumentOutOfRangeException">If one of the ports passed in does not exist in the current network</exception>
        /// <exception cref="ArgumentException">If no valid route exists between two ports specified then an ArgumentException is thrown</exception>
        public int TotalJourneyTime(List<string> ports)
        {
            ValidatePorts(ports);

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
                catch (InvalidOperationException) 
                {
                    throw new ArgumentException("Invalid route: " + start + " to " + end + " is not a valid route.");
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
        public int CalculateShortestJourney(string start, string end)
        {
            ValidatePorts(new List<string>() { start, end });

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
        public int GetJourneysWithMaxStops(string start, string end, int maxStops)
        {
            ValidatePorts(new List<string>() { start, end });

            if (maxStops <= 0)
                throw new ArgumentOutOfRangeException("maxStops", maxStops, "Value must be greater than zero.");

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
        public object GetJourneysWithExactStops(string start, string end, int totalStops)
        {
            ValidatePorts(new List<string>() { start, end });

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
        public object GetJourneysWithinDuration(string start, string end, int duration)
        {
            ValidatePorts(new List<string>() { start, end });

            CalculateValidJourneys(start, end);
            return m_validJourneys.Where(m => m.Duration <= duration).Count();
        }

        /// <summary>
        /// Utility method to print out the current contents of m_validJourneys
        /// </summary>
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
        private void ValidatePorts(IList<string> ports)
        {
            foreach (string port in ports)
            {
                if (!m_validPorts.Contains(port))
                    throw new ArgumentOutOfRangeException("ports", port, "Invalid port");
            }
        }

        private void CalculateValidJourneys(string start, string end)
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

        private int CalculateSubJourneys(string start, string end)
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
    /// Represents a portion of a journey, that has a defined start port and end port.
    /// </summary>
    public class Route
    {
        public string Start {get; set; }
        public string End { get; set; }
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
