#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
		public static async Task<string> Generate(List<string> log, GameStats stats, GameMetaData gameMetaData, bool includeDeck = false)
		{
			Directory.CreateDirectory(HsReplayPath);
			Directory.CreateDirectory(TmpDirPath);

			if(!File.Exists(HsReplayExe) || HsReplayUpdater.CheckForUpdate())
				await HsReplayUpdater.Update();
			if(!File.Exists(Msvcr100DllPath))
				File.Copy(Msvcr100DllHearthstonePath, Msvcr100DllPath);

			var result = ValidateLog(log);
			if(!result.IsValid)
				return null;

			using(var sw = new StreamWriter(TmpFilePath))
			{
				foreach(var line in log)
					sw.WriteLine(line);
			}

			RunExe(stats?.StartTime, result.IsPowerTaskList);

			if(new FileInfo(HsReplayOutput).Length == 0)
			{
				Log.Error("Not able to convert log file.");
				return null;
			}

			XmlHelper.AddData(HsReplayOutput, gameMetaData, stats, includeDeck);
			File.Delete(TmpFilePath);
			return HsReplayOutput;
		}

		private static ValidLogResult ValidateLog(List<string> log)
		{
			var result = new ValidLogResult {IsValid = log.Count > 0};
			if(!result.IsValid)
				return result;
			result.IsPowerTaskList = log[0].Contains("PowerTaskList.");
			var createGameLine = -1;
			for(var i = 0; i < log.Count - 1; i++)
			{
				if(log[i].Contains("CREATE_GAME"))
				{
					createGameLine = i;
					break;
				}
			}
			if(createGameLine == -1)
			{
				var debugPrintPowerLine = log.FirstOrDefault(x => x.Contains("DebugPrintPower"));
				if(debugPrintPowerLine == null)
				{
					result.IsValid = false;
					return result;
				}
				var createLine = new string(debugPrintPowerLine.TakeWhile(x => x != '-').ToArray()) + "- ";
				if(result.IsPowerTaskList)
					createLine += "    ";
				createLine += "CREATE_GAME";
				log.Insert(0, createLine);
			}
			return result;
		}

		private static void RunExe(DateTime? time, bool usePowerTaskList)
		{
			var dateString = time?.ToString("yyyy-MM-dd");
			var defaultDateArg = time.HasValue ? $"--default-date={dateString} " : "";
			var processorArg = usePowerTaskList ? "--processor=PowerTaskList " : "";
			var procInfo = new ProcessStartInfo
			{
				FileName = HsReplayExe,
				Arguments = defaultDateArg + processorArg + TmpFilePath,
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

	public class ValidLogResult
	{
		public bool IsValid { get; set; }
		public bool IsPowerTaskList { get; set; }
	}
}