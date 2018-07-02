using System;
using System.Collections.Generic;
using System.Text;
using Nancy;

namespace Kaisei.Modules
{
    public class AuthModule : NancyModule
    {
		public AuthModule() : base("\auth")
		{
			//Get("\", );
		}
    }
}
