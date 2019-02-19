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
			var host = new NancyHost(new HostConfiguration(), new Uri(hostUri));
			host.Start();
			Console.WriteLine(KaiseiCore.Users.Find("{ id : 'iV71m5jq0EiqpVk5acoHTg'}").First());
			/*var app = KaiseiCore.RegisterApp(new DataModels.AppInfo
			{
				Hostname = "localhost:4321",
				Name = "Aoba Test",
				Description = "Test app for Aoba"
			}, "FwgbjHcrVU6gThnWkkx2_g");
			Console.WriteLine(JsonConvert.SerializeObject(app));
			File.WriteAllText("app.json", JsonConvert.SerializeObject(app));*/
			//var authid = KaiseiCore.AuthorizeApp(appId: "iV71m5jq0EiqpVk5acoHTg", userId: "FgLoaTwqwkS6+8mwW_lx9w");
			//var userid = KaiseiCore.ConfirmAuthorization(apiKey: "JvmIM_20E0mwxCmEZvRi2A", authId: authid);
			Console.WriteLine($"Hosting at {hostUri}");
			Console.ReadLine();
			host.Dispose();
        }
    }
}
