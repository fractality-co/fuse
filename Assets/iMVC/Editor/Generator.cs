using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace iMVC.Editor
{
    public class Generator : AssetPostprocessor
    {
        [MenuItem("Window/iMVC/Configure")]
        [MenuItem("Assets/iMVC/Configure")]
        public static void SelectConfiguration()
        {
            string assetName = typeof(Configuration).Name;
            string[] assets = AssetDatabase.FindAssets("t:" + assetName);
            if (assets.Length > 0)
                Selection.activeObject =
                    AssetDatabase.LoadAssetAtPath<Configuration>("Assets/Resources/iMVC.asset");
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
                string path = "Assets/Resources";
                PreparePath(path);

                ScriptableObject configuration = ScriptableObject.CreateInstance<Configuration>();
                configuration.name = "iMVC";
                AssetDatabase.CreateAsset(configuration, path + "/" + configuration.name + ".asset");

                Debug.Log("Initialized iMVC Configuration ...");
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

            string path = "Assets/Resources/" + implementation + "/" + type.Name;
            IEnumerable<string> assetNames = implementation.GetAssets(type);
            foreach (string assetName in assetNames)
            {
                string assetPath = path + "/" + assetName + ".asset";
                ScriptableObject asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
                if (asset == null)
                {
                    PreparePath(path);

                    asset = ScriptableObject.CreateInstance(type);
                    asset.name = assetName;
                    AssetDatabase.CreateAsset(asset, assetPath);

                    Debug.Log("Created implementation: " + assetName + " [" + implementation + "]");
                }
            }

            AssetDatabase.SaveAssets();
        }

        public static void PreparePath(string path)
        {
            string[] directories = path.Split('/');
            string parent = string.Empty;
            for (int i = 0; i <= directories.Length - 1; i++)
            {
                string current = directories[i];
                string next = parent == string.Empty ? current : parent + "/" + current;
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(parent, current);
                parent = next;
            }
        }
    }
}