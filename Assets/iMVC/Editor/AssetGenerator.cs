using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace iMVC.Editor
{
	public class AssetGenerator : AssetPostprocessor
	{
		private const string CoreAssetPath = "Assets/Bundles/Core";
		private const string StatesAssetPath = CoreAssetPath + "/States";
		private const string CoreBundleName = "imvc.core";
		private const string ImplementationAssetPath = "Assets/Bundles/Implementations";
		private const string ImplementationBundleName = "imvc.{0}.{1}";

		private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
			string[] movedAssets,
			string[] movedFromAssetPaths)
		{
			ProcessAssets();
		}

		[DidReloadScripts]
		private static void ProcessAssets()
		{
			ProcessCore();
			ProcessImplementations();

			AssetDatabase.RemoveUnusedAssetBundleNames();
			AssetDatabase.SaveAssets();
		}

		private static void ProcessCore()
		{
			Utils.PreparePath(CoreAssetPath);
			Utils.PreparePath(StatesAssetPath);

			AssetImporter importer = AssetImporter.GetAtPath(CoreAssetPath);
			if (importer != null)
				importer.SetAssetBundleNameAndVariant(CoreBundleName, string.Empty);

			string assetName = typeof(Configuration).Name;
			string assetPath = CoreAssetPath + "/" + assetName + ".asset";
			ScriptableObject asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
			if (asset == null)
			{
				asset = ScriptableObject.CreateInstance<Configuration>();
				asset.name = assetName;
				AssetDatabase.CreateAsset(asset, assetPath);
			}
		}

		private static void ProcessImplementations()
		{
			Utils.PreparePath(ImplementationAssetPath);

			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (Type type in assembly.GetTypes())
				{
					object[] implemenations = type.GetCustomAttributes(typeof(ImplementationAttribute), true);
					if (implemenations.Length > 0)
						SyncImplementation(type, implemenations[0] as ImplementationAttribute);
				}
			}
		}

		private static void SyncImplementation(Type type, ImplementationAttribute implementation)
		{
			// TODO: move to correct folders, migrate re-named, cleanup removed implementations

			string path = ImplementationAssetPath + "/" + implementation + "/" + type.Name;
			IEnumerable<string> assetNames = implementation.GetAssets(type);
			foreach (string assetName in assetNames)
			{
				string assetPath = path + "/" + assetName + ".asset";
				ScriptableObject asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
				if (asset == null)
				{
					Utils.PreparePath(path);
					AssetImporter importer = AssetImporter.GetAtPath(path);
					if (importer != null)
					{
						string bundleType = implementation.ToString().ToLower();
						string bundleName = type.Name.ToLower().Replace(implementation.ToString().ToLower(), string.Empty);
						importer.SetAssetBundleNameAndVariant(string.Format(ImplementationBundleName, bundleType, bundleName),
							string.Empty);
					}

					asset = ScriptableObject.CreateInstance(type);
					asset.name = assetName;
					AssetDatabase.CreateAsset(asset, assetPath);

					Logger.Info("Created implementation: " + assetName + " [" + implementation + "]");
				}
			}
		}
	}
}