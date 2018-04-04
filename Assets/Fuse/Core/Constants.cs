using System.IO;
using System.Reflection;
using UnityEngine;

namespace Fuse.Core
{
	public static class Constants
	{
		public const char DefaultSeparator = '/';
		public const string EditorBundlePath = "Bundles";
		public const string AssetExtension = ".asset";
		public const string BundleExtension = ".unity3d";

		public const string CoreAssetPath = "Assets/Bundles/Core";
		public const string StatesAssetPath = CoreAssetPath + "/States";
		public const string EnvironmentsAssetPath = CoreAssetPath + "/Environments";
		public const string CoreBundle = "core";
		public const string CoreBundleFile = CoreBundle + BundleExtension;

		public const string FeatureAssetPath = "Assets/Bundles/Features";
		public const string FeatureScriptsPath = "Assets/Scripts";
		public const string FeatureBundle = "{0}-feature";
		public const string FeatureBundleFile = FeatureBundle + BundleExtension;
		public const BindingFlags FeatureFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

		public const string ScenesAssetPath = "Assets/Bundles/Scenes";
		public const string SceneExtension = ".unity";
		private const string SceneBundle = "{0}-scene";
		private const string SceneBundleFile = SceneBundle + BundleExtension;

		public const string AssetsBakedEditorPath = "Assets/StreamingAssets/Bundles";

		public static readonly string AssetsBakedPath =
			Application.streamingAssetsPath + Path.DirectorySeparatorChar + "Bundles" +
			Path.DirectorySeparatorChar + "{0}";

		public static string GetSceneBundleFromPath(string path)
		{
			return string.Format(SceneBundle, ScenePathToBundleName(path));
		}

		public static string GetSceneBundleFileFromPath(string path)
		{
			return string.Format(SceneBundleFile, ScenePathToBundleName(path));
		}

		public static string GetFileNameFromPath(string path, string extension)
		{
			string[] values = path.Split(path.Contains("/") ? '/' : '\\');
			return values[values.Length - 1].Replace(extension, string.Empty);
		}

		private static string ScenePathToBundleName(string path)
		{
			string subPath = path.Replace(ScenesAssetPath + DefaultSeparator, string.Empty);
			return subPath.ToLower().Replace(" ", string.Empty).Replace(SceneExtension, string.Empty)
				.Replace(DefaultSeparator, '_');
		}

		public static string GetConfigurationAssetName()
		{
			return typeof(Configuration).Name;
		}

		public static string GetConfigurationAssetPath()
		{
			return CoreAssetPath + "/" + GetConfigurationAssetName() + AssetExtension;
		}
	}
}