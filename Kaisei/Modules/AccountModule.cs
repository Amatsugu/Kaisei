using System;
using System.Collections.Generic;
using System.Text;
using Nancy;
using Nancy.Authentication.Stateless;
using Nancy.Security;
using System.Linq;

namespace Kaisei.Modules
{
	public class AccountModule : NancyModule
	{
		public AccountModule() : base("/account")
		{
			StatelessAuthentication.Enable(this, KaiseiCore.StatelessConfig);
			//this.RequiresAuthentication();
			Get("/", _ =>
			{

				return $"{Context.Request.Headers.Referrer}"; //TODO: Create account page
			});
		}
	}
}
