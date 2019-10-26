using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

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

		public const int FeatureFolderDepth = 4;
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

#if UNITY_EDITOR
		public static string GetPlatformName(BuildTarget buildTarget)
		{
			return GetPlatformName(buildTarget.ToRuntimePlatform());
		}
#endif

		public static string GetPlatformName(RuntimePlatform platform)
		{
			return Strip(platform.ToString(), new[] {"Player", "Editor", "X86", "X64", "ARM"}).ToLower();
		}

		private static string Strip(string value, IEnumerable<string> toStrip)
		{
			return toStrip.Aggregate(value, (current, strip) => current.Replace(strip, string.Empty));
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