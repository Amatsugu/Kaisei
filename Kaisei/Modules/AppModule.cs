using System;
using System.Collections.Generic;
using System.Text;
using Kaisei.DataModels;
using Nancy;
using Nancy.Authentication.Stateless;
using Nancy.ModelBinding;

namespace Kaisei.Modules
{
	public class AppModule : NancyModule
	{
		public AppModule() : base("/app")
		{
			StatelessAuthentication.Enable(this, KaiseiCore.StatelessConfig);

			Get("/", _ => Context.CurrentUser);

			Get("/{id}", p =>
			{
				return Response.AsJson(KaiseiCore.GetAppInfo(((string)p.id).Replace(' ', '+')));
			});

			Get("/user/{id}", p =>
			{
				if (!(Context.CurrentUser is AppInfo app))
					return new Response
					{
						StatusCode = HttpStatusCode.Unauthorized
					};
				var apiKey = ((AppInfo)Context.CurrentUser).ApiKey;
				var appUser = KaiseiCore.GetAppUser(apiKey, ((string)p.id).Replace(' ', '+'));
				if (appUser == null)
					return new Response { StatusCode = HttpStatusCode.Unauthorized };
				else
					return Response.AsJson(appUser);
			});

			Post("/sso/confirm", _ =>
			{
				if (!(Context.CurrentUser is AppInfo app))
					return new Response
					{
						StatusCode = HttpStatusCode.Unauthorized
					};
				var sso = this.Bind<SSOData>();
				//var app = (AppInfo)Context.CurrentUser;
				return KaiseiCore.ConfirmAuthorization(app.ApiKey, app.Id, sso.AuthId) ?? new Response { StatusCode = HttpStatusCode.Unauthorized };
			});
		}
	}
}
