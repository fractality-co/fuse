using System;
using System.Collections;
using System.IO;
using System.Reflection;
using Fuse.Core;
using Fuse.Implementation;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditorInternal;
using UnityEngine;
using Logger = Fuse.Core.Logger;
using State = Fuse.Core.State;

namespace Fuse.Editor
{
	/// <summary>
	/// Handles the generation and post-processing of assets for <see cref="Executor"/>.
	/// </summary>
	public class AssetGenerator : AssetPostprocessor
	{
		private const string SimulateMenuItem = "Fuse/Assets/Simulate %&m";

		[MenuItem("Fuse/Configure %&c")]
		private static void EditConfiguration()
		{
			Selection.activeObject = AssetDatabase.LoadAssetAtPath<ScriptableObject>(Constants.GetConfigurationAssetPath());
		}

		[MenuItem("Fuse/New/State %&s")]
		public static void ShowCreateStateWindow()
		{
			CreateStateWindow window = EditorWindow.GetWindow<CreateStateWindow>();
			window.OnCreate += CreateStateAsset;
		}

		[MenuItem(SimulateMenuItem)]
		public static void ToggleSimulateAssets()
		{
			AssetBundles.Simulate = !AssetBundles.Simulate;
		}

		[MenuItem(SimulateMenuItem, true)]
		public static bool ToggleSimulateAssetsValidate()
		{
			Menu.SetChecked(SimulateMenuItem, AssetBundles.Simulate);
			return true;
		}

		[MenuItem("Fuse/Assets/Clear Cache %&d")]
		public static void ClearAssets()
		{
			Caching.ClearCache();
			Resources.UnloadUnusedAssets();
			GC.Collect();
		}

		[MenuItem("Fuse/Assets/Build %&b")]
		public static void BuildAssets()
		{
			if (!InternalEditorUtility.inBatchMode)
			{
				bool result = EditorUtility.DisplayDialog("Build Assets?",
					"Do you want to build Assets for " + EditorUserBuildSettings.activeBuildTarget + "?", "Yes", "No");

				if (!result)
					return;
			}

			string outputPath = Constants.EditorBundlePath + Constants.DefaultSeparator +
			                    EditorUserBuildSettings.activeBuildTarget;
			outputPath = outputPath.Replace(Constants.DefaultSeparator, Path.DirectorySeparatorChar.ToString());

			if (!Directory.Exists(Constants.EditorBundlePath))
				Directory.CreateDirectory(Constants.EditorBundlePath);

			if (!Directory.Exists(outputPath))
				Directory.CreateDirectory(outputPath);

			var manifest = BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.ChunkBasedCompression,
				EditorUserBuildSettings.activeBuildTarget);
			if (manifest == null)
			{
				if (!InternalEditorUtility.inBatchMode)
					throw new Exception("Unable to build asset bundles for: " + EditorUserBuildSettings.activeBuildTarget + "!");

				Debug.LogError("Unable to build asset bundles for: " + EditorUserBuildSettings.activeBuildTarget + "!");
				EditorApplication.Exit(1);
				return;
			}

			DirectoryInfo buildDirectory = new DirectoryInfo(outputPath);
			string[] bundleNames = AssetDatabase.GetAllAssetBundleNames();
			FileInfo[] infos = buildDirectory.GetFiles();

			// delete manifests, old asset bundles, and the weird generated file from a Unity issue
			foreach (FileInfo fileInfo in infos)
				if (fileInfo.Name.Contains(".unity3d") || fileInfo.Name.Contains(".manifest") ||
				    fileInfo.Name == buildDirectory.Name)
					File.Delete(fileInfo.FullName);

			// lastly rename existing ones to proper extension
			foreach (FileInfo fileInfo in infos)
				if (((IList) bundleNames).Contains(fileInfo.Name))
					File.Move(fileInfo.FullName, fileInfo.FullName + ".unity3d");

			Debug.Log("Built assets for: " + EditorUserBuildSettings.activeBuildTarget);

			if (!InternalEditorUtility.inBatchMode)
				EditorUtility.DisplayDialog("Built Assets",
					"Assets built to\"" + outputPath + "\".",
					"Ok");
		}

		private static void CreateStateAsset(string name)
		{
			string path = Constants.StatesAssetPath + "/" + name + ".asset";

			State state = ScriptableObject.CreateInstance<State>();
			state.name = name;

			AssetDatabase.CreateAsset(state, path);
			Selection.activeObject = state;

			Logger.Info("Created new " + typeof(State).Name + ": " + state.name);
		}

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
			EditorUtils.PreparePath(Constants.CoreAssetPath);
			EditorUtils.PreparePath(Constants.StatesAssetPath);

			AssetImporter importer = AssetImporter.GetAtPath(Constants.CoreAssetPath);
			if (importer != null)
				importer.SetAssetBundleNameAndVariant(Constants.CoreBundle, string.Empty);

			ScriptableObject asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(Constants.GetConfigurationAssetPath());
			if (asset == null)
			{
				asset = ScriptableObject.CreateInstance<Configuration>();
				asset.name = Constants.GetConfigurationAssetName();
				AssetDatabase.CreateAsset(asset, Constants.GetConfigurationAssetPath());
			}
		}

		private static void ProcessImplementations()
		{
			EditorUtils.PreparePath(Constants.ImplementationAssetPath);

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
			string path = Constants.ImplementationAssetPath + "/" + type.Name;
			if (!AssetDatabase.IsValidFolder(path))
			{
				string assetName = type.Name;
				string assetPath = path + "/" + assetName + ".asset";
				ScriptableObject asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
				if (asset == null)
				{
					EditorUtils.PreparePath(path);
					asset = ScriptableObject.CreateInstance(type);
					asset.name = assetName;
					AssetDatabase.CreateAsset(asset, assetPath);
					Logger.Info("Created implementation: " + assetName + " [" + implementation + "]");
				}
			}

			AssetImporter importer = AssetImporter.GetAtPath(path);
			if (importer != null)
			{
				string bundleName = type.Name.ToLower();
				importer.SetAssetBundleNameAndVariant(string.Format(Constants.ImplementationBundle, bundleName),
					string.Empty);
			}
		}
	}

	public class CreateStateWindow : EditorWindow
	{
		public Action<string> OnCreate;

		private string _name = string.Empty;

		public CreateStateWindow()
		{
			titleContent = new GUIContent("State");
			maxSize = minSize = new Vector2(350, 60);
		}

		private void OnGUI()
		{
			GUILayout.Space(10);

			_name = EditorGUILayout.TextField("Name", _name);
			_name = _name.Replace(" ", string.Empty);

			GUI.enabled = Validate();

			GUILayout.Space(5);
			if (GUILayout.Button("Create"))
			{
				OnCreate(_name);
				Close();
			}

			GUI.enabled = true;
		}

		private bool Validate()
		{
			return !string.IsNullOrEmpty(_name);
		}
	}
}