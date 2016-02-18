#region

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Stats;
using static Hearthstone_Deck_Tracker.HsReplay.HsReplayConstants;

#endregion

namespace Hearthstone_Deck_Tracker.HsReplay
{
	public class HsReplayGenerator
	{
		private static XmlMetaData[] GetMetaData(GameStats stats, GameMetaData metaData)
			=>
				new[]
				{
					new XmlMetaData("x-hsbuild", metaData?.HearthstoneBuild),
					new XmlMetaData("x-address", metaData?.ServerAddress),
					new XmlMetaData("x-clientid", metaData?.ClientId),
					new XmlMetaData("x-spectateKey", metaData?.SpectateKey),
					new XmlMetaData("x-gameid", metaData?.GameId),
					new XmlMetaData("x-rank", stats?.Rank),
					new XmlMetaData("x-legendrank", metaData?.LegendRank)
				};

		public static async Task<string> Generate(List<string> log, GameStats stats, GameMetaData gameMetaData)
		{
			Directory.CreateDirectory(HsReplayPath);
			Directory.CreateDirectory(TmpDirPath);

			if(!File.Exists(HsReplayExe) || CheckForUpdate())
				await Update();

			using(var sw = new StreamWriter(TmpFilePath))
			{
				foreach(var line in log)
					sw.WriteLine(line);
			}

			RunExe();

			if(new FileInfo(HsReplayOutput).Length == 0)
			{
				Logger.WriteLine("Not able to convert log file.", "HsReplayGenerator");
				return null;
			}

			AddMetaData(HsReplayOutput, GetMetaData(stats, gameMetaData));
			File.Delete(TmpFilePath);
			return HsReplayOutput;
		}

		private static void RunExe()
		{
			var procInfo = new ProcessStartInfo
			{
				FileName = HsReplayExe,
				Arguments = TmpFilePath,
				CreateNoWindow = true,
				RedirectStandardOutput = true,
				UseShellExecute = false
			};
			var proc = Process.Start(procInfo);
			using(var sw = new StreamWriter(HsReplayOutput))
				sw.Write(proc?.StandardOutput.ReadToEnd());
			proc?.WaitForExit();
		}

		private static void AddMetaData(string xmlFile, XmlMetaData[] metaData)
		{
			var xml = XDocument.Load(xmlFile);
			var game = xml.Elements().FirstOrDefault(x => x.Name == "HSReplay")?.Elements().FirstOrDefault(x => x.Name == "Game");
			foreach(var pair in metaData)
				game?.SetAttributeValue(pair.Key, pair.Value);
			xml.Save(xmlFile);
		}

		private static async Task Update()
		{
			var version = "1.0";
			var zipPath = string.Format(ZipFilePath, version);
			Logger.WriteLine($"Downloading hsreplay converter version {version}...", "HsReplay");
			using(var wc = new WebClient())
				await wc.DownloadFileTaskAsync(string.Format(DownloadUrl, version), zipPath);
			Logger.WriteLine("Finished downloading. Unpacking...", "HsReplay");
			using(var fs = new FileInfo(zipPath).OpenRead())
			{
				var archive = new ZipArchive(fs, ZipArchiveMode.Read);
				archive.ExtractToDirectory(HsReplayPath, true);
			}
			File.Delete(zipPath);
		}

		private static bool CheckForUpdate()
		{
			//TODO
			return false;
		}
	}
}