using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Object = UnityEngine.Object;

namespace Fuse.Core
{
	/// <summary>
	/// Executor for the framework. You should not be interacting with this.
	/// </summary>
	public class Fuse : SingletonBehaviour<Fuse>
	{
		private Configuration _configuration;
		private List<State> _allStates;
		private State _root;

		private List<StateTransition> _transitions;
		private Dictionary<string, List<ImplementationReference>> _implementations;

		private IEnumerator Start()
		{
			_transitions = new List<StateTransition>();
			_implementations = new Dictionary<string, List<ImplementationReference>>();

#if RELEASE
			Logger.Enabled = false;
#else
			Logger.Enabled = true;
#endif

			Logger.Info("Starting ...");

			string localPath = string.Format(Constants.AssetsBakedPath, Constants.CoreBundleFile);
			yield return AssetBundles.LoadBundle(localPath, null, null, Logger.Exception);

			yield return AssetBundles.LoadAsset<Configuration>(
				Constants.GetConfigurationAssetPath(),
				result => { _configuration = result; },
				null,
				Logger.Exception
			);

			Logger.Info("Loaded core");

			if (_configuration.Core.Load == LoadMethod.Online)
			{
				yield return AssetBundles.UnloadBundle(Constants.CoreBundle, false);

				yield return AssetBundles.LoadBundle(_configuration.Core.GetUri(Constants.CoreBundleFile),
					_configuration.Core.Version, null, null, Logger.Exception);

				yield return AssetBundles.LoadAsset<Configuration>(
					Constants.GetConfigurationAssetPath(),
					result => { _configuration = result; },
					null,
					Logger.Exception
				);

				Logger.Info("Updated core from remote source");
			}

			yield return AssetBundles.LoadAssets<State>(
				Constants.CoreBundle,
				result => { _allStates = result; },
				null,
				Logger.Exception);


			yield return SetState(_configuration.Start);

			Logger.Info("Started");
		}

		private void OnApplicationQuit()
		{
			AssetBundles.UnloadAllBundles(true);

			_transitions = null;
			_implementations = null;
			_configuration = null;
			_allStates = null;
			_root = null;

			Logger.Info("Stopped");
		}

		private IEnumerator SetState(string state)
		{
			if (string.IsNullOrEmpty(state))
			{
				Logger.Exception("You must have a valid initial state set in your " + typeof(Configuration).Name);
				yield break;
			}

			if (_root != null)
			{
				foreach (Implementation implementation in GetImplementations(_root))
					RemoveReference(implementation);
			}

			_root = GetState(state);

			_transitions.Clear();
			foreach (Transition transition in GetTransitions(_root))
				_transitions.Add(new StateTransition(transition));

			foreach (Implementation implementation in GetImplementations(_root))
				AddReference(implementation);

			foreach (KeyValuePair<string, List<ImplementationReference>> pair in _implementations)
			{
				foreach (ImplementationReference reference in pair.Value)
					if (!reference.Referenced)
						yield return CleanupImplementation(reference.Implementation);
			}

			foreach (KeyValuePair<string, List<ImplementationReference>> pair in _implementations)
			{
				foreach (ImplementationReference reference in pair.Value)
					if (reference.Referenced && !reference.Loaded)
						yield return LoadImplementation(reference.Implementation);
			}

			foreach (KeyValuePair<string, List<ImplementationReference>> pair in _implementations)
			{
				foreach (ImplementationReference reference in pair.Value)
					if (reference.Referenced && reference.Loaded && !reference.Active)
						yield return SetupImplementation(reference.Implementation);
			}
		}

		private State GetState(string state)
		{
			string[] values = state.Split(Constants.DefaultSeparator);
			string stateName = values[values.Length - 1].Replace(Constants.AssetExtension, string.Empty);
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
				result.AddRange(state.Implementations.Select(implementationPath => new Implementation(implementationPath)));
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
			Logger.Exception(implementation + "\n" + error);
		}

		private IEnumerator SetupImplementation(Implementation implementation)
		{
			yield return AssetBundles.LoadAssets(
				implementation.Bundle,
				Type.GetType(implementation.Type, true, true),
				result => { GetReference(implementation).Asset = result[0]; },
				null,
				Logger.Exception);

			// TODO: inject invocations
			// TODO: setup invocations
			// TODO: add pub/sub hooks

			GetReference(implementation).Active = true;
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

		private class ImplementationReference
		{
			public readonly Implementation Implementation;
			public uint Count;
			public bool Loaded;
			public bool Active;
			[UsedImplicitly] public Object Asset;

			public bool Referenced
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
	}
}