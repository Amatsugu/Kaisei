using System;
using System.Collections.Generic;
using System.Text;

namespace Kaisei.DataModels
{
	public class SSOData
	{
		public string Callback { get; set; }
		public bool Verified { get; set; }
		public string AppId { get; set; }
	}
}
