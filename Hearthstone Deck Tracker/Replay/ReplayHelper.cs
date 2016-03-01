#region

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.HsReplay.Converter;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Utility.Logging;

#endregion

namespace Hearthstone_Deck_Tracker.Replay
{
	public class ReplayHelper
	{
		private const string HdtReplayFile = "replay.json";
		private const string RawLogFile = "output_log.txt";
		private const string HsReplayFile = "hsreplay.xml";
		private readonly GameStats _game;

		public ReplayHelper(GameStats game)
		{
			_game = game;
			if(!ReplayExists)
				return;
			using(var fs = new FileStream(FilePath, FileMode.Open))
			using(var archive = new ZipArchive(fs, ZipArchiveMode.Read))
			{
				HasHdtReplayFile = archive.Entries.Any(x => x.Name == HdtReplayFile);
				if(archive.Entries.Any(x => x.Name == HsReplayFile))
				{
					using(var sr = new StreamReader(archive.GetEntry(HsReplayFile).Open()))
						HsReplay = sr.ReadToEnd();
				}
				if(archive.Entries.Any(x => x.Name == RawLogFile))
				{
					using(var sr = new StreamReader(archive.GetEntry(RawLogFile).Open()))
						RawLog = sr.ReadToEnd();
				}
			}
		}

		public string FilePath => Path.Combine(Config.Instance.ReplayDir, _game.ReplayFile);
		public bool ReplayExists => File.Exists(FilePath);
		public bool HasHdtReplayFile { get; }
		public bool HasRawLogFile => !string.IsNullOrEmpty(RawLog);
		public bool HasHsReplayFile => !string.IsNullOrEmpty(HsReplay);
		public string RawLog { get; }
		public string HsReplay { get; private set; }

		public void StoreHsReplay(string filePath)
		{
			if(HasHsReplayFile)
				return;
			try
			{
				using(var sr = new StreamReader(filePath))
				{
					var hsreplay = sr.ReadToEnd();
					using(var fs = new FileStream(FilePath, FileMode.Open))
					using(var archive = new ZipArchive(fs, ZipArchiveMode.Create))
					using(var sw = new StreamWriter(archive.CreateEntry(HsReplayFile).Open()))
						sw.Write(hsreplay);
					HsReplay = hsreplay;
				}
			}
			catch(Exception ex)
			{
				Log.Error(ex);
			}
		}

		public async Task<bool> GenerateHsReplay()
		{
			if(!HasRawLogFile)
				return false;
			var log = RawLog.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).ToList();
			var output = await HsReplayConverter.Convert(log, _game, null);
			if(output == null)
				return false;
			StoreHsReplay(output);
			return true;
		}
	}
}