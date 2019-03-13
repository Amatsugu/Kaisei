using System;
using System.Collections.Generic;
using System.Text;
using Nancy.ViewEngines.Razor;

namespace Kaisei.Bootstrap
{
	public class RazorConfig : IRazorConfiguration
	{
		public IEnumerable<string> GetAssemblyNames()
		{
			return null;
		}

		public IEnumerable<string> GetDefaultNamespaces()
		{
			yield return "Kaisei";
			yield return "Kaisei.DataModels";
		}

		public bool AutoIncludeModelNamespace
		{
			get { return true; }
		}
	}
}
