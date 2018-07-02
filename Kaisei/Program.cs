using System;
using Nancy.Hosting.Self;

namespace Kaisei
{
    class Program
    {
        static void Main(string[] args)
        {
			var hostUri = "http://localhost:6130";
			var host = new NancyHost(new HostConfiguration
			{
				UrlReservations = new UrlReservations
				{
					CreateAutomatically = true
				}
			}, new Uri(hostUri));
			host.Start();
			Console.WriteLine($"Hosting at {hostUri}");
			Console.ReadLine();
			host.Dispose();
        }
    }
}
