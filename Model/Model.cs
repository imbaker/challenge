namespace Hitachi
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    
    public class Model
    {
        /// <summary>
        /// Used to store a list of all valid routes in the network.
        /// </summary>
        private IList<Route> validRoutes = new List<Route>();

        /// <summary>
        /// Used to store a list of all valid ports in the network.
        /// </summary>
        private HashSet<string> validPorts = new HashSet<string>();

        /// <summary>
        /// Used to store which ports have been stopped at in the current journey
        /// </summary>
        private IList<string> stopsMade = new List<string>();

        /// <summary>
        /// Used to store a list of all valid journeys for the current Start and End ports.
        /// </summary>
        private IList<Journey> validJourneys = new List<Journey>();

        /// <summary>
        /// Used to store the route of the current journey.
        /// </summary>
        private Journey currentJourney = new Journey();

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

            if (!this.validRoutes.Contains(route))
                this.validRoutes.Add(route);

            this.validPorts.Add(route.Start);
            this.validPorts.Add(route.End);
        }

        /// <summary>
        /// Returns a count of the number of routes in the Model's network
        /// </summary>
        /// <returns>An integer indicating the number of routes</returns>
        public int CountValidRoutes()
        {
            return this.validRoutes.Count();
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
            this.ValidatePorts(ports);

            var duration = 0;
            for (int i = 0; i < ports.Count() - 1; i++)
            {
                var start = ports[i];
                var end = ports[i + 1];

                try
                {
                    Route currentRoute = this.validRoutes.Where(s => s.Start == start).Where(e => e.End == end).Single();
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
            this.ValidatePorts(new List<string>() { start, end });

            this.CalculateValidJourneys(start, end);
            
            return this.validJourneys.Min(p => p.Duration());
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
            this.ValidatePorts(new List<string>() { start, end });

            if (maxStops <= 0)
                throw new ArgumentOutOfRangeException("maxStops", maxStops, "Value must be greater than zero.");

            this.CalculateValidJourneys(start, end);
            return this.validJourneys.Where(m => m.Stops() <= maxStops).Count();
        }

        /// <summary>
        /// Calculates the number of journeys with an exact number of stops.
        /// </summary>
        /// <param name="start">The Port the journey starts from</param>
        /// <param name="end">The Port the journey finishes at</param>
        /// <param name="totalStops">An integer indicating the number of stops allowed</param>
        /// <returns>An integer indicating the number of journeys that have precisely totalStops</returns>
        public int GetJourneysWithExactStops(string start, string end, int totalStops)
        {
            this.ValidatePorts(new List<string>() { start, end });

            this.CalculateValidJourneys(start, end);
            return this.validJourneys.Where(m => m.Stops() == totalStops).Count();
        }

        /// <summary>
        /// Calculates the number of journeys within a specified duration.
        /// </summary>
        /// <param name="start">The Port the journey starts from</param>
        /// <param name="end">The Port the journey finishes at</param>
        /// <param name="duration">An integer indicating the maximum duration allowed</param>
        /// <returns>An integer indicating the number of journeys that are within the specified duration</returns>
        public int GetJourneysWithinDuration(string start, string end, int duration)
        {
            this.ValidatePorts(new List<string>() { start, end });

            this.CalculateValidJourneys(start, end);
            return this.validJourneys.Where(m => m.Duration() <= duration).Count();
        }

        /// <summary>
        /// Utility method to print out the current contents of validJourneys
        /// </summary>
        public void WriteJourneyDetails()
        {
            foreach (Journey current in this.validJourneys)
            {
                Console.WriteLine("Stops: {0}", current.ToString());
                Console.WriteLine("Stops: {0}\tDuration: {1}", current.Stops(), current.Duration());
            }
        }

        #endregion

        #region Private Methods
        private void ValidatePorts(IList<string> ports)
        {
            foreach (string port in ports)
            {
                if (!this.validPorts.Contains(port))
                    throw new ArgumentOutOfRangeException("ports", port, "Invalid port");
            }
        }

        private void CalculateValidJourneys(string start, string end)
        {
            this.validJourneys.Clear();

            // Only work with routes that start from the starting port
            foreach (Route currentRoute in this.validRoutes.Where(s => s.Start == start))
            {
                Console.WriteLine("----------------Looking for a new route---------------");
                int currentTotalDuration = 0;
                this.ClearJourneyDetails();
                this.stopsMade.Clear();

                Console.WriteLine("Looking for: {0} -> {1}", start, end);
                Console.WriteLine("Found: {0} -> {1} - {2}", currentRoute.Start, currentRoute.End, currentRoute.Duration);
                
                // Store the first destination for subsequent circular references checks.
                this.stopsMade.Add(currentRoute.End);

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

                this.StoreJourneyDetails(currentRoute, currentTotalDuration);
            }
        }

        private int CalculateSubJourneys(string start, string end)
        {
            int currentTotalDuration = 0;

            foreach (Route currentRoute in this.validRoutes.Where(s => s.Start == start))
            {
                Console.WriteLine("Looking for: {0} -> {1}", start, end);
                Console.WriteLine("Found: {0} -> {1} - {2}", currentRoute.Start, currentRoute.End, currentRoute.Duration);

                // Check that the current destination has not been visited before (to prevent circular references).
                if (!this.stopsMade.Contains(currentRoute.End))
                {
                    this.stopsMade.Add(currentRoute.End);
                }
                else
                {
                    throw new InvalidOperationException("Circular Reference - " + currentRoute.Start + " -> " + currentRoute.End);
                }

                // If the route we've identified is the final route then...
                if (currentRoute.End == end)
                {
                    // ... Store this route into the journey, and start to return.
                    this.StoreJourneyDetails(currentRoute);
                    return currentRoute.Duration;
                }
                else
                {
                    try
                    {
                        // ... recurse looking for a journey that starts with the current end point and ends with the original end point
                        currentTotalDuration += this.CalculateSubJourneys(currentRoute.End, end);
                        currentTotalDuration += currentRoute.Duration;
                        this.StoreJourneyDetails(currentRoute);
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
            this.currentJourney = new Journey();
        }

        private void StoreJourneyDetails(Route route)
        {
            this.currentJourney.Add(route);
        }

        private void StoreJourneyDetails(Route route, int duration)
        {
            this.currentJourney.Add(route);

            this.validJourneys.Add(this.currentJourney);
        }
        #endregion
    }
    
    /// <summary>
    /// Represents a portion of a journey, that has a defined start port and end port and duration.
    /// </summary>
    public class Route
    {
        public string Start { get; set; }

        public string End { get; set; }

        public int Duration { get; set; }
    }

    /// <summary>
    /// Represents the routes that combine to make up a valid journey.
    /// </summary>
    public class Journey
    {
        private IList<Route> routes = new List<Route>();

        private int duration = 0;

        public void Add(Route route)
        {
            this.routes.Add(route);
            this.duration += route.Duration;
        }

        public void Clear()
        {
            this.routes = new List<Route>();
            this.duration = 0;
        }

        public int Duration()
        {
            return this.duration;
        }

        public int Stops()
        {
            return this.routes.Count();
        }

        public override string ToString()
        {
            string output = string.Empty;

            // Need to reverse the routes list as they will have been added in backwards due to recursion
            foreach (Route route in this.routes.Reverse())
            {
                if (output == string.Empty)
                    output += route.Start + " -(" + route.Duration + ")-> " + route.End;
                else
                    output += " -(" + route.Duration + ")-> " + route.End;
            }

            return output;
        }
    }
}
