using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
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
	[DisallowMultipleComponent]
	public class Executor : MonoBehaviour
	{
		private class Reference
		{
			public readonly Implementation Implementation;
			public uint Count;
			public bool Loaded;
			public bool Setup;
			[UsedImplicitly] public Object Asset;

			public bool Active
			{
				get { return Count > 0; }
			}

			public Reference(Implementation implementation)
			{
				Implementation = implementation;
			}
		}

		[SerializeField] private Loading _core;

		private Configuration _configuration;
		private List<State> _allStates;

		private State _root;
		private readonly Dictionary<string, List<Reference>> _implementations = new Dictionary<string, List<Reference>>();

		private void Awake()
		{
			StartCoroutine(LoadCore());
		}

		private IEnumerator LoadCore()
		{
			yield return LoadImplementation(_core, new Implementation(Constants.CoreBundle, string.Empty));

			yield return AssetBundles.LoadAsset<Configuration>(
				Constants.GetConfigurationAssetPath(),
				result => { _configuration = result; },
				null,
				FatalError
			);

			yield return AssetBundles.LoadAssets<State>(
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
			AssetBundles.UnloadAllBundles(true);
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
				foreach (Implementation implementation in GetImplementations(_root))
					RemoveReference(implementation);
			}

			_root = GetState(stateName);

			foreach (Implementation implementation in GetImplementations(_root))
				AddReference(implementation);

			foreach (KeyValuePair<string, List<Reference>> pair in _implementations)
			{
				foreach (Reference reference in pair.Value)
				{
					if (!reference.Active)
						UnloadImplementation(reference.Implementation);
					else if (!reference.Loaded)
						StartCoroutine(LoadImplementation(_configuration.LoadImplementations, reference.Implementation));
				}
			}
		}

		private State GetState(string stateName)
		{
			return _allStates.Find(current => current.name == stateName);
		}

		private List<Implementation> GetImplementations(State state)
		{
			List<Implementation> result = new List<Implementation>();

			while (state != null)
			{
				result.AddRange(state.Implementations);
				state = state.IsRoot ? null : GetState(state.Parent);
			}

			return result;
		}

		private void RemoveReference(Implementation implementation)
		{
			GetReference(implementation).Count--;
		}

		private void AddReference(Implementation implementation)
		{
			if (!_implementations.ContainsKey(implementation.Type))
				_implementations[implementation.Type] = new List<Reference>();

			Reference reference = GetReference(implementation);
			if (reference == null)
			{
				reference = new Reference(implementation);
				_implementations[implementation.Type].Add(reference);
			}

			reference.Count++;
		}

		private Reference GetReference(Implementation implementation)
		{
			if (!_implementations.ContainsKey(implementation.Type))
				return null;

			return _implementations[implementation.Type].Find(current => current.Implementation.Name == implementation.Name);
		}

		private IEnumerator LoadImplementation(Loading loading, Implementation implementation)
		{
			Logger.Info("Loading implementation: " + implementation.Type + " ...");

			GetReference(implementation).Loaded = true;

			string asset = implementation.Bundle + Constants.BundleExtension;
			switch (loading.Load)
			{
				case LoadMethod.Baked:
					yield return AssetBundles.LoadBundle
					(
						loading.GetPath(asset),
						bundle => { OnImplementationLoaded(implementation); },
						null,
						error => { OnImplementationLoadError(implementation, error); }
					);
					break;
				case LoadMethod.Online:
					yield return AssetBundles.LoadBundle
					(
						loading.GetUri(asset),
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

		private void UnloadImplementation(Implementation implementation)
		{
			Logger.Info("Unloading implementation: " + implementation.Type);

			if (implementation.Type == Constants.CoreBundle)
			{
				AssetBundles.UnloadBundle(implementation.Bundle, false);
				return;
			}

			StartCoroutine(CleanupImplementation(implementation));
		}

		private void OnImplementationLoaded(Implementation implementation)
		{
			Logger.Info("Loading implementation: " + implementation.Type);

			if (implementation.Type == Constants.CoreBundle) return;

			if (!GetReference(implementation).Setup)
				StartCoroutine(SetupImplementation(implementation));
		}

		private void OnImplementationLoadError(Implementation implementation, string error)
		{
			FatalError(implementation + "\n" + error);
		}

		private IEnumerator SetupImplementation(Implementation implementation)
		{
			yield return AssetBundles.LoadAssets(
				implementation.Bundle,
				Type.GetType(implementation.Type, true, true),
				result => { GetReference(implementation).Asset = result[0]; },
				null,
				FatalError);

			// TODO: inject invocations
			// TODO: setup invocations
			// TODO: add pub/sub hooks

			GetReference(implementation).Setup = true;
		}

		private IEnumerator CleanupImplementation(Implementation implementation)
		{
			// TODO: remove pub/sub hooks
			// TODO: cleanup invocations

			yield return null;

			Reference reference = GetReference(implementation);
			reference.Asset = null;

			_implementations[implementation.Type].Remove(reference);
			if (_implementations[implementation.Type].Count == 0)
			{
				_implementations.Remove(implementation.Type);
				AssetBundles.UnloadBundle(implementation.Bundle, true);
			}
		}
	}
}