using System;
using System.Reflection;
using Fuse.Core;
using Fuse.Implementation;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using Logger = Fuse.Core.Logger;

namespace Fuse.Editor
{
	/// <summary>
	/// Handles the generation and post-processing of assets for <see cref="Executor"/>.
	/// </summary>
	public class AssetGenerator : AssetPostprocessor
	{
#if UNITY_EDITOR
		[MenuItem("Window/Fuse/Configure %&c")]
		[MenuItem("Assets/Fuse/Configure")]
		private static void EditConfiguration()
		{
			Selection.activeObject = AssetDatabase.LoadAssetAtPath<ScriptableObject>(Constants.GetConfigurationAssetPath());
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
				string bundleName = type.Name.ToLower().Replace(implementation.ToString().ToLower(), string.Empty);
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