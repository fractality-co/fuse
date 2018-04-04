using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Fuse.Core;
using Fuse.Feature;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;
using Environment = Fuse.Core.Environment;
using Logger = Fuse.Core.Logger;
using State = Fuse.Core.State;

namespace Fuse.Editor
{
	/// <summary>
	/// Handles the generation and post-processing of assets for <see cref="Fuse"/>.
	/// </summary>
	public class AssetGenerator : AssetPostprocessor, IPreprocessBuild
	{
		private const string SimulateMenuItem = "Fuse/Assets/Simulate %&m";

		public int callbackOrder
		{
			get { return 0; }
		}

		public void OnPreprocessBuild(BuildTarget target, string path)
		{
			if (AssetBundles.Simulate)
				Logger.Exception("You can not make a build while in asset simulation mode.");
		}

		[MenuItem("Fuse/Configure %&c")]
		private static void EditConfiguration()
		{
			Selection.activeObject = AssetDatabase.LoadAssetAtPath<ScriptableObject>(Constants.GetConfigurationAssetPath());
		}

		[MenuItem("Fuse/New/State %&s")]
		public static void ShowCreateStateWindow()
		{
			CreateAssetWindow window = EditorWindow.GetWindow<CreateAssetWindow>();
			window.Setup("State");
			window.OnCreate += CreateStateAsset;
		}

		[MenuItem("Fuse/New/Environment %&e")]
		public static void ShowCreateEnvironmentWindow()
		{
			CreateAssetWindow window = EditorWindow.GetWindow<CreateAssetWindow>();
			window.Setup("Environment");
			window.OnCreate += CreateEnvironmentAsset;
		}

		[MenuItem("Fuse/New/Scene %&l")]
		public static void ShowCreateLevelWindow()
		{
			CreateAssetWindow window = EditorWindow.GetWindow<CreateAssetWindow>();
			window.Setup("Scene");
			window.OnCreate += CreateScene;
		}

		[MenuItem(SimulateMenuItem)]
		public static void ToggleSimulateAssets()
		{
			if (AssetBundles.Simulate)
			{
				if (!AreAssetsBuilt())
					BuildAssets();

				IntegrateAssets();
				AssetBundles.Simulate = false;

				ProcessScenes();
			}
			else
			{
				RemoveIntegratedAssets();
				AssetBundles.Simulate = true;
				ProcessScenes();

				EditorUtility.DisplayDialog("Simulation Mode",
					"Integrated assets removed, you are now in simulation mode.",
					"Ok");
			}
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
		public static void StartBuildAssets()
		{
			BuildAssets();

			if (!AssetBundles.Simulate)
				IntegrateAssets();
			else
				EditorUtility.DisplayDialog("Built Assets",
					"Assets built to \"" + GetAssetOutput() + "\".",
					"Ok");
		}

		private static bool AreAssetsBuilt()
		{
			return Directory.Exists(GetAssetOutput());
		}

		private static void IntegrateAssets()
		{
			RemoveIntegratedAssets();
			EditorUtils.PreparePath(Constants.AssetsBakedEditorPath);

			Configuration configuration = AssetDatabase.LoadAssetAtPath<Configuration>(Constants.GetConfigurationAssetPath());
			Environment environment = AssetDatabase.LoadAssetAtPath<Environment>(configuration.Environment);

			if (environment == null)
			{
				Logger.Warn("Unable to integrate assets, no environment in Configuraton.");
				return;
			}

			if (environment.Loading == LoadMethod.Baked)
			{
				EditorUtils.DirectoryCopy(GetAssetOutput(), GetAssetIntegration(), true);
			}
			else
			{
				string sourceCorePath = GetAssetOutput() + Path.DirectorySeparatorChar + Constants.CoreBundleFile;
				string destCorePath = GetAssetIntegration() + Path.DirectorySeparatorChar + Constants.CoreBundleFile;
				File.Copy(sourceCorePath, destCorePath, true);
			}

			AssetDatabase.Refresh();

			EditorUtility.DisplayDialog("Integrated Assets",
				"Assets integrated into current project, and built to \"" + GetAssetOutput() + "\".", "Ok");
		}

		private static void RemoveIntegratedAssets()
		{
			if (Directory.Exists(GetAssetIntegration()))
			{
				Directory.Delete(GetAssetIntegration(), true);
				AssetDatabase.Refresh();
			}
		}

		private static void BuildAssets()
		{
			string outputPath = GetAssetOutput();

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

			foreach (FileInfo fileInfo in infos)
				if (fileInfo.Name.Contains(Constants.BundleExtension) || fileInfo.Name.Contains(".manifest") ||
				    fileInfo.Name == buildDirectory.Name)
					File.Delete(fileInfo.FullName);

			foreach (FileInfo fileInfo in infos)
				if (((IList) bundleNames).Contains(fileInfo.Name))
					File.Move(fileInfo.FullName, fileInfo.FullName + Constants.BundleExtension);

			Debug.Log("Built assets for: " + EditorUserBuildSettings.activeBuildTarget);
		}

		private static string GetAssetIntegration()
		{
			return EditorUtils.SystemPath(Constants.AssetsBakedEditorPath);
		}

		private static string GetAssetOutput()
		{
			return (Constants.EditorBundlePath + Constants.DefaultSeparator +
			        EditorUserBuildSettings.activeBuildTarget)
				.Replace(Constants.DefaultSeparator, Path.DirectorySeparatorChar);
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

		private static void CreateEnvironmentAsset(string name)
		{
			string path = Constants.EnvironmentsAssetPath + "/" + name + ".asset";

			Environment environment = ScriptableObject.CreateInstance<Environment>();
			environment.name = name;

			AssetDatabase.CreateAsset(environment, path);
			Selection.activeObject = environment;

			Logger.Info("Created new " + typeof(Environment).Name + ": " + environment.name);
		}

		private static void CreateScene(string name)
		{
			string path = Constants.ScenesAssetPath + "/" + name + ".unity";

			Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
			EditorSceneManager.SaveScene(scene, path);
			SceneManager.UnloadSceneAsync(scene);
			Selection.activeObject = AssetDatabase.LoadAssetAtPath(path, typeof(SceneAsset));

			Logger.Info("Created new level (Scene): " + name);
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
			const string title = "Processing Assets";

			EditorUtility.DisplayProgressBar(title, "Processing core", 0);
			ProcessCore();

			EditorUtility.DisplayProgressBar(title, "Processing scenes", 0.5f);
			ProcessScenes();

			EditorUtility.DisplayProgressBar(title, "Processing features", 0.5f);
			ProcessFeatures();

			EditorUtility.DisplayProgressBar(title, "Saving", 1f);
			AssetDatabase.RemoveUnusedAssetBundleNames();
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			EditorUtility.ClearProgressBar();
		}

		private static void ProcessCore()
		{
			EditorUtils.PreparePath(Constants.CoreAssetPath);
			EditorUtils.PreparePath(Constants.StatesAssetPath);
			EditorUtils.PreparePath(Constants.EnvironmentsAssetPath);

			AssetImporter importer = AssetImporter.GetAtPath(Constants.CoreAssetPath);
			if (importer != null)
				importer.SetAssetBundleNameAndVariant(Constants.CoreBundle, string.Empty);

			Configuration config = AssetDatabase.LoadAssetAtPath<Configuration>(Constants.GetConfigurationAssetPath());
			if (config == null)
			{
				const string envDefaultName = "Default";
				CreateEnvironmentAsset(envDefaultName);

				const string stateDefaultName = "Start";
				CreateStateAsset(stateDefaultName);

				config = ScriptableObject.CreateInstance<Configuration>();
				config.name = Constants.GetConfigurationAssetName();
				config.Environment = Constants.EnvironmentsAssetPath + Constants.DefaultSeparator + envDefaultName +
				                     Constants.AssetExtension;
				config.Start = Constants.StatesAssetPath + Constants.DefaultSeparator + stateDefaultName +
				               Constants.AssetExtension;
				AssetDatabase.CreateAsset(config, Constants.GetConfigurationAssetPath());
			}
		}

		private static void ProcessScenes()
		{
			EditorUtils.PreparePath(Constants.ScenesAssetPath);

			List<EditorBuildSettingsScene> baseScenes = EditorBuildSettings.scenes.ToList();
			List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();
			foreach (string guid in AssetDatabase.FindAssets("t:Scene", new[] {Constants.ScenesAssetPath}))
			{
				string assetPath = AssetDatabase.GUIDToAssetPath(guid);
				if (AssetBundles.Simulate)
					scenes.Add(new EditorBuildSettingsScene(assetPath, true));

				int index = baseScenes.FindIndex(current => current.path == assetPath);
				if (index >= 0)
					baseScenes.RemoveAt(index);

				AssetImporter importer = AssetImporter.GetAtPath(assetPath);
				if (importer != null)
					importer.SetAssetBundleNameAndVariant(Constants.GetSceneBundleFromPath(assetPath), string.Empty);
			}

			baseScenes.AddRange(scenes);
			for (int i = baseScenes.Count - 1; i >= 0; i--)
			{
				string path = baseScenes[i].path;
				if (!AssetBundles.Simulate && path.Contains(Constants.ScenesAssetPath) ||
				    AssetDatabase.LoadMainAssetAtPath(path) == null)
				{
					baseScenes.RemoveAt(i);
				}
			}

			EditorBuildSettings.scenes = baseScenes.ToArray();
		}

		private static void ProcessFeatures()
		{
			EditorUtils.PreparePath(Constants.FeatureAssetPath);

			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (Type type in assembly.GetTypes())
				{
					if (type.GetCustomAttributes(typeof(FeatureAttribute), true).Length > 0)
						SyncFeature(type);
				}
			}
		}

		private static void SyncFeature(Type type)
		{
			string path = Constants.FeatureAssetPath + "/" + type.Name;
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

					Logger.Info("Created feature: " + assetName);
				}
			}

			AssetImporter importer = AssetImporter.GetAtPath(path);
			if (importer != null)
			{
				string bundleName = type.Name.ToLower();
				importer.SetAssetBundleNameAndVariant(string.Format(Constants.FeatureBundle, bundleName),
					string.Empty);
			}
		}
	}

	public class CreateAssetWindow : EditorWindow
	{
		public Action<string> OnCreate;

		private string _name = string.Empty;

		public CreateAssetWindow()
		{
			maxSize = minSize = new Vector2(350, 60);
		}

		public void Setup(string windowTitle)
		{
			titleContent = new GUIContent(windowTitle);
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