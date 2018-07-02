using System;
using System.Collections.Generic;
using System.Text;
using Nancy;

namespace Kaisei.Modules
{
    public class IndexModule : NancyModule
    {
		public IndexModule()
		{
			Get("/", _ =>
			{
				return View["index"];
			});
		}
    }
}
