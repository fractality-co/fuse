using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace Fuse.Core
{
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	public static class AssetBundles
	{
#if UNITY_EDITOR
		private const string SimulateKey = "SimulateAssetBundles";

		public static bool Simulate
		{
			get { return EditorPrefs.GetBool(SimulateKey, true); }
			set { EditorPrefs.SetBool(SimulateKey, value); }
		}
#endif

		public static IEnumerator LoadBundle(Uri uri, int version = -1, Action<string> onComplete = null,
			Action<float> onProgress = null, Action<string> onError = null)
		{
#if UNITY_EDITOR
			if (Simulate)
			{
				if (onComplete != null)
					onComplete(Constants.GetFileNameFromPath(uri.AbsolutePath, Constants.BundleExtension));
				yield break;
			}
#endif

			UnityWebRequest request;
			if (version >= 0)
				request = UnityWebRequest.GetAssetBundle(uri.AbsoluteUri, (uint) version, 0);
			else
				request = UnityWebRequest.GetAssetBundle(uri.AbsoluteUri, 0);

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
					onError("Error when loading bundle from web at: " + uri.AbsoluteUri + " [" + version + "]\n" + request.error);

				yield break;
			}

			if (onComplete != null)
				onComplete(assetBundle.name);
		}

		public static IEnumerator LoadBundle(string path, Action<string> onComplete = null, Action<float> onProgress = null,
			Action<string> onError = null)
		{
#if UNITY_EDITOR
			if (Simulate)
			{
				if (onComplete != null)
					onComplete(Constants.GetFileNameFromPath(path, Constants.BundleExtension));
				yield break;
			}
#endif

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

		public static bool UnloadBundle(string bundleName, bool unloadActiveAssets)
		{
#if UNITY_EDITOR
			if (Simulate)
			{
				Clean();
				return true;
			}
#endif

			AssetBundle loadedBundle = null;
			foreach (AssetBundle bundle in AssetBundle.GetAllLoadedAssetBundles())
			{
				if (bundle.name == bundleName)
					loadedBundle = bundle;
			}

			if (loadedBundle == null)
				return false;

			loadedBundle.Unload(unloadActiveAssets);
			Clean();
			return true;
		}

		public static void UnloadAllBundles(bool unloadActiveAssets)
		{
			AssetBundle.UnloadAllAssetBundles(unloadActiveAssets);
			Clean();
		}

		public static IEnumerator LoadAsset<T>(string path, Action<T> onComplete, Action<float> onProgress = null,
			Action<string> onError = null) where T : Object
		{
#if UNITY_EDITOR
			if (Simulate)
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

		public static IEnumerator LoadAssets(string bundle, Type type, Action<List<Object>> onComplete,
			Action<float> onProgress = null,
			Action<string> onError = null)
		{
#if UNITY_EDITOR
			if (Simulate)
			{
				onComplete(LoadEditorAssets(bundle, type));
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

			AssetBundleRequest assetBundleRequest = assetBundle.LoadAllAssetsAsync(type);
			while (!assetBundleRequest.isDone)
			{
				yield return null;

				if (onProgress != null)
					onProgress(assetBundleRequest.progress);
			}

			onComplete(assetBundleRequest.allAssets.ToList());
		}

		public static IEnumerator LoadAssets<T>(string bundle, Action<List<T>> onComplete,
			Action<float> onProgress = null,
			Action<string> onError = null) where T : Object
		{
#if UNITY_EDITOR
			if (Simulate)
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

		private static string GetBundleWithAsset(string path)
		{
#if UNITY_EDITOR
			if (Simulate)
				return FindEditorAssetsBundle(path);
#endif

			foreach (AssetBundle bundle in AssetBundle.GetAllLoadedAssetBundles())
				if (bundle.Contains(path))
					return bundle.name;

			return string.Empty;
		}

		private static void Clean()
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

		private static List<Object> LoadEditorAssets(string bundle, Type type)
		{
			List<Object> result = new List<Object>();

			// find all assets that are within an asset bundle, and match our type
			string[] bundleAssetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(bundle);
			string typeFilter = string.Format("t:{0}", type.Name);
			string[] guids = AssetDatabase.FindAssets(typeFilter);
			foreach (string guid in guids)
			{
				// find asset that matches our passed path
				string assetPath = AssetDatabase.GUIDToAssetPath(guid);
				if (bundleAssetPaths.Contains(assetPath))
				{
					// make sure it is the correct type, even if it matches our path
					Object loaded = AssetDatabase.LoadAssetAtPath(assetPath, type);
					if (loaded != null)
						result.Add(loaded);
				}
			}

			return result;
		}
#endif
	}
}