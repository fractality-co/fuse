using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Fuse.Editor
{
    [InitializeOnLoad]
    public static class EditorAssetResolver
    {
        static EditorAssetResolver()
        {
            Bundles.EditorResolver = new EditorResolver();
        }
    }

    public class EditorResolver : IEditorResolver
    {
        public string FindEditorAssetsBundle(string path)
        {
            foreach (var bundle in AssetDatabase.GetAllAssetBundleNames())
            {
                if (AssetDatabase.GetAssetPathsFromAssetBundle(bundle).Any(assetPath => assetPath == path))
                    return bundle;
            }

            return string.Empty;
        }

        public T LoadEditorAsset<T>(string path) where T : Object
        {
            // find all assets that are within our path, and match our type
            foreach (var guid in AssetDatabase.FindAssets($"t:{typeof(T).Name}"))
            {
                // find asset that matches our passed path
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (assetPath.Contains(path))
                {
                    // make sure it is the correct type, even if it matches our path
                    var loaded = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                    if (loaded != null) return loaded;
                }
            }

            return null;
        }

        public List<T> LoadEditorAssets<T>(string bundle) where T : Object
        {
            var result = new List<T>();

            // find all assets that are within an asset bundle, and match our type
            var bundleAssetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(bundle);
            foreach (var guid in AssetDatabase.FindAssets($"t:{typeof(T).Name}"))
            {
                // find asset that matches our passed path
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (bundleAssetPaths.Contains(assetPath))
                {
                    // make sure it is the correct type, even if it matches our path
                    var loaded = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                    if (loaded != null)
                        result.Add(loaded);
                }
            }

            return result;
        }

        public List<Object> LoadEditorAssets(string bundle, Type type)
        {
            var result = new List<Object>();

            // find all assets that are within an asset bundle, and match our type
            var bundleAssetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(bundle);
            foreach (var guid in AssetDatabase.FindAssets($"t:{type.Name}"))
            {
                // find asset that matches our passed path
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (bundleAssetPaths.Contains(assetPath))
                {
                    // make sure it is the correct type, even if it matches our path
                    var loaded = AssetDatabase.LoadAssetAtPath(assetPath, type);
                    if (loaded != null)
                        result.Add(loaded);
                }
            }

            return result;
        }
    }
}