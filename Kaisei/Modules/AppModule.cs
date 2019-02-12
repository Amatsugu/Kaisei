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
				return Response.AsJson(KaiseiCore.GetAppInfo((string)p.id));
			});

			Get("/user/{id}", p =>
			{
				if (Context.CurrentUser == null)
					return new Response
					{
						StatusCode = HttpStatusCode.Unauthorized
					};
				var apiKey = ((AppInfo)Context.CurrentUser).ApiKey;
				return KaiseiCore.GetAppUser(apiKey, p.id);
			});

			Post("/sso/confirm", _ =>
			{
				if (Context.CurrentUser == null)
					return new Response
					{
						StatusCode = HttpStatusCode.Unauthorized
					};
				var sso = this.Bind();
				var app = (AppInfo)Context.CurrentUser;
				return KaiseiCore.ConfirmAuthorization(app.ApiKey, sso.authId);
			});
		}
	}
}
