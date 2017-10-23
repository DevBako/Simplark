using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Simplark
{
	class OAuth
	{
		public readonly string ConsumerKey;
		public readonly string ConsumerSecret;
		public readonly string AccessToken;
		public readonly string AccessSecret;

		public OAuth(string consumerKey, string consumerSecret)
		{
			ConsumerKey = consumerKey;
			ConsumerSecret = consumerSecret;
		}

		public OAuth(string consumerKey, string consumerSecret, string accessToken, string accessSecret)
		{
			ConsumerKey = consumerKey;
			ConsumerSecret = consumerSecret;
			AccessToken = accessToken;
			AccessSecret = accessSecret;
		}

		public string GenerateOAuthHeader(string url, string method, List<KeyValuePair<String,String>> param, bool _callback=false)
		{
			List<KeyValuePair<String, String>> _param = new List<KeyValuePair<String, String>>();
			
			string version = "1.0";
			string signatureMethod = "HMAC-SHA1";
			string callback = "oob";
			string consumerKey = ConsumerKey;
			string token = AccessToken;
			string nonce = Helper.GenerateNonce();
			string timestamp = Helper.GenerateTimestamp().ToString();

			_param.Add(new KeyValuePair<String, String>("oauth_version", version));
			_param.Add(new KeyValuePair<String, String>("oauth_signature_method", signatureMethod));
			_param.Add(new KeyValuePair<String, String>("oauth_consumer_key", consumerKey));
			_param.Add(new KeyValuePair<String, String>("oauth_token", token));
			_param.Add(new KeyValuePair<String, String>("oauth_nonce", nonce));
			_param.Add(new KeyValuePair<String, String>("oauth_timestamp", timestamp));
			if (_callback)
			{
				_param.Add(new KeyValuePair<String, String>("oauth_callback", callback));
			}
			_param.AddRange(param);

			//var __param = String.Join("&", _param.Select(x => new KeyValuePair<String, String>(Helper.EncodeRFC3986(x.Key), Helper.EncodeRFC3986(x.Value))));

			string sigStr = String.Join("&", _param.OrderBy(x => x.Key).Select(x => String.Format("{0}={1}", Helper.EncodeRFC3986(x.Key), Helper.EncodeRFC3986(x.Value))));
			
			string sigBaseStr = string.Format("{0}&{1}&{2}", method, Helper.EncodeRFC3986(url), Helper.EncodeRFC3986(sigStr));

			string signingStr = string.Format("{0}&{1}", Helper.EncodeRFC3986(ConsumerSecret), Helper.EncodeRFC3986(AccessSecret));
			
			HMACSHA1 sigHasher = new HMACSHA1(Encoding.ASCII.GetBytes(signingStr));

			string signature = Convert.ToBase64String(sigHasher.ComputeHash(Encoding.ASCII.GetBytes(sigBaseStr)));

			string headerFormat;
			if (_callback)
			{
				headerFormat = "OAuth " +
								"oauth_callback=\"{0}\", " +
								"oauth_consumer_key=\"{1}\", " +
								"oauth_nonce=\"{2}\", " +
								"oauth_signature=\"{3}\"," +
								"oauth_signature_method=\"{4}\", " +
								"oauth_timestamp=\"{5}\", " +
								"oauth_token=\"{6}\", " +
								"oauth_version=\"{7}\"";
			}
			else
			{
				headerFormat = "OAuth " +
								"oauth_consumer_key=\"{1}\", " +
								"oauth_nonce=\"{2}\", " +
								"oauth_signature=\"{3}\"," +
								"oauth_signature_method=\"{4}\", " +
								"oauth_timestamp=\"{5}\", " +
								"oauth_token=\"{6}\", " +
								"oauth_version=\"{7}\"";
			}

			return string.Format(headerFormat,
					Helper.EncodeRFC3986(callback),
					Helper.EncodeRFC3986(consumerKey),
					Helper.EncodeRFC3986(nonce),
					Helper.EncodeRFC3986(signature),
					Helper.EncodeRFC3986(signatureMethod),
					Helper.EncodeRFC3986(timestamp),
					Helper.EncodeRFC3986(token),
					Helper.EncodeRFC3986(version));
		}
	}
}
