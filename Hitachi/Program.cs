using System;

using Hitachi;

namespace Runner
{
    class Program
    {
        private const string BuenosAires = "Buenos Aires";
        private const string Casablanca = "Casablanca";
        private const string CapeTown = "Cape Town";
        private const string Liverpool = "Liverpool";
        private const string NewYork = "New York";

        static void Main(string[] args)
        {
            Model model = new Model();
            model.AddValidRoute(new Route() { Start = BuenosAires, End = NewYork, Duration = 6 });
            model.AddValidRoute(new Route() { Start = BuenosAires, End = Casablanca, Duration = 5 });
            model.AddValidRoute(new Route() { Start = BuenosAires, End = CapeTown, Duration = 4 });
            model.AddValidRoute(new Route() { Start = NewYork, End = Liverpool, Duration = 4 });
            model.AddValidRoute(new Route() { Start = Liverpool, End = Casablanca, Duration = 3 });
            model.AddValidRoute(new Route() { Start = Liverpool, End = CapeTown, Duration = 6 });
            model.AddValidRoute(new Route() { Start = Casablanca, End = Liverpool, Duration = 3 });
            model.AddValidRoute(new Route() { Start = Casablanca, End = CapeTown, Duration = 6 });
            model.AddValidRoute(new Route() { Start = CapeTown, End = NewYork, Duration = 8 });
            Console.WriteLine(model.CountValidRoutes());
            Console.ReadLine();
        }
    }
}
