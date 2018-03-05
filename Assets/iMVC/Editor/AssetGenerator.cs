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
		[MenuItem("Window/iMVC/Configure %#c")]
		[MenuItem("Assets/iMVC/Configure")]
		public static void SelectConfiguration()
		{
			Selection.activeObject = Configuration.Load();
		}

		private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
			string[] movedAssets,
			string[] movedFromAssetPaths)
		{
			ProcessConfiguraton();
			ProcessImplementations();
		}

		private static void ProcessConfiguraton()
		{
			string[] assets = AssetDatabase.FindAssets("t:" + typeof(Configuration).Name);
			if (assets.Length == 0)
			{
				string path = Configuration.RootConfigurationPath;
				Utils.PreparePath(path);

				ScriptableObject configuration = ScriptableObject.CreateInstance<Configuration>();
				configuration.name = Configuration.AssetName;
				AssetDatabase.CreateAsset(configuration, Configuration.FullConfigurationPath);

				Logger.Info("Initialized iMVC Configuration ...");
			}
		}

		[DidReloadScripts]
		private static void ProcessImplementations()
		{
			// TODO: move all implementations to their correct folders

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
			// TODO: migrate re-named implementations
			// TODO: cleanup removed implementations

			Configuration config = Configuration.Load();
			string path = config.FullLoadPath + "/" + implementation + "/" + type.Name;
			IEnumerable<string> assetNames = implementation.GetAssets(type);
			foreach (string assetName in assetNames)
			{
				string assetPath = path + "/" + assetName + ".asset";
				ScriptableObject asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
				if (asset == null)
				{
					Utils.PreparePath(path);

					asset = ScriptableObject.CreateInstance(type);
					asset.name = assetName;
					AssetDatabase.CreateAsset(asset, assetPath);

					Logger.Info("Created implementation: " + assetName + " [" + implementation + "]");
				}
			}

			AssetDatabase.SaveAssets();
		}
	}
}