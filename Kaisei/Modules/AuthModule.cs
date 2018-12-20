using System;
using System.Collections.Generic;
using System.Text;
using Nancy;
using JWT;
using JWT.Builder;
using JWT.Algorithms;
using Newtonsoft.Json;
using Nancy.ModelBinding;
using Kaisei.DataModels;
using Nancy.Authentication.Stateless;

namespace Kaisei.Modules
{
    public class AuthModule : NancyModule
    {
		public AuthModule() : base("/auth")
		{
			StatelessAuthentication.Enable(this, KaiseiCore.StatelessConfig);
			Post("/login", _ =>
			{
				var user = this.Bind<UserCredentials>();
				var verifiedUser = KaiseiCore.VerifyUser(user);
				if (verifiedUser == null)
				{
					return new Response
					{
						StatusCode = HttpStatusCode.Unauthorized
					};
				}else
				{
					return new Response
					{
						StatusCode = HttpStatusCode.OK,
					}.WithCookie("session", verifiedUser.Session);
				}
			});
			Post("/register", _ =>
			{
				var user = this.Bind<UserCredentials>();
				var registeredUser = KaiseiCore.RegisterUser(user);
				if (registeredUser == null)
				{
					return new Response
					{
						StatusCode = HttpStatusCode.Unauthorized
					};
				}
				else
				{
					return new Response
					{
						StatusCode = HttpStatusCode.OK,
					}.WithCookie("session", registeredUser.Session);
				}
			});

			//User
			Get("/user", _ =>
			{
				if (Context.CurrentUser == null)
					return new Response
					{
						StatusCode = HttpStatusCode.Unauthorized
					};
				return Response.AsJson(Context.CurrentUser);
			});

			Get("/user/avatar", _ =>
			{
				return "";
			});


			//App
			Get("/app", _ =>
			{
				var app = this.Bind<AppInfo>();
				return Response.AsJson(KaiseiCore.GetAppInfo(app.Id));
			});

			Post("/app/sso", _ =>
			{
				var sso = this.Bind();
				var userId = ((UserModel)Context.CurrentUser).Id;
				return KaiseiCore.AuthorizeApp(sso.appId, userId);
			});


			Post(@"/", _ =>
			{
				var post = this.Bind<SSOData>();
				if(Context.CurrentUser != null)
				{
					post.Verified = true;
				}
				return View["Index", post];
			});
		}
    }
}
