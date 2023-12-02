using System.IO;
using System.Linq;
using UnityEngine;

namespace Fuse
{
	/// <summary>
	/// Manages the centralized constants for Fuse that are needed for both runtime and editor.
	/// </summary>
	public static class Constants
	{
		public const char DefaultSeparator = '/';
		public const string EditorBuildsPath = "Builds";
		public const string EditorBundlePath = "Bundles";
		public const string AssetExtension = ".asset";
		public const string BundleExtension = "";

		public const string CoreAssetPath = "Assets/Bundles/Core";
		public const string StatesAssetPath = CoreAssetPath + "/States";
		public const string EnvironmentsAssetPath = CoreAssetPath + "/Environments";
		public const string CoreBundle = "core";
		public const string CoreBundleFile = CoreBundle + BundleExtension;

		public const string ScenesAssetPath = "Assets/Bundles/Scenes";
		public const string SceneExtension = ".unity";
		private const string SceneBundle = "{0}_scene";
		private const string SceneBundleFile = SceneBundle + BundleExtension;

		public const string ContentAssetPath = "Assets/Bundles/Content";
		public const string ContentExtension = ".asset";
		private const string ContentBundle = "{0}_content";
		private const string ContentBundleFile = ContentBundle + BundleExtension;

		public const string AssetsBakedRootEditorPath = "Assets/StreamingAssets";
		public const string AssetsBakedEditorPath = AssetsBakedRootEditorPath + "/" + AssetsBakedRoot;
		public const string AssetsBakedRoot = "Bundles";

		public static readonly string AssetsBakedPath =
			Application.streamingAssetsPath + Path.DirectorySeparatorChar + AssetsBakedRoot +
			Path.DirectorySeparatorChar + "{0}";

		public static string GetContentBundleFileFromPath(string path) { return string.Format(ContentBundleFile, ContentPathToBundleName(path)); }

		public static string GetContentBundleFromPath(string path) { return string.Format(ContentBundle, ContentPathToBundleName(path)); }

		public static string GetSceneBundleFromPath(string path) { return string.Format(SceneBundle, ScenePathToBundleName(path)); }

		public static string GetSceneBundleFileFromPath(string path) { return string.Format(SceneBundleFile, ScenePathToBundleName(path)); }

		public static string GetFileNameFromPath(string path, string extension)
		{
			var values = path.Split(path.Contains("/") ? '/' : '\\');
			return string.IsNullOrEmpty(extension) ? values.LastOrDefault() : values.Last().Replace(extension, string.Empty);
		}

		private static string ScenePathToBundleName(string path)
		{
			var subPath = path.Replace(ScenesAssetPath + DefaultSeparator, string.Empty);
			return subPath.ToLower().Replace(" ", string.Empty).Replace(SceneExtension, string.Empty)
				.Replace(DefaultSeparator, '_');
		}

		private static string ContentPathToBundleName(string path)
		{
			var filename = Path.GetFileNameWithoutExtension(path);
			var subPath = path.Replace(ContentAssetPath + DefaultSeparator + filename + DefaultSeparator, string.Empty);
			var bundle = subPath.Replace(ContentExtension, string.Empty).Replace(" ", string.Empty).ToLower();
			return bundle;
		}

		public static string GetConfigurationAssetPath() { return CoreAssetPath + "/" + nameof(Configuration) + AssetExtension; }
	}
}