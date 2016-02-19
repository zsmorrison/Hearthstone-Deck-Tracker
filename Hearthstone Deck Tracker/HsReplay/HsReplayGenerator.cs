#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
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

			if(!File.Exists(HsReplayExe) || HsReplayUpdater.CheckForUpdate())
				await HsReplayUpdater.Update();
			if(!File.Exists(Msvcr100DllPath))
				File.Copy(Msvcr100DllHearthstonePath, Msvcr100DllPath);

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

			XmlHelper.AddData(HsReplayOutput, gameMetaData, stats);
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
	}
}