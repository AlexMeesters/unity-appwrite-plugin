using System;
using System.Collections.Generic;
using System.Net;
using Cysharp.Threading.Tasks;
using Lowscope.AppwritePlugin.Accounts.Enums;
using UnityEngine.Networking;

namespace Lowscope.AppwritePlugin.Utils
{
	public class WebRequest : IDisposable
	{
		private readonly UnityWebRequest webRequest;
		private readonly EWebRequestType requestType;

		public WebRequest(EWebRequestType requestType, string url, Dictionary<string,string> headers, string cookie,
			byte[] data = null)
		{
			UnityWebRequest.ClearCookieCache(new Uri(url[..url.IndexOf("v1", StringComparison.InvariantCulture)]));

			this.requestType = requestType;

			switch (requestType)
			{
				case EWebRequestType.GET:
					webRequest = UnityWebRequest.Get(url);
					break;
				case EWebRequestType.POST:
					// Workaround to send byte[] data over a post request. Instead of text.
					webRequest = UnityWebRequest.Put(url, data);
					webRequest.method = "POST";
					break;
				case EWebRequestType.PUT:
					webRequest = UnityWebRequest.Put(url, data);
					break;
				case EWebRequestType.DELETE:
					webRequest = UnityWebRequest.Delete(url);
					break;
				default:
					webRequest = null;
					break;
			}

			foreach (var (key, value) in headers)
				webRequest?.SetRequestHeader(key, value);

			if (!string.IsNullOrEmpty(cookie))
				webRequest?.SetRequestHeader("Cookie", cookie);
		}

		public async UniTask<(string, HttpStatusCode)> Send()
		{
			try
			{
				await webRequest.SendWebRequest();
				var responseCode = (int)webRequest.responseCode;

				if (requestType == EWebRequestType.DELETE) 
					return ("", HttpStatusCode.OK);
						
				string text = webRequest?.downloadHandler.text;
				return (text, (HttpStatusCode)responseCode);

			}
			catch (UnityWebRequestException exception)
			{
				string text = webRequest?.downloadHandler?.text;
				return (text, (HttpStatusCode)exception.ResponseCode);
			}
		}

		public void Dispose()
		{
			webRequest?.Dispose();
		}

		public string ExtractCookie()
		{
			return webRequest.GetResponseHeaders().TryGetValue("Set-Cookie", out string cookie)
				? cookie[..cookie.IndexOf(" expires=", StringComparison.InvariantCulture)]
				: "";
		}
	}
}