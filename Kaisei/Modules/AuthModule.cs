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
using Nancy.Security;

namespace Kaisei.Modules
{
    public class AuthModule : NancyModule
    {
		public AuthModule() : base("/auth")
		{
			StatelessAuthentication.Enable(this, KaiseiCore.StatelessConfig);

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
			Post("/app/sso", _ =>
			{
				if (Context.CurrentUser == null)
					return new Response
					{
						StatusCode = HttpStatusCode.Unauthorized
					};
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
