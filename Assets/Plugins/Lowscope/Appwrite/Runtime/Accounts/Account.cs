using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Cysharp.Threading.Tasks;
using Lowscope.Appwrite.Accounts.Enums;
using Lowscope.Appwrite.Accounts.Model;
using Lowscope.Appwrite.Utils;
using Newtonsoft.Json.Linq;
using UnityEngine;
using WebRequest = Lowscope.Appwrite.Utils.WebRequest;

namespace Lowscope.Appwrite.Accounts
{
	public class Account
	{
		private readonly AppwriteConfig config;
		private readonly Dictionary<string, string> headers;

		private User user;

		private DateTime lastRegisterRequestDate;

		private string UserPath => Path.Combine(Application.persistentDataPath, "user.json");

		public Account(AppwriteConfig config)
		{
			this.config = config;

			headers = new Dictionary<string, string>(new Dictionary<string, string>
			{
				{ "X-Appwrite-Project", config.AppwriteProjectID },
				{ "Content-Type", "application/json" }
			});

			// Fetches user info written to disk.
			user = FileUtilities.Read<User>(UserPath, config);
		}

		private void StoreUserToDisk()
			=> FileUtilities.Write(user, UserPath, config);

		private void ClearUserDataFromDisk()
		{
			if (File.Exists(UserPath))
				File.Delete(UserPath);

			user = null;
		}

		private async UniTask<bool> RefreshUserInfo()
		{
			if (user == null)
				return false;

			string url = $"{config.AppwriteURL}/account";
			using var request = new WebRequest(EWebRequestType.GET, url, headers, user?.Cookie);
			var (json, httpStatusCode) = await request.Send();

			if (httpStatusCode == HttpStatusCode.OK)
			{
				JObject parsedData = JObject.Parse(json);
				user.Name = (string)parsedData.GetValue("name");
				user.EmailVerified = (bool)parsedData.GetValue("emailVerification");
				StoreUserToDisk();
			}
			else
			{
				return false;
			}

			return true;
		}

		public async UniTask<(User, ELoginResponse)> Login(string email, string password)
		{
			if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
				return (null, ELoginResponse.MissingCredentials);

			if (user != null)
			{
				if (user.Email == email)
					return (user, ELoginResponse.AlreadyLoggedIn);

				await Logout();
			}

			JObject obj = new JObject(
				new JProperty("email", email),
				new JProperty("password", password));

			byte[] bytes = Encoding.UTF8.GetBytes(obj.ToString());

			string url = $"{config.AppwriteURL}/account/sessions";

			using var request = new WebRequest(EWebRequestType.POST, url, headers, user?.Cookie, bytes);

			var (json, httpStatusCode) = await request.Send();

			if (httpStatusCode != HttpStatusCode.Created)
			{
				switch (httpStatusCode)
				{
					case 0:
						return (null, ELoginResponse.NoConnection);
					case HttpStatusCode.Unauthorized:
					case HttpStatusCode.NotFound:
						return (null, json.Contains("blocked")
							? ELoginResponse.Blocked
							: ELoginResponse.WrongCredentials);
					case HttpStatusCode.ServiceUnavailable
						or HttpStatusCode.GatewayTimeout
						or HttpStatusCode.InternalServerError
						or HttpStatusCode.TooManyRequests:
						return (null, ELoginResponse.ServerBusy);
				}
			}

			JObject parsedData = JObject.Parse(json);

			user = new User
			{
				Id = (string)parsedData.GetValue("userId"),
				Email = (string)parsedData.GetValue("providerUid"),
				Cookie = request.ExtractCookie()
			};

			// Attempts to get account info to fill in additional user data such as 
			// If email is verified and Name.
			if (!await RefreshUserInfo())
				return (null, ELoginResponse.Failed);

			StoreUserToDisk();
			return (user, ELoginResponse.Success);
		}

		public async UniTask<(User, ERegisterResponse)> Register(string id, string name, string email, string password)
		{
			if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email))
				return (null, ERegisterResponse.MissingCredentials);

			if (user != null)
				return (user, ERegisterResponse.AlreadyLoggedIn);

			if ((DateTime.Now - lastRegisterRequestDate).Duration().TotalMinutes < config.RegisterTimeoutMinutes)
				return (null, ERegisterResponse.Timeout);

			JObject obj = new JObject(
				new JProperty("userId", id),
				new JProperty("email", email),
				new JProperty("password", password),
				new JProperty("name", name));

			byte[] bytes = Encoding.UTF8.GetBytes(obj.ToString());

			string url = $"{config.AppwriteURL}/account";

			using var request = new WebRequest(EWebRequestType.POST, url, headers, user?.Cookie, bytes);

			var (_, httpStatusCode) = await request.Send();

			if (httpStatusCode != HttpStatusCode.Created)
			{
				switch (httpStatusCode)
				{
					case 0:
						return (null, ERegisterResponse.NoConnection);
					case HttpStatusCode.Unauthorized:
						return (null, ERegisterResponse.Failed);
					case HttpStatusCode.ServiceUnavailable
						or HttpStatusCode.GatewayTimeout
						or HttpStatusCode.InternalServerError
						or HttpStatusCode.TooManyRequests:
						return (null, ERegisterResponse.ServerBusy);
				}
			}

			var (u, _) = await Login(email, password);
			return (u, ERegisterResponse.Success);
		}

		public async UniTask<bool> Logout()
		{
			if (user == null)
				return false;

			// Remove current session
			string url = $"{config.AppwriteURL}/account/sessions/current";
			using var request = new WebRequest(EWebRequestType.DELETE, url, headers, user.Cookie);
			await request.Send();

			ClearUserDataFromDisk();

			return true;
		}

		/// <summary>
		/// Obtain a JWT, that can be used to validate your session with external services.
		/// </summary>
		public async UniTask<string> ObtainJwt(bool fromCache = true)
		{
			if (user == null)
				return "";

			if (fromCache && !string.IsNullOrEmpty(user.Jwt))
				if ((DateTime.Now - user.JwtProvideDate).Duration().TotalMinutes < config.JwtExpireMinutes)
					return user.Jwt;

			string url = $"{config.AppwriteURL}/account/jwt";
			using var request = new WebRequest(EWebRequestType.POST, url, headers, user.Cookie);
			var (json, httpStatusCode) = await request.Send();

			if (httpStatusCode != HttpStatusCode.Created)
				return "";

			JObject parsedData = JObject.Parse(json);

			string jwt = (string)parsedData.GetValue("jwt");
			user.Jwt = jwt;

			// Remove minute to account for latency.
			user.JwtProvideDate = DateTime.Now - TimeSpan.FromMinutes(1);

			return jwt;
		}

		public async UniTask<EEmailVerifyResponse> RequestVerificationMail()
		{
			if (user == null)
				return EEmailVerifyResponse.NotLoggedIn;

			if (user.EmailVerified)
				return EEmailVerifyResponse.AlreadyVerified;

			if (user.LastEmailRequestDate != default &&
			    (DateTime.Now - user.LastEmailRequestDate).Duration().TotalMinutes < config.VerifyEmailTimeoutMinutes)
				return EEmailVerifyResponse.Timeout;

			if (string.IsNullOrEmpty(config.VerifyEmailURL))
				return EEmailVerifyResponse.NoURLSpecified;

			JObject obj = new JObject(new JProperty("url", config.VerifyEmailURL));
			byte[] bytes = Encoding.UTF8.GetBytes(obj.ToString());

			string url = $"{config.AppwriteURL}/account/verification";
			using var request = new WebRequest(EWebRequestType.POST, url, headers, user.Cookie, bytes);
			var (json, httpStatusCode) = await request.Send();

			user.LastEmailRequestDate = DateTime.Now;
			StoreUserToDisk();

			return httpStatusCode == HttpStatusCode.Created ? EEmailVerifyResponse.Sent : EEmailVerifyResponse.Failed;
		}

		public async UniTask<User> GetUser()
		{
			if (user == null)
				return null;

			if (await RefreshUserInfo())
				return user;

			ClearUserDataFromDisk();

			return null;
		}
	}
}