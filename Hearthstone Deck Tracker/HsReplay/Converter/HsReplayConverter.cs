#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Utility.Logging;
using static Hearthstone_Deck_Tracker.HsReplay.Constants;

#endregion

namespace Hearthstone_Deck_Tracker.HsReplay.Converter
{
	internal class HsReplayConverter
	{
		public static async Task<string> Convert(List<string> log, GameStats stats, GameMetaData gameMetaData, bool includeDeck = false)
		{
			Log.Info($"Converting hsreplay, game={{{stats}}}");
			var setup = await Setup();
			if(!setup)
				return null;
			var result = LogValidator.Validate(log);
			if(!result.Valid)
				return null;
			try
			{
				using(var sw = new StreamWriter(TmpFilePath))
				{
					foreach(var line in log)
						sw.WriteLine(line);
				}
			}
			catch(Exception e)
			{
				Log.Error(e);
				return null;
			}
			var success = await RunExeAsync(stats?.StartTime, result.IsPowerTaskList);
			if(!success)
				return null;
			if(new FileInfo(HsReplayOutput).Length == 0)
			{
				Log.Error("Converter output is empty.");
				return null;
			}
			XmlHelper.AddData(HsReplayOutput, gameMetaData, stats, includeDeck);
			try
			{
				File.Delete(TmpFilePath);
			}
			catch(Exception e)
			{
				Log.Error(e);
			}
			return HsReplayOutput;
		}

		private static async Task<bool> Setup()
		{
			try
			{
				Directory.CreateDirectory(HsReplayPath);
				Directory.CreateDirectory(TmpDirPath);
				if(!File.Exists(HsReplayExe) || HsReplayUpdater.CheckForUpdate())
					await HsReplayUpdater.Update();
				if(!File.Exists(Msvcr100DllPath))
					File.Copy(Msvcr100DllHearthstonePath, Msvcr100DllPath);
				return true;
			}
			catch(Exception e)
			{
				Log.Error(e);
				return false;
			}
		}

		private static async Task<bool> RunExeAsync(DateTime? time, bool usePowerTaskList)
		{
			try
			{
				return await Task.Run(() => RunExe(time, usePowerTaskList));
			}
			catch(Exception e)
			{
				Log.Error(e);
				return false;
			}
		}

		private static bool RunExe(DateTime? time, bool usePowerTaskList)
		{
			var dateString = time?.ToString("yyyy-MM-dd");
			var defaultDateArg = time.HasValue ? $"--default-date={dateString} " : "";
			var processorArg = usePowerTaskList ? "--processor=PowerTaskList " : "";
			try
			{
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
				return true;
			}
			catch(Exception e)
			{
				Log.Error(e);
				return false;
			}
		}
	}
}