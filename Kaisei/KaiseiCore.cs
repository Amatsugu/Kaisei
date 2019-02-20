using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Kaisei.DataModels;
using MongoDB.Bson;
using MongoDB.Driver;
using Nancy.Authentication.Stateless;

namespace Kaisei
{
	public class KaiseiCore
	{
		internal static StatelessAuthenticationConfiguration StatelessConfig { get; private set; } = new StatelessAuthenticationConfiguration(nancyContext =>
		{
			if(nancyContext.Request.Cookies.ContainsKey("session"))
			{ 
				var sessionID = nancyContext.Request.Cookies.First(c => c.Key == "session").Value;
				return GetUserFromSessionID(sessionID);
			}else if(nancyContext.Request.Cookies.ContainsKey("apiKey"))
			{
				var apiKey = nancyContext.Request.Cookies.First(c => c.Key == "apiKey").Value;
				return VerifyApp(apiKey);
			}
			else
				return null;
		});

		public static MongoClient DBClient
		{
			get
			{
				if (KaiseiDB.mongoClient == null)
					return KaiseiDB.mongoClient = new MongoClient("mongodb://192.168.86.74:27017");
				else
					return KaiseiDB.mongoClient;
			}
		}

		public static IMongoDatabase DB
		{
			get
			{
				if (KaiseiDB.database == null)
					return KaiseiDB.database = DBClient.GetDatabase("Kaisei");
				else
					return KaiseiDB.database;
			}
		}

		public static IMongoCollection<BsonDocument> Users
		{
			get
			{
				if (KaiseiDB.users == null)
					return KaiseiDB.users = DB.GetCollection<BsonDocument>("Users");
				else
					return KaiseiDB.users;
			}
		}

		public static IMongoCollection<BsonDocument> Apps
		{
			get
			{
				if (KaiseiDB.apps == null)
					return KaiseiDB.apps = DB.GetCollection<BsonDocument>("Apps");
				else
					return KaiseiDB.apps;
			}
		}

		private static class KaiseiDB
		{
			internal static MongoClient mongoClient;
			internal static IMongoDatabase database;
			internal static IMongoCollection<BsonDocument> users;
			internal static IMongoCollection<BsonDocument> apps;
			internal static IMongoCollection<BsonDocument> appUsers;

		}

		

		public static Dictionary<string, UserModel> ActiveSessions = new Dictionary<string, UserModel>();

		internal static string HashPassword(string password)
		{
			byte[] salt;
			new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);

			var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
			byte[] hash = pbkdf2.GetBytes(20);

			byte[] hashBytes = new byte[36];
			Array.Copy(salt, 0, hashBytes, 0, 16);
			Array.Copy(hash, 0, hashBytes, 16, 20);

			return Convert.ToBase64String(hashBytes);
		}

		internal static bool VerifyPassword(string password, string passwordHash)
		{
			if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
				return false;
			/* Extract the bytes */
			byte[] hashBytes = Convert.FromBase64String(passwordHash);
			/* Get the salt */
			byte[] salt = new byte[16];
			Array.Copy(hashBytes, 0, salt, 0, 16);
			/* Compute the hash on the password the user entered */
			var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
			byte[] hash = pbkdf2.GetBytes(20);
			/* Compare the results */
			for (int i = 0; i < 20; i++)
				if (hashBytes[i + 16] != hash[i])
					return false;
			return true;
		}

		#region User
		/// <summary>
		/// Register a new user
		/// </summary>
		/// <param name="credentials">User Credentials</param>
		/// <returns>User Info</returns>
		public static UserModel RegisterUser(UserCredentials credentials)
		{
			var id = GetNewID();
			Users.InsertOne(new BsonDocument
			{
				{ "id", id },
				{ "email", Sanitize(credentials.Email) },
				{ "password", HashPassword(credentials.Password) },
				{ "username", Sanitize(credentials.Username) },
				{ "claims", BsonValue.Create(new string[0])},
				{ "apps", BsonValue.Create(new BsonDocument[0]) },
				{ "authorizations", BsonValue.Create(new BsonDocument[0]) }
			});
			var user = new UserModel
			{
				Id = id,
				Username = credentials.Username,
				Session = GetNewID(),
				Email = credentials.Email
			};
			ActiveSessions.Add(user.Session, user);
			return user;

		}

		public static void UploadIcon(string userId, Stream file, string mime)
		{
			Users.UpdateOne($"{{ id : '{userId}' }}", new BsonDocument
			{
				{ "$set", new BsonDocument
					{
						{ "icon", new BsonBinaryData(StreamToByteA(file)) },
						{ "icon-mime", mime }
					}
				}
			});
		}

		private static byte[] StreamToByteA(Stream stream)
		{
			byte[] bArray = new byte[stream.Length];
			stream.Read(bArray, 0, (int)stream.Length);
			return bArray;
		}

		public static (Stream icon, string mime) GetIcon(string userId)
		{
			var user = Users.Find($"{{ id : '{userId}' }}").FirstOrDefault();
			if (user == null)
				return default;
			if (user.TryGetValue("icon", out var icon))
				return (new MemoryStream(icon.AsByteArray), user.GetValue("icon-mime").AsString);
			else
				return default;
		}

		/// <summary>
		/// Verify a user login
		/// </summary>
		/// <param name="credentials">User Credentials</param>
		/// <returns>User</returns>
		public static UserModel VerifyUser(UserCredentials credentials)
		{
			credentials.Email = Sanitize(credentials.Email);
			var userinfo = Users.Find($"{{ email : '{ credentials.Email}' }}").First();
			if (userinfo.IsBsonNull)
				return null;
			if (VerifyPassword(credentials.Password, userinfo.GetValue("password").AsString))
			{
				var user = new UserModel
				{
					Id = userinfo.GetValue("id").AsString,
					Username = Uri.UnescapeDataString(userinfo.GetValue("username").AsString),
					Session = GetNewID(),
					Email = Uri.UnescapeDataString(userinfo.GetValue("email").AsString)
				};
				ActiveSessions.Add(user.Session, user);
				return user;
			}
			else
				return null;
		}


		/// <summary>
		/// Get the user from the session ID
		/// </summary>
		/// <param name="sessionId">The Session</param>
		/// <returns>The User</returns>
		public static UserModel GetUserFromSessionID(string sessionId)
		{
			if (ActiveSessions.ContainsKey(sessionId))
				return ActiveSessions[sessionId];
			else
				return null;
		}

		#endregion

		/// <summary>
		/// Get App Info
		/// </summary>
		/// <param name="appId">Target App</param>
		/// <returns>App Info</returns>
		public static AppInfo GetAppInfo(string appId)
		{
			var app = Apps.Find($"{{ id : '{appId}'}}").FirstOrDefault();
			if (app == null)
				return null;
			return new AppInfo()
			{
				Id = appId,
				Name = UnSanitize(app.GetValue("name").AsString),
				Description = UnSanitize(app.GetValue("description").AsString),
				Hostname = UnSanitize(app.GetValue("hostname").AsString)
			};
		}

		public static AppInfo VerifyApp(string appApiKey)
		{
			appApiKey = Sanitize(appApiKey);
			var app = Apps.Find($"{{ apiKey : '{appApiKey}'}}").FirstOrDefault();
			if (app == null)
				return null;
			return new AppInfo
			{
				Id = app.GetValue("id").AsString,
				Hostname = UnSanitize(app.GetValue("hostname").AsString),
				Name = UnSanitize(app.GetValue("name").AsString),
				Description = UnSanitize(app.GetValue("description").AsString),
				ApiKey = appApiKey
			};
		}

		/// <summary>
		/// Get a user from an app
		/// </summary>
		/// <param name="apiKey">Target App</param>
		/// <param name="userId">App Specfific userid</param>
		/// <returns></returns>
		public static UserModel GetAppUser(string apiKey, string userId)
		{
			var app = VerifyApp(apiKey);
			if (app == null)
				return null;
			var userInfo = Users.Find($"{{ 'apps.appId' : '{app.Id}' }}").FirstOrDefault();
			if (userInfo == null)
				return null;
			var user = new UserModel
			{
				Username = userInfo.GetValue("username").AsString,
				Email = userInfo.GetValue("email").AsString,
				Id = userId,
			};
			return user;
		}

		public static AppInfo[] GetUserAuthedApps(string userId)
		{
			var apps = Users.Find($"{{ id : '{userId}' }}").FirstOrDefault()?.GetValue("apps").AsBsonArray;
			var appInfo = new AppInfo[apps.Count];
			var i = 0;
			foreach(var app in apps)
			{
				appInfo[i++] = GetAppInfo(app.AsBsonDocument.GetValue("appId").AsString);
			}
			return appInfo;
		}

		public static AppInfo[] GetUsersApps(string userId)
		{
			var apps = Apps.Find($"{{ owner : '{userId}' }}").ToList();
			var appInfo = new AppInfo[apps.Count];
			var i = 0;
			foreach (var app in apps)
			{
				appInfo[i++] = new AppInfo
				{
					Id = app.GetValue("id").AsString,
					Name = app.GetValue("name").AsString,
					Description = app.GetValue("description").AsString,
					Hostname = app.GetValue("hostname").AsString

				};
			}
			return appInfo;
		}

		/// <summary>
		/// Checks whether an app auth is valid
		/// </summary>
		/// <param name="appId">App Id</param>
		/// <param name="origin">Origin uri</param>
		/// <returns></returns>
		public static bool ValidateAppAuth(string appId, Uri origin)
		{
			return !Apps.Find($"{{ id : {Sanitize(appId)}, hostname : {origin.Host} }}").First().IsBsonNull;
		}

		/// <summary>
		/// Create a new app
		/// </summary>
		/// <param name="app">App Info</param>
		/// <param name="ownerId">Owner of the app</param>
		/// <returns>The filled out app info</returns>
		public static AppInfo RegisterApp(AppInfo app, string ownerId)
		{
			var id = GetNewID();
			var apiKey = GetNewID();
			Apps.InsertOne(new BsonDocument
			{
				{ "id", id },
				{ "name", Sanitize(app.Name) },
				{ "description", Sanitize(app.Description) },
				{ "owner", Sanitize(ownerId) },
				{ "apiKey", apiKey },
				{ "hostname", Sanitize(app.Hostname) },
			});
			app.Id = id;
			app.ApiKey = apiKey;
			return app;
		}

		/// <summary>
		/// Generate a authorization id for this user tied to the specified app
		/// </summary>
		/// <param name="appId">Target App</param>
		/// <param name="userId">Target User</param>
		/// <returns>Authorization Id</returns>
		public static string AuthorizeApp(string appId, string userId)
		{
			appId = Sanitize(appId);
			var authId = GetNewID();
			Users.UpdateOne($"{{ id : '{userId}'}}", $"{{ $push : {{ authorizations : {{ authId : '{authId}', appId : '{appId}'}} }} }}");
			return authId;
		}

		/// <summary>
		/// Remove the autorization for a given app
		/// </summary>
		/// <param name="userId">The user to revoke the app from</param>
		/// <param name="appId">The app to revoke</param>
		/// <returns></returns>
		public static bool RevokeApp(string userId, string appId)
		{
			var res = Users.UpdateMany($"{{ id : '{userId}' }}", $"{{ $pull : {{ apps: {{ appId : '{appId}' }} }} }}");
			return res.IsModifiedCountAvailable && res.ModifiedCount > 0;
		}

		public static void ClearAuthorizations(string userId)
		{
			Users.UpdateOne($"{{ id : '{userId}' }}", $"{{ $set : {{ authorizations : [] }} }}");
		}

		public static bool IsAppAuthed(string appId, string userId)
		{
			return Users.CountDocuments($"{{ id : '{userId}', 'apps.appId' : '{appId}' }}") > 0;
		}

		/// <summary>
		/// Use the auth Id given by kaisei to authorize the app with the user who generated the auth id
		/// </summary>
		/// <param name="apiKey">The Api Key for the app</param>
		/// <param name="authId">The authorization id</param>
		/// <returns>The app sepcific user id</returns>
		public static string ConfirmAuthorization(string apiKey, string appId, string authId)
		{
			apiKey = Sanitize(apiKey);
			authId = Sanitize(authId);
			//Find User to authorize
			var user = Users.Find($"{{ 'authorizations.authId' : '{authId}' }}").FirstOrDefault();
			if (user == null)
				return null;
			var userId = user.GetValue("id").AsString;
			//Get Auth details
			var authorization = user.GetValue("authorizations").AsBsonArray.First(a => a.AsBsonDocument.GetValue("authId") == authId);
			var authAppId = authorization.AsBsonDocument.GetValue("appId").AsString;
			var app = Apps.Find($"{{ id : '{appId}', apiKey : '{apiKey}' }}").FirstOrDefault();
			//Clear Auths
			ClearAuthorizations(userId);
			if (appId != authAppId)
				return null;
			if (IsAppAuthed(appId, userId))
				return null;
			//Create app userid
			var appUserId = GetNewID();
			Users.UpdateOne($"{{ id : '{userId}' }}", $"{{ $push : {{ apps : {{ appId : '{appId}', appUserId : '{appUserId}' }} }} }}");

			return appUserId;
		}


		/// <summary>
		/// Delete an app and de-authorizes all its users
		/// </summary>
		/// <param name="appId">App to delete</param>
		public static void DeleteApp(string appId)
		{
			appId = Sanitize(appId);
			Apps.DeleteOneAsync($"{{ id : '{appId}' }}");
		}

		/// <summary>
		/// Updates the information about an app
		/// </summary>
		/// <param name="appInfo">The information</param>
		public static void UpdateApp(AppInfo appInfo)
		{
			appInfo.Id = Sanitize(appInfo.Id);
			var update = new BsonDocument();
			if(appInfo.Name != null)
				update.Add("name", Sanitize(appInfo.Name));
			if (appInfo.Hostname != null)
				update.Add("description", Sanitize(appInfo.Description));
			if (appInfo.Hostname != null)
				update.Add("hostname", Sanitize(appInfo.Hostname));
			Apps.UpdateOne($"{{ id : '{appInfo.Id}' }}", update); ;
		}

		public static string Sanitize(string raw)
		{
			raw = raw.Replace("<", "&lt");
			raw = raw.Replace(">", "&gt");
			raw = raw.Replace("\'", "&quot");
			raw = raw.Replace("\"", "&apos");
			//raw = Uri.EscapeDataString(raw);
			return raw;
		}

		public static string UnSanitize(string sanitized)
		{
			sanitized = sanitized.Replace("&lt", "<");
			sanitized = sanitized.Replace("&gt", ">");
			sanitized = sanitized.Replace("&quot", "\'");
			sanitized = sanitized.Replace("&apos", "\"");
			sanitized = Uri.UnescapeDataString(sanitized);
			return sanitized;
		}

		internal static string GetNewID() => Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace("=", "").Replace("/", "_").Replace(@"\", ".");

	}
}
