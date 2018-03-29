using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
		private class ImplementationReference
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

			public ImplementationReference(Implementation implementation)
			{
				Implementation = implementation;
			}
		}

		private class StateTransition
		{
			private readonly List<string> _events;

			public string State { get; private set; }

			public bool Transition
			{
				get { return _events.Count == 0; }
			}

			public StateTransition(Transition transition)
			{
				State = transition.To;
				_events = transition.Events.ToList();
			}

			public bool ProcessEvent(string type)
			{
				int index = _events.IndexOf(type);
				if (index >= 0)
					_events.RemoveAt(index);

				return Transition;
			}
		}

		private Configuration _configuration;
		private List<State> _allStates;
		private State _root;

		private readonly List<StateTransition> _transitions = new List<StateTransition>();

		private readonly Dictionary<string, List<ImplementationReference>> _implementations =
			new Dictionary<string, List<ImplementationReference>>();

		private void Awake()
		{
			StartCoroutine(LoadCore());
		}

		private IEnumerator LoadCore()
		{
			string localPath = string.Format(Constants.AssetsBakedPath, Constants.CoreBundleFile);
			yield return AssetBundles.LoadBundle(localPath, null, null, FatalError);

			yield return AssetBundles.LoadAsset<Configuration>(
				Constants.GetConfigurationAssetPath(),
				result => { _configuration = result; },
				null,
				FatalError
			);

			if (_configuration.Core.Load == LoadMethod.Online)
			{
				yield return AssetBundles.UnloadBundle(Constants.CoreBundle, false);

				yield return AssetBundles.LoadBundle(_configuration.Core.GetUri(Constants.CoreBundleFile),
					_configuration.Core.Version, null, null, FatalError);

				yield return AssetBundles.LoadAsset<Configuration>(
					Constants.GetConfigurationAssetPath(),
					result => { _configuration = result; },
					null,
					FatalError
				);
			}

			yield return AssetBundles.LoadAssets<State>(
				Constants.CoreBundle,
				result => { _allStates = result; },
				null,
				FatalError);

			yield return SetState(_configuration.Start);
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

		private IEnumerator SetState(string stateName)
		{
			if (_root != null)
			{
				foreach (Implementation implementation in GetImplementations(_root))
					RemoveReference(implementation);
			}

			_root = GetState(stateName);

			_transitions.Clear();
			foreach (Transition transition in GetTransitions(_root))
				_transitions.Add(new StateTransition(transition));

			foreach (Implementation implementation in GetImplementations(_root))
				AddReference(implementation);

			foreach (KeyValuePair<string, List<ImplementationReference>> pair in _implementations)
			{
				foreach (ImplementationReference reference in pair.Value)
					if (!reference.Active)
						yield return CleanupImplementation(reference.Implementation);
			}

			foreach (KeyValuePair<string, List<ImplementationReference>> pair in _implementations)
			{
				foreach (ImplementationReference reference in pair.Value)
					if (reference.Active && !reference.Loaded)
						yield return LoadImplementation(reference.Implementation);
			}

			foreach (KeyValuePair<string, List<ImplementationReference>> pair in _implementations)
			{
				foreach (ImplementationReference reference in pair.Value)
					if (reference.Active && reference.Loaded && !reference.Setup)
						yield return SetupImplementation(reference.Implementation);
			}
		}

		private State GetState(string stateName)
		{
			return _allStates.Find(current => current.name == stateName);
		}

		private List<Transition> GetTransitions(State state)
		{
			List<Transition> result = new List<Transition>();

			while (state != null)
			{
				result.AddRange(state.Transitions);
				state = state.IsRoot ? null : GetState(state.Parent);
			}

			return result;
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
				_implementations[implementation.Type] = new List<ImplementationReference>();

			ImplementationReference reference = GetReference(implementation);
			if (reference == null)
			{
				reference = new ImplementationReference(implementation);
				_implementations[implementation.Type].Add(reference);
			}

			reference.Count++;
		}

		private ImplementationReference GetReference(Implementation implementation)
		{
			if (!_implementations.ContainsKey(implementation.Type))
				return null;

			return _implementations[implementation.Type].Find(current => current.Implementation.Name == implementation.Name);
		}

		private IEnumerator LoadImplementation(Implementation implementation)
		{
			Logger.Info("Loading implementation: " + implementation.Type + " ...");

			GetReference(implementation).Loaded = true;

			switch (_configuration.Implementations.Load)
			{
				case LoadMethod.Baked:
					yield return AssetBundles.LoadBundle
					(
						_configuration.GetAssetPath(implementation),
						bundle => { OnImplementationLoaded(implementation); },
						null,
						error => { OnImplementationLoadError(implementation, error); }
					);
					break;
				case LoadMethod.Online:
					yield return AssetBundles.LoadBundle
					(
						_configuration.GetAssetUri(implementation),
						_configuration.GetAssetVersion(implementation),
						bundle => { OnImplementationLoaded(implementation); },
						null,
						error => { OnImplementationLoadError(implementation, error); }
					);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void OnImplementationLoaded(Implementation implementation)
		{
			Logger.Info("Loaded implementation: " + implementation.Type);
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

			ImplementationReference reference = GetReference(implementation);
			reference.Asset = null;

			_implementations[implementation.Type].Remove(reference);
			if (_implementations[implementation.Type].Count == 0)
			{
				_implementations.Remove(implementation.Type);
				AssetBundles.UnloadBundle(implementation.Bundle, true);
			}
		}

		private void OnEventPublished(string type)
		{
			// TODO: invoke listeners to this event (subscribers)

			foreach (StateTransition transition in _transitions)
			{
				if (transition.ProcessEvent(type))
				{
					StartCoroutine(SetState(transition.State));
					break;
				}
			}
		}
	}
}