using System.Net;
using System.Text;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace Hearthstone_Deck_Tracker.HsReplay.API
{
	internal class Web
	{
		public static async Task<WebResponse> PostAsync(string url, string data)
		{
			try
			{
				var request = CreatePostRequest(url);
				using(var stream = await request.GetRequestStreamAsync())
					stream.Write(Encoding.UTF8.GetBytes(data), 0, data.Length);
				return await request.GetResponseAsync();
			}
			catch(WebException e)
			{
				Log.Error(e);
				throw;
			}
		}

		private static HttpWebRequest CreatePostRequest(string url) => CreateRequest(url, "POST");

		private static HttpWebRequest CreateRequest(string url, string method)
		{
			var request = (HttpWebRequest)WebRequest.Create(url);
			request.ContentType = "application/json";
			request.Accept = "application/json";
			request.Method = method;
			return request;
		}
	}
}