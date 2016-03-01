#region

using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Utility.Logging;
using static Hearthstone_Deck_Tracker.HsReplay.Constants;
using Hearthstone_Deck_Tracker.Utility.Extensions;

#endregion

namespace Hearthstone_Deck_Tracker.HsReplay
{
	internal class HsReplayUpdater
	{
		internal static bool CheckForUpdate()
		{
			//TODO
			return false;
		}

		internal static async Task Update()
		{
			var version = "0.1";
			var zipPath = string.Format(ZipFilePath, version);
			Log.Info($"Downloading hsreplay converter version {version}...");
			using(var wc = new WebClient())
				await wc.DownloadFileTaskAsync(string.Format(DownloadUrl, version), zipPath);
			Log.Info("Finished downloading. Unpacking...");
			using(var fs = new FileInfo(zipPath).OpenRead())
			{
				var archive = new ZipArchive(fs, ZipArchiveMode.Read);
				archive.ExtractToDirectory(HsReplayPath, true);
			}
			File.Delete(zipPath);
		}
	}
}