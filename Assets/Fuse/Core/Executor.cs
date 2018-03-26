using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace Fuse.Core
{
	/// <summary>
	/// Executor for the framework. You should not be interacting with this.
	/// </summary>
	public class Executor : MonoBehaviour
	{
		[SerializeField] private bool _simulateBundles;
		[SerializeField] private BuildMode _mode;
		[SerializeField] private Loading _core;

		private AssetBundles _bundles;
		private Configuration _configuration;
		private List<State> _allStates;

		private State _root;
		private Dictionary<string, int> _implementations;

		private void Awake()
		{
			_bundles = new AssetBundles(_simulateBundles);
			_implementations = new Dictionary<string, int>();
		}

		private IEnumerator Start()
		{
			yield return LoadBundle(_core, Constants.CoreBundleFile, 1);

			yield return _bundles.LoadAsset<Configuration>(
				Constants.GetConfigurationAssetPath(),
				result => { _configuration = result; },
				null,
				FatalError
			);

			yield return _bundles.LoadAssets<State>(
				Constants.CoreBundle,
				result => { _allStates = result; },
				null,
				FatalError);

			SetState(_configuration.Start);
		}

		private void OnApplicationQuit()
		{
			Logger.Info("Application quitting; immediately stopping");
			DestroyImmediate(gameObject);
		}

		private void OnDestroy()
		{
			_bundles.UnloadAllBundles(true);
			Logger.Info("Stopped");
		}

		private void SetState(string stateName)
		{
			if (_root != null)
			{
				foreach (KeyValuePair<string, int> pair in GetReferenceTree(_root))
					UnloadImplementation(pair.Key, pair.Value);
			}

			_root = GetState(stateName);
			if (_root == null)
			{
				FatalError("Attempting to go invalid state, unable to find: " + stateName);
				return;
			}

			foreach (KeyValuePair<string, int> pair in GetReferenceTree(_root))
				LoadImplementation(pair.Key, pair.Value);
		}

		private State GetState(string stateName)
		{
			return _allStates.Find(current => current.name == stateName);
		}

		private Dictionary<string, int> GetReferenceTree(State state)
		{
			throw new NotImplementedException("Find top-level dependencies via state tree");
		}

		private void UnloadImplementation(string implementation, int references = 1)
		{
			if (!_implementations.ContainsKey(implementation))
			{
				Logger.Warn("Attempting to unload a implementation reference, but it is not loaded: " + implementation);
				return;
			}

			_implementations[implementation] -= references;
		}

		private void LoadImplementation(string implementation, int references)
		{
			if (_implementations.ContainsKey(implementation))
			{
				Logger.Info("Implementation already loaded, skipping load: " + implementation);
				OnBundleLoaded(implementation, references);
				return;
			}

			StartCoroutine(LoadBundle(_configuration.Implementations, implementation, references));
		}

		private void FatalError(string message)
		{
			Logger.Error("Encountered a fatal error!\n" + message);

#if UNITY_EDITOR
			EditorApplication.isPaused = true;
#else
			Application.Quit();
#endif
		}

		private IEnumerator LoadBundle(Loading loading, string bundleName, int references)
		{
			Logger.Info("Loading " + bundleName + " ...");

			string asset = bundleName + Constants.BundleExtension;
			switch (loading.Load)
			{
				case LoadMethod.Baked:
					yield return _bundles.LoadBundle
					(
						loading.GetPath(asset),
						bundle => { OnBundleLoaded(bundleName, references); },
						progress => { OnBundleProgress(bundleName, progress); },
						error => { OnBundleError(bundleName, error); }
					);
					break;
				case LoadMethod.Online:
					yield return _bundles.LoadBundle
					(
						loading.GetUri(_mode, asset),
						loading.Version,
						bundle => { OnBundleLoaded(bundleName, references); },
						progress => { OnBundleProgress(bundleName, progress); },
						error => { OnBundleError(bundleName, error); }
					);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void UnloadBundle(string bundleName)
		{
			if (!_implementations.ContainsKey(bundleName))
			{
				Logger.Error("Attempting to unload implementation bundle when none exist for it: " + bundleName);
				return;
			}

			if (_implementations[bundleName] > 0)
			{
				Logger.Warn("Attempted to unload bundle when there are references to it: " + bundleName);
				return;
			}

			_implementations.Remove(bundleName);
			_bundles.UnloadBundle(bundleName, true);
			Logger.Info("Unloaded bundle: " + bundleName);
		}

		private void OnBundleLoaded(string bundleName, int references)
		{
			Logger.Info("Loaded bundle: " + bundleName);

			if (bundleName != Constants.CoreBundle)
			{
				if (!_implementations.ContainsKey(bundleName))
				{
					_implementations[bundleName] = 0;
					StartCoroutine(SetupImplementation(bundleName));
				}

				_implementations[bundleName] += references;
			}
		}

		private void OnBundleProgress(string bundleName, float progress)
		{
		}

		private void OnBundleError(string bundleName, string error)
		{
			if (bundleName == Constants.CoreBundle)
			{
				FatalError(error);
				return;
			}

			Logger.Error(error);
		}

		private IEnumerator SetupImplementation(string implementation)
		{
		}

		private IEnumerator CleanupImplementation(string implementation)
		{
		}
	}
}