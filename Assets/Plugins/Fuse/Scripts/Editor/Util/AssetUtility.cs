using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Fuse.Editor
{
    /// <summary>
    /// Helper class for asset fetching and caching layer.
    /// </summary>
    public static class AssetUtility
    {
        private static readonly Dictionary<string, Object> Cache = new Dictionary<string, Object>();

        public static T FetchByGuid<T>(string guid) where T : Object
        {
            return FetchByPath<T>(AssetDatabase.GUIDToAssetPath(guid));
        }

        public static T FetchByPath<T>(string path) where T : Object
        {
            if (Cache.ContainsKey(path) && Cache[path] == null)
            {
                Cache.Remove(path);
                return default;
            }

            if (!Cache.ContainsKey(path))
                Cache[path] = AssetDatabase.LoadAssetAtPath<T>(path);

            var value = Cache[path] as T;
            if (value != null)
                return value;

            value = AssetDatabase.LoadAssetAtPath<T>(path);
            if (value == default)
                return default;

            Cache[path] = value;
            return value;
        }
    }
}