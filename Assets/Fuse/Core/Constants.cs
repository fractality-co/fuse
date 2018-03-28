namespace Fuse.Core
{
	public static class Constants
	{
		public const string CoreAssetPath = "Assets/Bundles/Core";
		public const string StatesAssetPath = CoreAssetPath + "/States";
		public const string ImplementationAssetPath = "Assets/Bundles/Implementations";
		public const string ImplementationScriptsPath = "Assets/Scripts";

		public const string AssetExtension = ".asset";
		public const string BundleExtension = ".unity3d";
		public const string CoreBundle = "fuse-core";
		public const string CoreBundleFile = CoreBundle + BundleExtension;
		public const string ImplementationBundle = "{0}";
		public const string ImplementationBundleFile = ImplementationBundle + BundleExtension;

		public static string GetConfigurationAssetName()
		{
			return typeof(Configuration).Name;
		}

		public static string GetConfigurationAssetPath()
		{
			return CoreAssetPath + "/" + GetConfigurationAssetName() + AssetExtension;
		}

		public static string GetImplementationAssetPath(string implementation, string name)
		{
			return ImplementationAssetPath + "/" + implementation + "/" + name + AssetExtension;
		}
	}
}