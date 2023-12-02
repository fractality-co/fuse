using System.IO;
using UnityEngine;

namespace Fuse.Editor
{
	/// <summary>
	/// Manages the licensing information for <see cref="Fuse"/>.
	/// </summary>
	public static class License
	{
		private static string _version;

		/// <summary>
		/// The current version found within the licensing information.
		/// </summary>
		public static string Version
		{
			get
			{
				if (string.IsNullOrEmpty(_version))
					_version = FetchVersion();

				return _version;
			}
		}

		public static Package FetchPackage()
		{
			var package = File.ReadAllText(PathUtility.BasePath + "/package.json");
			return JsonUtility.FromJson<Package>(package);
		}

		public static void IncreaseVersion()
		{
			var newPackage = FetchPackage().IncreaseVersion();
			File.WriteAllText(PathUtility.BasePath + "/package.json", JsonUtility.ToJson(newPackage, true));
			Logger.Info("Increased package version to: " + newPackage.version);
		}

		public static string FetchVersion()
		{
			try
			{
				return "v" + FetchPackage().version;
			}
			catch
			{
				return "(???)";
			}
		}
	}
}