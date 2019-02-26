using System;
using System.Collections.Generic;
using System.Text;
using Nancy;
using Nancy.Authentication.Stateless;
using Nancy.Security;
using System.Linq;
using Nancy.ModelBinding;
using Newtonsoft.Json;
using Kaisei.DataModels;

namespace Kaisei.Modules
{
	public class AccountModule : NancyModule
	{
		public AccountModule() : base("/account")
		{
			StatelessAuthentication.Enable(this, KaiseiCore.StatelessConfig);
			Get("/", _ =>
			{
				var user = (UserModel)Context.CurrentUser;
				if (user == null)
					return Response.AsRedirect("/");
				return View["account", new
				{
					user.Username,
					user.Email,
					AuthedApps = new object[KaiseiCore.GetUserAuthedApps(user.Id).Length],
					MyApps = new object[KaiseiCore.GetUserApps(user.Id).Length]
				}];
				//return $"{Context.Request.Headers.Referrer}"; //TODO: Create account page
			});

		}
	}
}
