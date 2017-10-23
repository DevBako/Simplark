using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Simplark
{
	class URL
	{
		public const string ApiBaseUrl = "https://api.twitter.com/";

		public const string OAuthAccessToken = "oauth/access_token";
		public const string OAuthAccessUrl = ApiBaseUrl + OAuthAccessToken;

		public const string OAuthAuthorize = "oauth/authorize";
		public const string OAuthAuthorizeUrl = ApiBaseUrl + OAuthAuthorize;
	}


	class APIResponse : Response
	{
		public APIResponse(string rawStr, bool isSuccess = true) : base(rawStr, isSuccess)
		{
		}

		protected override Dictionary<String, String> DecodeResponse(string response)
		{
			Dictionary<String, String> param = new Dictionary<string, string>();
			return param;
		}
	}
	
	class API
	{
		public readonly string ConsumerKey;
		public readonly string ConsumerSecret;
		public string AccessToken;
		public string AccessSecret;

		public API(string consumerKey, string consumerSecret, string accessToken, string accessSecret)
		{
			ConsumerKey = consumerKey;
			ConsumerSecret = consumerSecret;
			AccessToken = accessToken;
			AccessSecret = accessSecret;
		}

		public API(string consumerKey, string consumerSecret)
		{
			ConsumerKey = consumerKey;
			ConsumerSecret = consumerSecret;
		}

		private HttpResponseMessage PostRequest(string url, List<KeyValuePair<String, String>> headers, List<KeyValuePair<String, String>> param)
		{
			var http = new HttpClient();
			var content = new FormUrlEncodedContent(param);
			
			foreach (var entry in headers)
			{
				http.DefaultRequestHeaders.Add(entry.Key, entry.Value);
			}

			return http.PostAsync(url, content).Result;
		}

		public HttpResponseMessage GetRequest(string url, List<KeyValuePair<String, String>> headers, List<KeyValuePair<String, String>> param)
		{
			var http = new HttpClient();
			var content = new FormUrlEncodedContent(param);

			foreach (var entry in headers)
			{
				http.DefaultRequestHeaders.Add(entry.Key, entry.Value);
			}
			url = url + "?" + String.Join("&", param.Select(x => String.Format("{0}={1}", Helper.EncodeRFC3986(x.Key), Helper.EncodeRFC3986(x.Value))));

			return http.GetAsync(url).Result;
		}

		private OAuthResponse OAuthRequestToken()
		{
			OAuth oauth = new OAuth(ConsumerKey, ConsumerSecret);
			string url = URL.ApiBaseUrl + "oauth/request_token";

			List<KeyValuePair<String, String>> param = new List<KeyValuePair<String, String>>();

			List<KeyValuePair<String, String>> header = new List<KeyValuePair<String, String>>();
			header.Add(new KeyValuePair<String, String>("Authorization", oauth.GenerateOAuthHeader(url, "POST", param, true)));

			List<KeyValuePair<String, String>> body = new List<KeyValuePair<String, String>>();

			var response = PostRequest(url, header, body);
			return new OAuthResponse(response.Content.ReadAsStringAsync().Result,
								     response.IsSuccessStatusCode);
		}

		private string Authorize()
		{
			var url = URL.ApiBaseUrl + "oauth/authorize?oauth_token=" + AccessToken;

			Console.Write("PIN : ");
			System.Diagnostics.Process.Start(url);
			return Console.ReadLine();
		}

		private OAuthResponse OAuthAccessToken(string pin)
		{
			OAuth oauth = new OAuth(ConsumerKey, ConsumerSecret, AccessToken, AccessSecret);
			string url = URL.ApiBaseUrl + "oauth/access_token";

			List<KeyValuePair<String, String>> param = new List<KeyValuePair<String, String>>();

			List<KeyValuePair<String, String>> header = new List<KeyValuePair<String, String>>();
			header.Add(new KeyValuePair<String, String>("Authorization", oauth.GenerateOAuthHeader(url, "POST", param)));

			List<KeyValuePair<String, String>> body = new List<KeyValuePair<String, String>>();
			body.Add(new KeyValuePair<String, String>("oauth_verifier", pin));

			var response= PostRequest(url, header, body);
			return new OAuthResponse(response.Content.ReadAsStringAsync().Result,
									 response.IsSuccessStatusCode);
		}
		
		public bool Login()
		{
			OAuthResponse response;

			response = OAuthRequestToken();
			if (!response.isSuccess)
			{
				Console.WriteLine(String.Format("Failed to get request token: {0}", response.rawStr));
				AccessToken = "";
				AccessSecret = "";
				return false;
			}
			AccessToken = response["oauth_token"];
			AccessSecret = response["oauth_token_secret"];

			string pin = Authorize();

			response = OAuthAccessToken(pin);
			if (!response.isSuccess)
			{
				Console.WriteLine(String.Format("Failed to get access token: {0}", response.rawStr));
				AccessToken = "";
				AccessSecret = "";
				return false;
			}
			AccessToken = response["oauth_token"];
			AccessSecret = response["oauth_token_secret"];
			return true;
		}

		public void LoginAs(string accessToken, string accessSecret)
		{
			AccessToken = accessToken;
			AccessSecret = accessSecret;
		}

		public APIResponse StatusesUpdate(string status)
		{
			OAuth oauth = new OAuth(ConsumerKey, ConsumerSecret, AccessToken, AccessSecret);
			var url = URL.ApiBaseUrl + "1.1/statuses/update.json";

			List<KeyValuePair<string, string>> param = new List<KeyValuePair<string, string>>();
			param.Add(new KeyValuePair<string, string>("status", status));

			List<KeyValuePair<String, String>> header = new List<KeyValuePair<String, String>>();
			header.Add(new KeyValuePair<String, String>("Authorization", oauth.GenerateOAuthHeader(url, "POST", param)));

			List<KeyValuePair<String, String>> body = new List<KeyValuePair<String, String>>();
			body.Add(new KeyValuePair<string, string>("status", status));

			var response = PostRequest(url, header, body);
			return new APIResponse(response.Content.ReadAsStringAsync().Result,
								   response.IsSuccessStatusCode);
		}

		public APIResponse StatusesHomeTineline(int count = 15)
		{
			OAuth oauth = new OAuth(ConsumerKey, ConsumerSecret, AccessToken, AccessSecret);
			var url = URL.ApiBaseUrl + "1.1/statuses/home_timeline.json";

			List<KeyValuePair<string, string>> param = new List<KeyValuePair<string, string>>();
			param.Add(new KeyValuePair<string, string>("count", count.ToString()));

			List<KeyValuePair<String, String>> header = new List<KeyValuePair<String, String>>();
			header.Add(new KeyValuePair<String, String>("Authorization", oauth.GenerateOAuthHeader(url, "GET", param)));

			List<KeyValuePair<String, String>> body = new List<KeyValuePair<String, String>>();
			body.Add(new KeyValuePair<string, string>("count", count.ToString()));

			var response = GetRequest(url, header, body);
			return new APIResponse(response.Content.ReadAsStringAsync().Result,
								   response.IsSuccessStatusCode);
		}

		public void printKeys()
		{
			Console.WriteLine("ConsumerKey = {0}", ConsumerKey);
			Console.WriteLine("ConsumerSecret = {0}", ConsumerSecret);
			Console.WriteLine("AccessToken = {0}", AccessToken);
			Console.WriteLine("AccessSecret = {0}", AccessSecret);
		}
	}
}
