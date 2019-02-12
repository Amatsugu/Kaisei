using System;
using System.Collections.Generic;
using System.Text;
using Nancy;
using Nancy.Authentication.Stateless;

namespace Kaisei.Modules
{
    public class IndexModule : NancyModule
    {
		public IndexModule()
		{
			StatelessAuthentication.Enable(this, KaiseiCore.StatelessConfig);
			Get("/", _ =>
			{
				if (Context.CurrentUser != null)
					return Response.AsRedirect("/account");
				else
					return View["index", new { Callback = "/account", AppId = (string)null }];
			});
		}
    }
}
