using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kaisei.DataModels;
using Nancy;
using Nancy.Authentication.Stateless;
using Nancy.ModelBinding;

namespace Kaisei.Modules
{
	public class UserModule : NancyModule

	{
		public UserModule() : base("/user")
		{
			StatelessAuthentication.Enable(this, KaiseiCore.StatelessConfig);
			Get("/", _ =>
			{
				if (!(Context.CurrentUser is UserModel user))
					return new Response
					{
						StatusCode = HttpStatusCode.Unauthorized
					};
				return Response.AsJson(user);
			});

			Get("/avatar", _ =>
			{
				if (!(Context.CurrentUser is UserModel user))
					return new Response
					{
						StatusCode = HttpStatusCode.Unauthorized
					};
				var (icon, mime) = KaiseiCore.GetIcon(user.Id);
				if(icon == default)
					return Response.AsRedirect("/res/img/DefaultIcon.png");
				return Response.FromStream(icon, mime);
			});

			Get("/authedapps", _ =>
			{
				if (!(Context.CurrentUser is UserModel user))
					return new Response
					{
						StatusCode = HttpStatusCode.Unauthorized
					};
				return KaiseiCore.GetUserAuthedApps(user.Id);
			});

			Get("/apps", _ =>
			{
				if (!(Context.CurrentUser is UserModel user))
					return new Response
					{
						StatusCode = HttpStatusCode.Unauthorized
					};
				return KaiseiCore.GetUsersApps(user.Id);
			});

			Post("/avatar", _ =>
			{
				if (!(Context.CurrentUser is UserModel user))
					return new Response
					{
						StatusCode = HttpStatusCode.Unauthorized
					};
				try
				{
					var file = Context.Request.Files.First();
					//Console.WriteLine(file.ContentType);
					KaiseiCore.UploadIcon(user.Id, file.Value, file.ContentType);
					return new Response { StatusCode = HttpStatusCode.OK };
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
					Console.WriteLine(e.StackTrace);
					return new Response() { StatusCode = HttpStatusCode.ImATeapot };
				}
			});

			Post("/revokeApp", _ =>
			{
				if (!(Context.CurrentUser is UserModel user))
					return new Response
					{
						StatusCode = HttpStatusCode.Unauthorized
					};
				//TODO: Implement Revokation
				var app = this.Bind<SSOData>();
				if (KaiseiCore.RevokeApp(user.Id, app.AppId))
					return new Response { StatusCode = HttpStatusCode.OK };
				else
					return new Response { StatusCode = HttpStatusCode.NotModified };
			});

			Get("/isAuth", p =>
			{
				var user = Context.CurrentUser as UserModel;
				var app = this.Bind<SSOData>();
				return KaiseiCore.IsAppAuthed(app.AppId, user.Id);
			});
		}
	}
}
