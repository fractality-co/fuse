using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
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
		private class Implementation
		{
			public readonly string Name;
			public uint References;
			public bool Loaded;
			public bool Setup;
			public List<Object> Assets;

			public bool Active
			{
				get { return References > 0; }
			}

			public Implementation(string name)
			{
				Name = name;
			}
		}

		[SerializeField] private bool _simulateBundles;
		[SerializeField] private BuildMode _mode;
		[SerializeField] private Loading _core;

		private AssetBundles _bundles;
		private Configuration _configuration;
		private List<State> _allStates;

		private State _root;
		private Dictionary<string, Implementation> _implementations;

		private void Awake()
		{
			_bundles = new AssetBundles(_simulateBundles);
			_implementations = new Dictionary<string, Implementation>();
		}

		private IEnumerator Start()
		{
			yield return LoadImplementation(_core, Constants.CoreBundleFile);

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

		private static void FatalError(string message)
		{
			Logger.Error("Encountered a fatal error!\n" + message);

#if UNITY_EDITOR
			EditorApplication.isPaused = true;
#else
			Application.Quit();
#endif
		}

		private void SetState(string stateName)
		{
			if (_root != null)
			{
				foreach (string implementation in GetImplementations(_root))
					RemoveImplementation(implementation);
			}

			_root = GetState(stateName);

			foreach (string implementation in GetImplementations(_root))
				AddImplementation(implementation);

			foreach (KeyValuePair<string, Implementation> implementation in _implementations)
			{
				if (!implementation.Value.Active)
					UnloadImplementation(implementation.Key);
				else if (!implementation.Value.Loaded)
					StartCoroutine(LoadImplementation(_configuration.LoadImplementations, implementation.Value.Name));
			}
		}

		private State GetState(string stateName)
		{
			return _allStates.Find(current => current.name == stateName);
		}

		private List<string> GetImplementations(State state)
		{
			List<string> result = new List<string>();

			while (state != null)
			{
				result.AddRange(state.Implementations);
				state = GetState(state.Parent);
			}

			return result;
		}

		private void RemoveImplementation(string implementation)
		{
			_implementations[implementation].References--;
		}

		private void AddImplementation(string implementation)
		{
			if (!_implementations.ContainsKey(implementation))
				_implementations[implementation] = new Implementation(implementation);

			_implementations[implementation].References++;
		}

		private IEnumerator LoadImplementation(Loading loading, string implementation)
		{
			Logger.Info("Loading implementation: " + implementation + " ...");

			_implementations[implementation].Loaded = true;

			string asset = implementation.ToLower().Trim() + Constants.BundleExtension;
			switch (loading.Load)
			{
				case LoadMethod.Baked:
					yield return _bundles.LoadBundle
					(
						loading.GetPath(asset),
						bundle => { OnImplementationLoaded(implementation); },
						null,
						error => { OnImplementationLoadError(implementation, error); }
					);
					break;
				case LoadMethod.Online:
					yield return _bundles.LoadBundle
					(
						loading.GetUri(_mode, asset),
						loading.Version,
						bundle => { OnImplementationLoaded(implementation); },
						null,
						error => { OnImplementationLoadError(implementation, error); }
					);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void UnloadImplementation(string implementation)
		{
			StartCoroutine(CleanupImplementation(implementation));

			Logger.Info("Unloaded implementation: " + implementation);
		}

		private void OnImplementationLoaded(string implementation)
		{
			Logger.Info("Loaded bundle: " + implementation);

			if (implementation == Constants.CoreBundle) return;

			if (!_implementations[implementation].Setup)
				StartCoroutine(SetupImplementation(implementation));
		}

		private void OnImplementationLoadError(string implementation, string error)
		{
			FatalError(implementation + "\n" + error);
		}

		private IEnumerator SetupImplementation(string implementation)
		{
			yield return _bundles.LoadAssets(
				implementation,
				Type.GetType(implementation, true, true),
				result => { _implementations[implementation].Assets = result; },
				null,
				FatalError);

			// TODO: implementation vs implementations when loading and assigned to state, how do we solve this?
			// TODO: inject invocations
			// TODO: setup invocations
			// TODO: add pub/sub hooks

			_implementations[implementation].Setup = true;
		}

		private IEnumerator CleanupImplementation(string implementation)
		{
			// TODO: cleanup invocations
			// TODO: remove pub/sub hooks

			yield return null;

			_implementations.Remove(implementation);
			_bundles.UnloadBundle(implementation, true);
		}
	}
}