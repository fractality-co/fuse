/* Copyright (2021) Fractality LLC - All Rights Reserved
 * 
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace Fuse
{
    /// <summary>
    /// Manages everything related to assets.
    /// Access this to load in an asset that is weakly referenced or to manage your own custom bundles & assets.
    /// </summary>
    [Document("Assets",
        "Bundles is a static utility class that manages everything related to dynamic assets (content, scenes) in Fuse. " +
        "By default, you should not need to interact with this, as Fuse will handle all bundle loading and asset resolution for you."
        + "\n\nAccess this to load in an asset that is weakly referenced or to manage your own custom bundles & assets.")]
    public static class Bundles
    {
        /// <summary>
        /// During run-time, we inject the editor side resolver into this so while in the editor it can resolve the assets.
        /// </summary>
        public static IEditorResolver EditorResolver;

        private const string CrcCacheSyntax = "CRC: ";
        private const string CrcCacheKey = "{0}.crc";

        private static readonly Regex CrcRegex = new Regex(CrcCacheSyntax + "[0-9]*");
        private static readonly Dictionary<string, AssetBundle> LoadedBundles = new Dictionary<string, AssetBundle>();

        public static IEnumerator LoadBundle(Environment environment, string subPath, Action<string> onComplete = null,
            Action<float> onProgress = null, Action<string> onError = null)
        {
            switch (environment.loading)
            {
                case LoadMethod.Baked:
                    yield return LoadLocalBundle
                    (
                        environment.GetPath(subPath), onComplete, onProgress, onError
                    );
                    break;
                case LoadMethod.Online:
                    yield return LoadRemoteBundle
                    (
                        environment.GetFileUri(subPath, true), onComplete, onProgress, onError
                    );
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static IEnumerator LoadRemoteBundle(Uri uri, Action<string> onComplete = null,
            Action<float> onProgress = null, Action<string> onError = null)
        {
            var bundleName = Constants.GetFileNameFromPath(uri.AbsoluteUri, "unity3d");
            if (Application.isEditor)
            {
                onComplete?.Invoke(bundleName);
                yield break;
            }

            if (LoadedBundles.TryGetValue(bundleName, out var bundle))
            {
                onComplete?.Invoke(bundle.name);
                yield break;
            }

            // when going to download a bundle, if it is on a remote source, then extract the CRC hash for caching
            int crc = -1;
            UnityWebRequest request;
            if (uri.AbsoluteUri.Contains("http"))
            {
                string manifestUri = uri.AbsoluteUri.Replace("unity3d", "manifest");
                UnityWebRequest manifestRequest = UnityWebRequest.Get(manifestUri);
                UnityWebRequestAsyncOperation manifestSendRequest = manifestRequest.SendWebRequest();
                while (!manifestSendRequest.isDone)
                    yield return null;

                // attempt to get text from the manifest file, but if it failed let's check if we had a cached one already
                string crcCacheKey = string.Format(CrcCacheKey, bundleName);
                string manifest = manifestRequest.downloadHandler.text;
                if (!string.IsNullOrEmpty(manifestRequest.error) || string.IsNullOrEmpty(manifest))
                {
                    if (PlayerPrefs.HasKey(crcCacheKey))
                    {
                        crc = PlayerPrefs.GetInt(crcCacheKey);
                        Logger.Info($"Falling back to previously cached CRC hash [{crc}] for bundle {bundleName}");
                    }
                    else
                    {
                        onError?.Invoke("Error when attempting to find manifest for CRC hash (cache control): " +
                                        manifestUri);
                        yield break;
                    }
                }

                // let's extract the CRC value from the file and discard the rest, as that's all we care about
                // ReSharper disable once AssignNullToNotNullAttribute
                string crcExtraction = CrcRegex.Match(manifest).Value;
                crcExtraction = crcExtraction.Replace(CrcCacheSyntax, string.Empty).Trim();
                if (!int.TryParse(crcExtraction, out crc))
                {
                    onError?.Invoke("Unable to extract CRC hash from manifest due to unknown parsing error!");
                    yield break;
                }

                PlayerPrefs.SetInt(crcCacheKey, crc);
                request = UnityWebRequestAssetBundle.GetAssetBundle(uri.AbsoluteUri, (uint) crc);
            }
            else
                request = UnityWebRequestAssetBundle.GetAssetBundle(uri.AbsoluteUri, 0);

            UnityWebRequestAsyncOperation sendRequest = request.SendWebRequest();
            while (!sendRequest.isDone)
            {
                yield return null;
                onProgress?.Invoke(sendRequest.progress);
            }

            onProgress?.Invoke(1f);

            var assetBundle = ((DownloadHandlerAssetBundle) request.downloadHandler).assetBundle;
            if (!string.IsNullOrEmpty(request.error) || assetBundle == null)
            {
                onError?.Invoke("Error when loading bundle from web at: " + uri.AbsoluteUri + " [" + crc + "]\n" +
                                request.error);
                yield break;
            }

            LoadedBundles.Add(assetBundle.name, assetBundle);
            onComplete?.Invoke(assetBundle.name);
        }

        public static IEnumerator LoadLocalBundle(string path, Action<string> onComplete = null,
            Action<float> onProgress = null,
            Action<string> onError = null)
        {
            var bundleName = Constants.GetFileNameFromPath(path, Constants.BundleExtension);
            if (Application.isEditor)
            {
                onComplete?.Invoke(bundleName);
                yield break;
            }

            if (LoadedBundles.TryGetValue(bundleName, out var bundle))
            {
                onComplete?.Invoke(bundle.name);
                yield break;
            }

            var request = AssetBundle.LoadFromFileAsync(path);
            while (!request.isDone)
            {
                yield return request;
                onProgress?.Invoke(request.progress);
            }

            onProgress?.Invoke(1f);

            var assetBundle = request.assetBundle;
            if (assetBundle == null)
            {
                onError?.Invoke("Attempted to load a AssetBundle, but it came back null!");
                yield break;
            }

            LoadedBundles.Add(assetBundle.name, assetBundle);
            onComplete?.Invoke(assetBundle.name);
        }

        public static bool UnloadBundle(string bundleName, bool unloadActiveAssets)
        {
            if (Application.isEditor)
            {
                Clean();
                return true;
            }

            AssetBundle loadedBundle = null;
            string bundleToRemove = null;
            foreach (var bundle in LoadedBundles)
                if (bundle.Value.name == bundleName)
                {
                    loadedBundle = bundle.Value;
                    bundleToRemove = bundle.Key;
                }

            if (loadedBundle == null)
                return false;

            loadedBundle.Unload(unloadActiveAssets);
            Clean();
            LoadedBundles.Remove(bundleToRemove);
            return true;
        }

        public static void UnloadAllBundles(bool unloadActiveAssets)
        {
            AssetBundle.UnloadAllAssetBundles(unloadActiveAssets);
            Clean();
            LoadedBundles.Clear();
        }

        public static T LoadAsset<T>(string path) where T : Object
        {
            if (Application.isEditor)
                return EditorResolver.LoadEditorAsset<T>(path);

            var bundle = GetBundleWithAsset(path);
            if (string.IsNullOrEmpty(bundle))
            {
                Debug.LogError($"Error trying to resolve bundle for path: {path}");
                return null;
            }

            var assetBundle = AssetBundle.GetAllLoadedAssetBundles().First(loaded => loaded.name == bundle);
            return assetBundle.LoadAsset<T>(path);
        }

        public static IEnumerator LoadAsset<T>(string path, Action<T> onComplete, Action<float> onProgress = null,
            Action<string> onError = null) where T : Object
        {
            if (Application.isEditor)
            {
                onComplete(EditorResolver.LoadEditorAsset<T>(path));
                yield break;
            }

            var bundle = GetBundleWithAsset(path);
            if (string.IsNullOrEmpty(bundle))
            {
                onError?.Invoke("Unable to find a loaded Asset Bundle with that name.");
                yield break;
            }

            var assetBundle = AssetBundle.GetAllLoadedAssetBundles().First(loaded => loaded.name == bundle);
            var assetBundleRequest = assetBundle.LoadAssetAsync<T>(path);
            while (!assetBundleRequest.isDone)
            {
                yield return null;
                onProgress?.Invoke(assetBundleRequest.progress);
            }

            onProgress?.Invoke(1f);

            var asset = assetBundleRequest.asset as T;
            if (asset == default(T))
                onError?.Invoke("Unable to retrieve asset by path (" + path + ") as type " + typeof(T));
            else
                onComplete(asset);
        }

        public static IEnumerator LoadAssets(string bundle, Type type, Action<List<Object>> onComplete,
            Action<float> onProgress = null,
            Action<string> onError = null)
        {
            if (Application.isEditor)
            {
                onComplete(EditorResolver.LoadEditorAssets(bundle, type));
                yield break;
            }

            var assetBundle = AssetBundle.GetAllLoadedAssetBundles().First(loaded => loaded.name == bundle);
            if (assetBundle == null)
            {
                onError?.Invoke("Unable to find a loaded Asset Bundle with that name.");
                yield break;
            }

            var assetBundleRequest = assetBundle.LoadAllAssetsAsync(type);
            while (!assetBundleRequest.isDone)
            {
                yield return null;
                onProgress?.Invoke(assetBundleRequest.progress);
            }

            onProgress?.Invoke(1f);

            onComplete(assetBundleRequest.allAssets.ToList());
        }

        public static IEnumerator LoadAssets<T>(string bundle, Action<List<T>> onComplete,
            Action<float> onProgress = null,
            Action<string> onError = null) where T : Object
        {
            if (Application.isEditor)
            {
                onComplete(EditorResolver.LoadEditorAssets<T>(bundle));
                yield break;
            }

            var assetBundle = AssetBundle.GetAllLoadedAssetBundles().First(loaded => loaded.name == bundle);
            if (assetBundle == null)
            {
                onError?.Invoke("Unable to find a loaded Asset Bundle with that name.");
                yield break;
            }

            var assetBundleRequest = assetBundle.LoadAllAssetsAsync<T>();
            while (!assetBundleRequest.isDone)
            {
                yield return null;
                onProgress?.Invoke(assetBundleRequest.progress);
            }

            onComplete(assetBundleRequest.allAssets.Cast<T>().ToList());
        }

        private static string GetBundleWithAsset(string path)
        {
            if (Application.isEditor)
                return EditorResolver.FindEditorAssetsBundle(path);

            foreach (var bundle in AssetBundle.GetAllLoadedAssetBundles())
                if (bundle.Contains(path))
                    return bundle.name;

            return string.Empty;
        }

        private static void Clean()
        {
            Resources.UnloadUnusedAssets();
            GC.Collect();
        }
    }

    public interface IEditorResolver
    {
        string FindEditorAssetsBundle(string path);
        List<T> LoadEditorAssets<T>(string bundle) where T : Object;
        List<Object> LoadEditorAssets(string bundle, Type type);
        T LoadEditorAsset<T>(string path) where T : Object;
    }
}