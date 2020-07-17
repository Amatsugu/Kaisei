using Nancy;
using Nancy.Bootstrapper;
using Nancy.Conventions;
using Nancy.TinyIoc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Reflection;
using Nancy.Configuration;

namespace Anzu.Bootstrap
{
	public class NancyBootstrap : DefaultNancyBootstrapper
	{
		private string PROJECT => "Kaisei";

		public override void Configure(INancyEnvironment environment)
		{
			base.Configure(environment);

#if DEBUG
			environment.Views(runtimeViewUpdates: true);
#endif
			environment.Tracing(false, true);
		}

		protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
		{
			Conventions.ViewLocationConventions.Add((viewName, model, context) =>
			{
				return $@"{PROJECT}Web/{viewName}";
			});
		}

#if DEBUG
		protected override IRootPathProvider RootPathProvider
		{
			get { return new RootProvider(); }
		}
#endif

		protected override void ConfigureConventions(NancyConventions nancyConventions)
		{
			nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("res", $@"{PROJECT}Web/res"));
		}
	}

#if DEBUG
	public class RootProvider : IRootPathProvider
	{
		public string GetRootPath()
		{
			return $"{Directory.GetCurrentDirectory()}/../../../";
		}
	}
#endif

}