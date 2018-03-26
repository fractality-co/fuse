using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace Fuse.Core
{
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	public class AssetBundles
	{
		private readonly bool _simulateBundles;
		private readonly int _requestTimeout;

		public AssetBundles(bool simulateBundles, int requestTimeout = 120)
		{
			_requestTimeout = requestTimeout;
			_simulateBundles = simulateBundles;
		}

		public IEnumerator LoadBundle(Uri uri, Hash128 version, Action<string> onComplete = null,
			Action<float> onProgress = null, Action<string> onError = null)
		{
			UnityWebRequest request = UnityWebRequest.GetAssetBundle(uri.AbsolutePath, version, 0);
			request.timeout = _requestTimeout;

			UnityWebRequestAsyncOperation sendRequest = request.SendWebRequest();
			while (!sendRequest.isDone)
			{
				yield return null;

				if (onProgress != null)
					onProgress(sendRequest.progress);
			}

			if (onProgress != null)
				onProgress(1f);

			AssetBundle assetBundle = ((DownloadHandlerAssetBundle) request.downloadHandler).assetBundle;
			if (!string.IsNullOrEmpty(request.error) || assetBundle == null)
			{
				if (onError != null)
					onError("Error when loading bundle from web at: " + uri.AbsolutePath + " [" + version + "]\n" + request.error);

				yield break;
			}

			if (onComplete != null)
				onComplete(assetBundle.name);
		}

		public IEnumerator LoadBundle(string path, Action<string> onComplete = null, Action<float> onProgress = null,
			Action<string> onError = null)
		{
			AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(path);

			while (!request.isDone)
			{
				yield return request;

				if (onProgress != null)
					onProgress(request.progress);
			}

			if (onProgress != null)
				onProgress(1f);

			AssetBundle assetBundle = request.assetBundle;
			if (assetBundle == null)
			{
				if (onError != null)
					onError("Attempted to load a AssetBundle, but it came back null!");

				yield break;
			}

			if (onComplete != null)
				onComplete(assetBundle.name);
		}

		public bool UnloadBundle(string bundleName, bool unloadActiveAssets)
		{
			AssetBundle loadedBundle = AssetBundle.GetAllLoadedAssetBundles().First(bundle => bundle.name == bundleName);
			if (loadedBundle == null)
				return false;

			loadedBundle.Unload(unloadActiveAssets);
			Clean();
			return true;
		}

		public void UnloadAllBundles(bool unloadActiveAssets)
		{
			AssetBundle.UnloadAllAssetBundles(unloadActiveAssets);
			Clean();
		}

		public IEnumerator LoadAsset<T>(string path, Action<T> onComplete, Action<float> onProgress = null,
			Action<string> onError = null) where T : Object
		{
			path = path.ToLower().Trim();

#if UNITY_EDITOR
			if (_simulateBundles)
			{
				onComplete(LoadEditorAsset<T>(path));
				yield break;
			}
#endif

			string bundle = GetBundleWithAsset(path);
			if (string.IsNullOrEmpty(bundle))
			{
				if (onError != null)
					onError("Unable to find a loaded Asset Bundle with that name.");

				yield break;
			}

			AssetBundle assetBundle = AssetBundle.GetAllLoadedAssetBundles().First(loaded => loaded.name == bundle);
			AssetBundleRequest assetBundleRequest = assetBundle.LoadAssetAsync<T>(path);
			while (!assetBundleRequest.isDone)
			{
				yield return null;

				if (onProgress != null)
					onProgress(assetBundleRequest.progress);
			}

			if (onProgress != null)
				onProgress(1f);

			T asset = assetBundleRequest.asset as T;
			if (asset == default(T))
			{
				if (onError != null)
					onError("Unable to retrieve asset by path (" + path + ") as type " + typeof(T));
			}
			else
				onComplete(asset);
		}

		public IEnumerator LoadAssets<T>(string bundle, Action<List<T>> onComplete,
			Action<float> onProgress = null,
			Action<string> onError = null) where T : Object
		{
#if UNITY_EDITOR
			if (_simulateBundles)
			{
				onComplete(LoadEditorAssets<T>(bundle));
				yield break;
			}
#endif

			AssetBundle assetBundle = AssetBundle.GetAllLoadedAssetBundles().First(loaded => loaded.name == bundle);
			if (assetBundle == null)
			{
				if (onError != null)
					onError("Unable to find a loaded Asset Bundle with that name.");

				yield break;
			}

			AssetBundleRequest assetBundleRequest = assetBundle.LoadAllAssetsAsync<T>();
			while (!assetBundleRequest.isDone)
			{
				yield return null;

				if (onProgress != null)
					onProgress(assetBundleRequest.progress);
			}

			onComplete(assetBundleRequest.allAssets.Cast<T>().ToList());
		}

		private string GetBundleWithAsset(string path)
		{
#if UNITY_EDITOR
			if (_simulateBundles)
				return FindEditorAssetsBundle(path);
#endif

			foreach (AssetBundle bundle in AssetBundle.GetAllLoadedAssetBundles())
				if (bundle.Contains(path))
					return bundle.name;

			return string.Empty;
		}

		private void Clean()
		{
			Resources.UnloadUnusedAssets();
			GC.Collect();
		}

#if UNITY_EDITOR

		private static string FindEditorAssetsBundle(string path)
		{
			foreach (string bundle in AssetDatabase.GetAllAssetBundleNames())
			{
				foreach (string assetPath in AssetDatabase.GetAssetPathsFromAssetBundle(bundle))
				{
					if (assetPath == path)
						return bundle;
				}
			}

			return string.Empty;
		}

		private static T LoadEditorAsset<T>(string path) where T : Object
		{
			// find all assets that are within our path, and match our type
			string typeFilter = string.Format("t:{0}", typeof(T).Name);
			string[] guids = AssetDatabase.FindAssets(typeFilter);
			foreach (string guid in guids)
			{
				// find asset that matches our passed path
				string assetPath = AssetDatabase.GUIDToAssetPath(guid);
				if (assetPath.Contains(path))
				{
					// make sure it is the correct type, even if it matches our path
					T loaded = AssetDatabase.LoadAssetAtPath<T>(assetPath);
					if (loaded != null)
					{
						return loaded;
					}
				}
			}

			return null;
		}

		private static List<T> LoadEditorAssets<T>(string bundle) where T : Object
		{
			List<T> result = new List<T>();

			// find all assets that are within an asset bundle, and match our type
			string[] bundleAssetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(bundle);
			string typeFilter = string.Format("t:{0}", typeof(T).Name);
			string[] guids = AssetDatabase.FindAssets(typeFilter);
			foreach (string guid in guids)
			{
				// find asset that matches our passed path
				string assetPath = AssetDatabase.GUIDToAssetPath(guid);
				if (bundleAssetPaths.Contains(assetPath))
				{
					// make sure it is the correct type, even if it matches our path
					T loaded = AssetDatabase.LoadAssetAtPath<T>(assetPath);
					if (loaded != null)
						result.Add(loaded);
				}
			}

			return result;
		}
#endif
	}
}