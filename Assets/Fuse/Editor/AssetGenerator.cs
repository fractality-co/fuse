using System;
using System.Reflection;
using Fuse.Core;
using Fuse.Implementation;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Fuse.Editor
{
	/// <summary>
	/// Handles the generation and post-processing of assets for <see cref="Fuse"/>.
	/// </summary>
	public class AssetGenerator : AssetPostprocessor
	{
		private const string CoreAssetPath = "Assets/Bundles/Core";
		private const string StatesAssetPath = CoreAssetPath + "/States";
		private const string CoreBundleName = "imvc.core";
		private const string ImplementationAssetPath = "Assets/Bundles/Implementations";
		private const string ImplementationBundleName = "imvc.{0}.{1}";

#if UNITY_EDITOR
		[MenuItem("Window/Fuse/Configure %&c")]
		[MenuItem("Assets/Fuse/Configure")]
		private static void EditConfiguration()
		{
			Selection.activeObject = AssetDatabase.LoadAssetAtPath<ScriptableObject>(GetConfigurationAssetPath());
		}
#endif

		[MenuItem("Window/Fuse/New/State %&s")]
		[MenuItem("Assets/Fuse/New/State")]
		public static void ShowCreateStateWindow()
		{
			CreateStateWindow window = EditorWindow.GetWindow<CreateStateWindow>();
			window.OnCreate += CreateStateAsset;
		}

		private static void CreateStateAsset(string name)
		{
			string path = StatesAssetPath + "/" + name + ".asset";

			State state = ScriptableObject.CreateInstance<State>();
			state.name = name;

			AssetDatabase.CreateAsset(state, path);
			Selection.activeObject = state;

			FuseLogger.Info("Created new " + typeof(State).Name + ": " + state.name);
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
			EditorUtils.PreparePath(CoreAssetPath);
			EditorUtils.PreparePath(StatesAssetPath);

			AssetImporter importer = AssetImporter.GetAtPath(CoreAssetPath);
			if (importer != null)
				importer.SetAssetBundleNameAndVariant(CoreBundleName, string.Empty);

			ScriptableObject asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(GetConfigurationAssetPath());
			if (asset == null)
			{
				asset = ScriptableObject.CreateInstance<Configuration>();
				asset.name = GetConfigurationAssetName();
				AssetDatabase.CreateAsset(asset, GetConfigurationAssetPath());
			}
		}

		private static void ProcessImplementations()
		{
			EditorUtils.PreparePath(ImplementationAssetPath);

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
			string path = ImplementationAssetPath + "/" + type.Name;
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
					FuseLogger.Info("Created implementation: " + assetName + " [" + implementation + "]");
				}
			}

			AssetImporter importer = AssetImporter.GetAtPath(path);
			if (importer != null)
			{
				string bundleType = implementation.ToString().ToLower();
				string bundleName = type.Name.ToLower().Replace(implementation.ToString().ToLower(), string.Empty);
				importer.SetAssetBundleNameAndVariant(string.Format(ImplementationBundleName, bundleType, bundleName),
					string.Empty);
			}
		}

		private static string GetConfigurationAssetName()
		{
			return typeof(Configuration).Name;
		}

		private static string GetConfigurationAssetPath()
		{
			return CoreAssetPath + "/" + GetConfigurationAssetName() + ".asset";
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