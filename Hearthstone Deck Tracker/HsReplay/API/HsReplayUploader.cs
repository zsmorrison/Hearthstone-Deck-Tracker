#region

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Utility.Logging;

#endregion

namespace Hearthstone_Deck_Tracker.HsReplay.API
{
	internal class HsReplayUploader
	{
		public static async Task<UploadResult> UploadXml(string xml)
		{
			Log.Info("Uploading...");
			try
			{
				var response = await Web.PostAsync(Constants.UploadUrl, xml);
				var location = response.Headers["Location"];
				var id = location.Split('/').Last();
				Log.Info("Success!");
				return UploadResult.Successful(id);
			}
			catch(Exception e)
			{
				Log.Error(e);
				return UploadResult.Failed;
			}
		}

		public static async Task<UploadResult> UploadXmlFromFile(string filePath)
		{
			string content;
			using(var sr = new StreamReader(filePath))
				content = sr.ReadToEnd();
			return await UploadXml(content);
		}
	}
}