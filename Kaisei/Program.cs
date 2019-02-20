using System;
using System.IO;
using MongoDB.Driver;
using Nancy.Hosting.Self;
using Newtonsoft.Json;

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
			Console.WriteLine(KaiseiCore.Users.Find("{ id : 'iV71m5jq0EiqpVk5acoHTg'}").First());
			Console.WriteLine($"Hosting at {hostUri}");
			Console.ReadLine();
			host.Dispose();
        }
    }
}
