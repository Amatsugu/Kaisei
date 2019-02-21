using System;
using System.Collections.Generic;
using System.Text;
using Nancy;
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
			
			//App
			Post("/sso", _ =>
			{
				if (Context.CurrentUser == null)
					return new Response
					{
						StatusCode = HttpStatusCode.Unauthorized
					};
				var sso = this.Bind<SSOData>();
				var userId = ((UserModel)Context.CurrentUser).Id;
				return KaiseiCore.AuthorizeApp(sso.AppId, userId);
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

			Post("/verifyPassword", _ =>
			{
				if (!(Context.CurrentUser is UserModel user))
					return new Response
					{
						StatusCode = HttpStatusCode.Unauthorized
					};
				var credentials = this.Bind<UserCredentials>();
				user = KaiseiCore.VerifyUser(credentials);
				if(user == null)
					return new Response
					{
						StatusCode = HttpStatusCode.Unauthorized
					};
				else
					return new Response
					{
						StatusCode = HttpStatusCode.OK
					};
			});

			Post("/updateUser", _ =>
			{
				if (!(Context.CurrentUser is UserModel user))
					return new Response
					{
						StatusCode = HttpStatusCode.Unauthorized
					};
				var creds = this.Bind<UserCredentials>();
				KaiseiCore.UpdateUserInfo(user, creds);
				return new Response { StatusCode = HttpStatusCode.OK };
			});
		}
    }
}
