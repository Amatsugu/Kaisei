using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using MongoDB.Bson;

namespace Kaisei.DataModels
{
	public class UserModel : ClaimsPrincipal
	{
		public string Username { get; set; }
		public string Email { get; set; }
		public string Id { get; set; }
		public string Session { get; set; }
	}
}
