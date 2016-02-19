#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Utility.Logging;
using static Hearthstone_Deck_Tracker.HsReplay.HsReplayConstants;

#endregion

namespace Hearthstone_Deck_Tracker.HsReplay
{
	public class HsReplayGenerator
	{
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

			RunExe(stats?.StartTime);

			if(new FileInfo(HsReplayOutput).Length == 0)
			{
				Log.Error("Not able to convert log file.");
				return null;
			}

			AddMetaData(HsReplayOutput, gameMetaData, stats);
			File.Delete(TmpFilePath);
			return HsReplayOutput;
		}

		private static void RunExe(DateTime? time)
		{
			var dateString = time?.ToString("yyyy-MM-dd");
			var defaultDateArg = time.HasValue ? $"--default-date={dateString} " : "";
			var procInfo = new ProcessStartInfo
			{
				FileName = HsReplayExe,
				Arguments = defaultDateArg + TmpFilePath,
				CreateNoWindow = true,
				RedirectStandardOutput = true,
				UseShellExecute = false
			};
			var proc = Process.Start(procInfo);
			using(var sw = new StreamWriter(HsReplayOutput))
				sw.Write(proc?.StandardOutput.ReadToEnd());
			proc?.WaitForExit();
		}

		private static void AddMetaData(string xmlFile, GameMetaData gameMetaData, GameStats stats)
		{
			var xml = XDocument.Load(xmlFile);
			var hsReplay = xml.Elements().FirstOrDefault(x => x.Name == "HSReplay");
			if(hsReplay == null)
				return;
			hsReplay.SetAttributeValue("build", gameMetaData?.HearthstoneBuild);
			var game = hsReplay.Elements().FirstOrDefault(x => x.Name == "Game");
			if(game != null)
			{
				if(stats != null)
				{
					var mode = HearthDbConverter.GetGameType(stats.GameMode);
					if (mode != GameType.GT_UNKNOWN)
						game.SetAttributeValue("type", (int)mode);
				}
				game.SetAttributeValue("id", gameMetaData?.GameId);
				game.SetAttributeValue("x-address", gameMetaData?.ServerAddress);
				game.SetAttributeValue("x-clientid", gameMetaData?.ClientId);
				game.SetAttributeValue("x-spectateKey", gameMetaData?.SpectateKey);
				var player = game.Elements().FirstOrDefault(x => x.Name == "Player" && x.Attributes().Any(a => a.Name == "name" && a.Value == stats?.PlayerName));
				if (stats?.Rank > 0)
					player?.SetAttributeValue("rank", stats.Rank);
				if (gameMetaData?.LegendRank > 0)
					player?.SetAttributeValue("legendRank", gameMetaData.LegendRank);
				if(player != null && stats != null && stats.DeckId != Guid.Empty)
					AddDeckList(player, stats);
				if(stats?.OpponentRank > 0)
					game.Elements().FirstOrDefault(x => x.Name == "Player" && x.Attributes().Any(a => a.Name == "name" && a.Value == stats.OpponentName))?
								   .SetAttributeValue("rank", stats.OpponentRank);
			}
			xml.Save(xmlFile);
		}

		private static void AddDeckList(XElement player, GameStats stats)
		{
			var deck = DeckList.Instance.Decks.FirstOrDefault(x => x.DeckId == stats.DeckId)?.GetVersion(stats.PlayerDeckVersion);
			if(deck == null)
				return;
			var xmlDeck = new XElement("Deck");
			foreach(var card in deck.Cards)
			{
				var xmlCard = new XElement("Card");
				xmlCard.SetAttributeValue("id", card.Id);
				if(card.Count > 1)
					xmlCard.SetAttributeValue("count", card.Count);
				xmlDeck.Add(xmlCard);
			}
			player.Add(xmlDeck);
		}

		private static async Task Update()
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

		private static bool CheckForUpdate()
		{
			//TODO
			return false;
		}
	}
}