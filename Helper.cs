using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simplark
{
	abstract class Response
	{
		public readonly string rawStr;
		public readonly bool isSuccess;

		protected Dictionary<String, String> _params;

		public Response(string rawStr, bool isSuccess = true)
		{
			this.rawStr = rawStr;
			this.isSuccess = isSuccess;
			_params = DecodeResponse(rawStr);
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

		protected virtual Dictionary<String, String> DecodeResponse(string rawStr)
		{
			throw new NotImplementedException();
		}
	}
	class Helper
	{
		public static int GenerateTimestamp()
		{
			return (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
		}

		public static string GenerateNonce()
		{
			return Guid.NewGuid().ToString("N");
		}

		public static string EncodeRFC3986(string value)
		{
			if (string.IsNullOrEmpty(value))
				return string.Empty;

			var encoded = Uri.EscapeDataString(value);

			return System.Text.RegularExpressions.Regex
				.Replace(encoded, "(%[0-9a-f][0-9a-f])", c => c.Value.ToUpper())
				.Replace("(", "%28")
				.Replace(")", "%29")
				.Replace("$", "%24")
				.Replace("!", "%21")
				.Replace("*", "%2A")
				.Replace("'", "%27")
				.Replace("%7E", "~");
		}
	}

}
