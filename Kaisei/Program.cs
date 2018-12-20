using System;
using Nancy.Hosting.Self;

namespace Kaisei
{
    class Program
    {
        static void Main(string[] args)
        {
			var hostUri = "http://localhost:6130";
			var host = new NancyHost(new HostConfiguration(), new Uri(hostUri));
			host.Start();
			//var authid = KaiseiCore.AuthorizeApp(appId: "iV71m5jq0EiqpVk5acoHTg", userId: "FgLoaTwqwkS6+8mwW_lx9w");
			//var userid = KaiseiCore.ConfirmAuthorization(apiKey: "JvmIM_20E0mwxCmEZvRi2A", authId: authid);
			Console.WriteLine($"Hosting at {hostUri}");
			Console.ReadLine();
			host.Dispose();
        }
    }
}
