#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Newtonsoft.Json;

#endregion

namespace Hearthstone_Deck_Tracker.Utility
{
	public class GitHub
	{
		public static async Task<GithubRelease> CheckForUpdate(string user, string repo, Version version)
		{
			try
			{
				var latest = await GetLatestRelease(user, repo);
				if(latest.Assets.Count == 0)
					return null;
				var v = new Version(latest.Tag);
				if(v.CompareTo(version) > 0)
					return latest;
			}
			catch(Exception e)
			{
				Log.Error(e);
			}
			return null;
		}

		private static async Task<GithubRelease> GetLatestRelease(string user, string repo)
		{
			try
			{
				string json;
				using(var wc = new WebClient())
				{
					wc.Headers.Add(HttpRequestHeader.UserAgent, user);
					json = await wc.DownloadStringTaskAsync($"https://api.github.com/repos/{user}/{repo}/releases/latest");
				}
				return JsonConvert.DeserializeObject<GithubRelease>(json);
			}
			catch(Exception ex)
			{
				throw ex;
			}
		}

		public static async Task<string> DownloadRelease(GithubRelease release, string downloadDirectory)
		{
			try
			{
				var path = Path.Combine(downloadDirectory, release.Assets[0].Name);
				using(var wc = new WebClient())
					await wc.DownloadFileTaskAsync(release.Assets[0].Url, path);
				return path;
			}
			catch(Exception e)
			{
				Log.Error(e);
				return null;
			}
		}

		public class GithubRelease
		{
			[JsonProperty("tag_name")]
			public string Tag { get; set; }

			[JsonProperty("assets")]
			public List<Asset> Assets { get; set; }

			public class Asset
			{
				[JsonProperty("browser_download_url")]
				public string Url { get; set; }

				[JsonProperty("name")]
				public string Name { get; set; }
			}
		}
	}
}