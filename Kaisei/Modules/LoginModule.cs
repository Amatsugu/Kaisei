using System;
using System.Collections.Generic;
using System.Text;
using Kaisei.DataModels;
using Nancy;
using Nancy.ModelBinding;

namespace Kaisei.Modules
{
	public class LoginModule : NancyModule
	{
		public LoginModule() : base("/login")
		{
			Post("/", _ =>
			{
				var user = this.Bind<UserCredentials>();
				var verifiedUser = KaiseiCore.VerifyUser(user);
				if (verifiedUser == null)
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
		}
	}
}
