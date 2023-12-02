

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;

namespace Fuse.Editor
{
	/// <summary>
	/// Handles the pipeline for assets.
	/// </summary>
	[SuppressMessage("ReSharper", "AccessToStaticMemberViaDerivedType")]
	public class AssetProcessor : AssetPostprocessor
	{
		private static bool ShownWelcome { get => EditorPrefs.GetBool("fuse.welcome"); set => EditorPrefs.SetBool("fuse.welcome", value); }
		private static bool Manual { get => EditorPrefs.GetBool("fuse.integration.manual"); set => EditorPrefs.SetBool("fuse.integration.manual", value); }

		private static bool Integrated => AssetDatabase.IsValidFolder(Constants.AssetsBakedEditorPath);

		private const string EnvironmentDefaultName = "Default";
		private const string StartStateName = "Start";
		private const string ContentStateName = "Default";
		private const string InstallSceneName = "Fuse";

		/// <summary>
		/// Builds asset bundles for the active build target and environment.
		/// </summary>
		public static void Build()
		{
			var configuration =
				AssetUtility.FetchByPath<Configuration>(Constants.GetConfigurationAssetPath());
			var environment = AssetUtility.FetchByPath<Environment>(configuration.Environment);
			if (environment == null)
			{
				if (!InternalEditorUtility.inBatchMode)
					throw new Exception("Unable to integrate asset bundles; no environment defined");

				Logger.Error("Unable to integrate assets, no environment assigned in configuration.");
				EditorApplication.Exit(1);
				return;
			}

			Build(PlatformUtility.GetPlatform().Item1, environment);
		}

		/// <summary>
		/// Builds asset bundles for the passed build target and environment.
		/// </summary>
		public static AssetBundleManifest Build(BuildTarget buildTarget, Environment environment)
		{
			var outputPath = GetOutputPath(buildTarget);
			if (Directory.Exists(outputPath))
				Directory.Delete(outputPath, true);

			if (!Directory.Exists(Constants.EditorBundlePath))
				Directory.CreateDirectory(Constants.EditorBundlePath);

			Directory.CreateDirectory(outputPath);

			var manifest = BuildPipeline.BuildAssetBundles(outputPath,
				(UnityEditor.BuildAssetBundleOptions)environment.assets, buildTarget);
			if (manifest == null)
			{
				if (!InternalEditorUtility.inBatchMode)
					throw new Exception("Unable to build asset bundles for: " + buildTarget + "!");

				Logger.Error("Unable to build asset bundles for: " + buildTarget + "!");
				EditorApplication.Exit(1);
				return null;
			}

			var buildDirectory = new DirectoryInfo(outputPath);
			var bundleNames = AssetDatabase.GetAllAssetBundleNames();
			var infos = buildDirectory.GetFiles();

			foreach (var fileInfo in infos)
				if (((IList)bundleNames).Contains(fileInfo.Name))
					File.Move(fileInfo.FullName, fileInfo.FullName + Constants.BundleExtension);

			Logger.Info("Built assets for: " + buildTarget.GetPlatformName());
			return manifest;
		}

		[MenuItem("FUSE/Advanced/Integrate")]
		private static void IntegrateViaMenu()
		{
			if (Manual)
			{
				Debug.LogWarning("Aborting integration; an already active integration exists. Cleanup via the menu (Fuse/Advanced/Cleanup) then try again.");
				return;
			}

			if (EditorUtility.DisplayDialog("FUSE - Manual Integration",
				"Would you like FUSE to integrate the assets into the project manually? Once integrated it will be placed into StreamingAssets; this will persist until you invoke Cleanup in Fuse/Advanced.",
				"Yes", "No"))
			{
				Integrate();
				Manual = Integrated;
			}
		}

		/// <summary>
		/// Integrates assets into the build for active platform; will build if none exist.
		/// </summary>
		public static void Integrate()
		{
			var configuration =
				AssetUtility.FetchByPath<Configuration>(Constants.GetConfigurationAssetPath());
			var environment = AssetUtility.FetchByPath<Environment>(configuration.Environment);

			if (environment == null)
			{
				if (!InternalEditorUtility.inBatchMode)
					throw new Exception("Unable to integrate asset bundles; no environment defined");

				Logger.Error("Unable to integrate assets, no environment assigned in configuration.");
				EditorApplication.Exit(1);
				return;
			}

			Integrate(PlatformUtility.GetPlatform().Item1, environment);
		}


		/// <summary>
		/// Integrates assets into the build; will build if none exist.
		/// </summary>
		public static void Integrate(BuildTarget buildTarget, Environment environment)
		{
			if (Integrated)
				return;

			Build();
			Cleanup();
			PathUtility.PreparePath(Constants.AssetsBakedEditorPath);

			var outputPath = GetOutputPath(buildTarget);
			switch (environment.loading)
			{
				case LoadMethod.Baked:
					// integrate everything for corresponding platform
					PathUtility.DirectoryCopy(outputPath, GetAssetIntegration(), true);

					// delete platform specific root bundle, as we copy those over for all platform later
					File.Delete(GetAssetIntegration() + Path.DirectorySeparatorChar + buildTarget.GetPlatformName().ToLower());
					File.Delete(GetAssetIntegration() + Path.DirectorySeparatorChar + buildTarget.GetPlatformName().ToLower() + ".manifest");
					break;
				case LoadMethod.Online:
					// we only integrate core bundle, this is required to know where things are hosted in the first place
					var sourceCorePath = outputPath + Path.DirectorySeparatorChar + Constants.CoreBundleFile;
					var destCorePath =
						GetAssetIntegration() + Path.DirectorySeparatorChar + Constants.CoreBundleFile;
					File.Copy(sourceCorePath, destCorePath, true);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			// for proper loading on platforms (e.g. Android), we need the root bundle / manifest integrated
			var sourceRootBundlePath = outputPath + Path.DirectorySeparatorChar + buildTarget.GetPlatformName().ToLower();
			var destRootBundlePath = GetAssetIntegration() + Path.DirectorySeparatorChar + "Bundles";
			var sourceRootBundleManifestPath = outputPath + Path.DirectorySeparatorChar + buildTarget.GetPlatformName().ToLower() + ".manifest";
			var destRootBundleManifestPath = GetAssetIntegration() + Path.DirectorySeparatorChar + "Bundles.manifest";
			File.Copy(sourceRootBundlePath, destRootBundlePath);
			File.Copy(sourceRootBundleManifestPath, destRootBundleManifestPath);

			AssetDatabase.Refresh();
			ProcessScenes();
		}

		[MenuItem("FUSE/Advanced/Cleanup")]
		private static void CleanupViaMenu()
		{
			Manual = false;
			Cleanup();
		}

		/// <summary>
		/// Removes any built asset bundles that are integrated.
		/// </summary>
		public static void Cleanup()
		{
			if (Manual)
			{
				Debug.LogWarning("Unable to cleanup automatically until manually invoked via menu (Fuse/Advanced/Cleanup())");
				return;
			}

			var rootPath = GetRootAssetIntegration();
			var integrationPath = GetAssetIntegration();

			var dirInfo = new DirectoryInfo(rootPath);
			if (dirInfo.Exists && dirInfo.GetFiles().Length == 1)
			{
				dirInfo.Delete(true);

				var metaFile = rootPath + ".meta";
				if (File.Exists(metaFile))
					File.Delete(metaFile);
			}
			else if (Directory.Exists(integrationPath))
			{
				Directory.Delete(integrationPath, true);

				var metaFile = integrationPath + ".meta";
				if (File.Exists(metaFile))
					File.Delete(metaFile);
			}

			AssetDatabase.Refresh();
			ProcessScenes();
		}

		/// <summary>
		/// Returns the location of where assets for Fuse will live when baked.
		/// </summary>
		/// <returns></returns>
		public static string GetAssetIntegration() { return PathUtility.SystemPath(Constants.AssetsBakedEditorPath); }

		private static string GetRootAssetIntegration() { return PathUtility.SystemPath(Constants.AssetsBakedRootEditorPath); }

		/// <summary>
		/// Returns the location of where assets are built to.
		/// </summary>
		public static string GetOutputPath(BuildTarget buildTarget)
		{
			var configuration =
				AssetUtility.FetchByPath<Configuration>(Constants.GetConfigurationAssetPath());
			var environment = AssetUtility.FetchByPath<Environment>(configuration.Environment);
			if (environment == null)
				return string.Empty;

			return (Constants.EditorBundlePath + Constants.DefaultSeparator +
			        environment.name + Constants.DefaultSeparator +
			        buildTarget.GetPlatformName()
				        .Replace(Constants.DefaultSeparator, Path.DirectorySeparatorChar));
		}

		/// <summary>
		/// Generates new state date.
		/// </summary>
		/// <param name="name"></param>
		public static State CreateState(string name)
		{
			var path = Constants.StatesAssetPath + "/" + name + ".asset";

			var state = ScriptableObject.CreateInstance<State>();
			state.name = name;

			AssetDatabase.CreateAsset(state, path);
			Selection.activeObject = state;
			return state;
		}

		/// <summary>
		/// Generates new environment data.
		/// </summary>
		/// <param name="name"></param>
		public static void CreateEnvironment(string name)
		{
			var path = Constants.EnvironmentsAssetPath + "/" + name + ".asset";

			var environment = ScriptableObject.CreateInstance<Environment>();
			environment.name = name;

			AssetDatabase.CreateAsset(environment, path);
			Selection.activeObject = environment;
		}

		public static void CreateContent(string name)
		{
			var path = Constants.ContentAssetPath + "/" + name;
			PathUtility.PreparePath(path);
			CreateAsset(typeof(Content), path, name);
		}

		public static ScriptableObject CreateAsset(Type type, string basePath, string name)
		{
			var path = basePath + "/" + name + ".asset";

			var asset = ScriptableObject.CreateInstance(type);
			asset.name = name;
			AssetDatabase.CreateAsset(asset, path);
			return asset;
		}

		/// <summary>
		/// Generates a new scene that is Fuse integrated.
		/// </summary>
		[SuppressMessage("ReSharper", "AccessToStaticMemberViaDerivedType")]
		public static void CreateScene(string name)
		{
			var path = Constants.ScenesAssetPath + "/" + name + ".unity";
			var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
			PopulateScene();
			EditorSceneManager.SaveScene(scene, path);
			EditorSceneManager.UnloadSceneAsync(scene);
		}

		private static void PopulateScene()
		{
			var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
			cube.transform.localEulerAngles = new Vector3(35, 35, 35);

			var camera = new GameObject("Camera").AddComponent<Camera>();
			camera.backgroundColor = Color.black;
			camera.clearFlags = CameraClearFlags.Color;
			camera.transform.localPosition = Vector3.back * 5;

			var light = new GameObject("Light").AddComponent<Light>();
			light.type = LightType.Directional;
		}

		private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
			string[] movedAssets,
			string[] movedFromAssetPaths)
		{
			ProcessCore();
			ProcessScenes();
			ProcessContent();

			AssetDatabase.RemoveUnusedAssetBundleNames();
		}

		private static void ProcessCore()
		{
			// only install/modify core if we aren't automated
			if (!InternalEditorUtility.inBatchMode)
			{
				if (!File.Exists(Constants.GetConfigurationAssetPath()))
				{
					ShownWelcome = false;

					PathUtility.PreparePath(Constants.CoreAssetPath);
					PathUtility.PreparePath(Constants.StatesAssetPath);
					PathUtility.PreparePath(Constants.EnvironmentsAssetPath);
					PathUtility.PreparePath(Constants.ScenesAssetPath);
					PathUtility.PreparePath(Constants.ContentAssetPath);

					CreateEnvironment(EnvironmentDefaultName);
					CreateContent(ContentStateName);
					CreateScene(StartStateName);

					var state = CreateState(StartStateName);
					state.Scenes = new[] { Constants.ScenesAssetPath + "/" + StartStateName + Constants.SceneExtension };
					EditorUtility.SetDirty(state);

					var config = ScriptableObject.CreateInstance<Configuration>();
					config.name = nameof(Configuration);
					config.Environment = Constants.EnvironmentsAssetPath + Constants.DefaultSeparator +
					                     EnvironmentDefaultName + Constants.AssetExtension;
					config.Start = Constants.StatesAssetPath + Constants.DefaultSeparator + StartStateName +
					               Constants.AssetExtension;
					config.Content = new[]
					{
						Constants.ContentAssetPath + Constants.DefaultSeparator + ContentStateName +
						Constants.DefaultSeparator + ContentStateName + Constants.AssetExtension
					};
					config.Loader =
						AssetUtility.FetchByPath<Loader>(PathUtility.GetDefaultVisualizerPath() + ".prefab");
					AssetDatabase.CreateAsset(config, Constants.GetConfigurationAssetPath());

					AssetDatabase.SaveAssets();
					AssetDatabase.Refresh();
				}
				else if (!ShownWelcome)
				{
					var logo = AssetUtility.FetchByPath<Sprite>(string.Format(PathUtility.IconPath, "FuseSplash"));
					if (logo == null)
						return; // AssetDatabase still needs time

					PathUtility.PreparePath("Assets/Scenes");
					var scenePath = "Assets/Scenes/" + InstallSceneName + ".unity";

					var scenes = EditorBuildSettings.scenes.ToList();
					scenes.Insert(0, new EditorBuildSettingsScene(scenePath, true));
					EditorBuildSettings.scenes = scenes.ToArray();

					Popup.Display<WelcomePopup>();
					ShownWelcome = true;
				}
			}

			var importer = AssetImporter.GetAtPath(Constants.CoreAssetPath);
			if (importer != null && string.IsNullOrEmpty(importer.assetBundleName))
				importer.SetAssetBundleNameAndVariant(Constants.CoreBundle, string.Empty);
		}

		private static void ProcessScenes()
		{
			if (!AssetDatabase.IsValidFolder(Constants.ScenesAssetPath))
			{
				PathUtility.PreparePath(Constants.ScenesAssetPath);
				return;
			}

			var baseScenes = EditorBuildSettings.scenes.ToList();
			for (var i = baseScenes.Count - 1; i >= 0; i--)
			{
				var scene = baseScenes[i];
				if (!File.Exists(scene.path))
					baseScenes.RemoveAt(i);
			}

			var scenes = new List<EditorBuildSettingsScene>();
			foreach (var guid in AssetDatabase.FindAssets("t:Scene", new[] { Constants.ScenesAssetPath }))
			{
				var assetPath = AssetDatabase.GUIDToAssetPath(guid);
				scenes.Add(new EditorBuildSettingsScene(assetPath, !Integrated));

				var index = baseScenes.FindIndex(current => current.path == assetPath);
				if (index >= 0)
					baseScenes.RemoveAt(index);

				var importer = AssetImporter.GetAtPath(assetPath);
				if (importer != null)
					importer.SetAssetBundleNameAndVariant(Constants.GetSceneBundleFromPath(assetPath), string.Empty);
			}

			baseScenes.AddRange(scenes);
			EditorBuildSettings.scenes = baseScenes.ToArray();
		}

		private static void ProcessContent()
		{
			PathUtility.PreparePath(Constants.ContentAssetPath);
			var rootDirectory = new DirectoryInfo(Constants.ContentAssetPath);
			foreach (var subDirectory in rootDirectory.GetDirectories())
			{
				if (subDirectory.GetFiles().Length == 0 && subDirectory.GetDirectories().Length == 0)
				{
					subDirectory.Delete(true);
					continue;
				}

				var contentName = subDirectory.Name.ToLower().Replace(" ", string.Empty) + "_content";
				var hasConfig = AssetDatabase.FindAssets(subDirectory.Name + " t:" + nameof(Content)).Length != 0;
				if (!hasConfig)
					contentName = string.Empty;

				var importer = AssetImporter.GetAtPath(Constants.ContentAssetPath + "/" + subDirectory.Name);
				if (importer != null)
					importer.SetAssetBundleNameAndVariant(contentName, string.Empty);
			}
		}
	}
}