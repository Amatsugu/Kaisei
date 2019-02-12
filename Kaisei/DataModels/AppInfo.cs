using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Kaisei.DataModels
{
	public class AppInfo : ClaimsPrincipal
	{
		public string Id { get; set; }
		public string ApiKey { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public string Hostname { get; set; }
	}
}
