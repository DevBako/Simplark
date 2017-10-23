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

	class APIResponse
	{
		public readonly string rawStr;
		public readonly bool isSuccess;

		private Dictionary<String, String> _params;

		public APIResponse(string rawStr, bool isSuccess = true)
		{
			this.rawStr = rawStr;
			this.isSuccess = isSuccess;
			_params = DecodeResponseParam(rawStr);
		}

		public string this[string key]
		{
			get
			{
				if (_params.ContainsKey(key))
				{
					return _params[key];
				}
				return null;
			}
			set
			{
				if (!_params.ContainsKey(key))
				{
					_params.Add(key, "");
				}
				_params[key] = value;
			}
		}

		private Dictionary<String, String> DecodeResponseParam(string response, char delimeter = '&')
		{
			Dictionary<String, String> param = new Dictionary<string, string>();
			foreach (var entry in response.Split(delimeter))
			{
				var kvp = entry.Split('=');
				try
				{
					param.Add(kvp[0], kvp[1]);
				}
				catch
				{
					Console.WriteLine("Something is wrong with the response I got : " + response);
					return new Dictionary<string, string>();
				}
			}
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

		private APIResponse PostRequest(string url, List<KeyValuePair<String, String>> headers, List<KeyValuePair<String, String>> param)
		{
			var http = new HttpClient();
			var content = new FormUrlEncodedContent(param);
			
			foreach (var entry in headers)
			{
				http.DefaultRequestHeaders.Add(entry.Key, entry.Value);
			}

			var response = http.PostAsync(url, content).Result;
			return new APIResponse(response.Content.ReadAsStringAsync().Result,
								   response.IsSuccessStatusCode);
		}

		public APIResponse GetRequest(string url, List<KeyValuePair<String, String>> headers, List<KeyValuePair<String, String>> param)
		{
			var http = new HttpClient();
			var content = new FormUrlEncodedContent(param);

			foreach (var entry in headers)
			{
				http.DefaultRequestHeaders.Add(entry.Key, entry.Value);
			}
			url = url + "?" + String.Join("&", param.Select(x => String.Format("{0}={1}", Helper.EncodeRFC3986(x.Key), Helper.EncodeRFC3986(x.Value))));

			var response = http.GetAsync(url).Result;
			return new APIResponse(response.Content.ReadAsStringAsync().Result,
								   response.IsSuccessStatusCode);
		}

		private APIResponse OAuthRequestToken()
		{
			OAuth oauth = new OAuth(ConsumerKey, ConsumerSecret);
			string url = URL.ApiBaseUrl + "oauth/request_token";

			List<KeyValuePair<String, String>> param = new List<KeyValuePair<String, String>>();

			List<KeyValuePair<String, String>> header = new List<KeyValuePair<String, String>>();
			header.Add(new KeyValuePair<String, String>("Authorization", oauth.GenerateOAuthHeader(url, "POST", param, true)));

			List<KeyValuePair<String, String>> body = new List<KeyValuePair<String, String>>();

			return PostRequest(url, header, body);
		}

		private string Authorize()
		{
			var url = URL.ApiBaseUrl + "oauth/authorize?oauth_token=" + AccessToken;

			Console.Write("PIN : ");
			System.Diagnostics.Process.Start(url);
			return Console.ReadLine();
		}

		private APIResponse OAuthAccessToken(string pin)
		{
			OAuth oauth = new OAuth(ConsumerKey, ConsumerSecret, AccessToken, AccessSecret);
			string url = URL.ApiBaseUrl + "oauth/access_token";

			List<KeyValuePair<String, String>> param = new List<KeyValuePair<String, String>>();

			List<KeyValuePair<String, String>> header = new List<KeyValuePair<String, String>>();
			header.Add(new KeyValuePair<String, String>("Authorization", oauth.GenerateOAuthHeader(url, "POST", param)));

			List<KeyValuePair<String, String>> body = new List<KeyValuePair<String, String>>();
			body.Add(new KeyValuePair<String, String>("oauth_verifier", pin));

			return PostRequest(url, header, body);
		}
		
		public bool Login()
		{
			APIResponse response;

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

		public void LoginAsKoinichi()
		{
			AccessToken = "wut";
			AccessSecret = "wut";
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

			return PostRequest(url, header, body);
		}

		public APIResponse StatusesHomeTineline(int count = 15)
		{
			return new APIResponse("");
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
