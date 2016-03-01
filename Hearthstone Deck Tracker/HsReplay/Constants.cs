#region

using System.IO;

#endregion

namespace Hearthstone_Deck_Tracker.HsReplay
{
	public class Constants
	{
		private const string BaseUrl = "http://hsreplay.net";
		private const string UploadApi = "/api/v1/replay/upload";
		public const string DownloadUrl = "https://github.com/Epix37/HSReplayFreezer/releases/download/{0}/hsreplay-{0}.zip";
		public const string ZipFile = "hsreplay-{0}.zip";
		private const string Msvcr100Dll = "msvcr100.dll";
		private const string HsReplayXmlFile = "hslog.xml";
		private const string TmpPowerLogFile = "tmp.log";
		private const string HsReplayDir = "HsReplay";
		private const string HsReplayExeFilename = "convert.exe";
		private const string TmpDir = "temp";
		private const string VersionFile = "version";
		private const string BuildDatesFile = "builddates.xml";

		public static string UploadUrl => BaseUrl + UploadApi;
		public static string BuildDateFilePath => Path.Combine(HsReplayPath, BuildDatesFile);
		public static string Msvcr100DllHearthstonePath => Path.Combine(Config.Instance.HearthstoneDirectory, Msvcr100Dll);
		public static string Msvcr100DllPath => Path.Combine(HsReplayPath, Msvcr100Dll);
		public static string TmpDirPath => Path.Combine(HsReplayPath, TmpDir);
		public static string TmpFilePath => Path.Combine(TmpDirPath, TmpPowerLogFile);
		public static string HsReplayPath => Path.Combine(Config.AppDataPath, HsReplayDir);
		public static string HsReplayExe => Path.Combine(HsReplayPath, HsReplayExeFilename);
		public static string HsReplayOutput => Path.Combine(TmpDirPath, HsReplayXmlFile);
		public static string VersionFilePath => Path.Combine(HsReplayPath, VersionFile);
		public static string ZipFilePath => Path.Combine(HsReplayPath, ZipFile);
	}
}