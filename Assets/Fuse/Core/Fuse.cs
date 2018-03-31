using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fuse.Implementation;
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
					if (!reference.Referenced) yield return CleanupImplementation(reference);
			}

			foreach (KeyValuePair<string, List<ImplementationReference>> pair in _implementations)
			{
				foreach (ImplementationReference reference in pair.Value)
					if (!reference.Running) yield return SetupImplementation(reference);
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
			GetReference(implementation).RemoveReference();
		}

		private void AddReference(Implementation implementation)
		{
			if (!_implementations.ContainsKey(implementation.Type))
				_implementations[implementation.Type] = new List<ImplementationReference>();

			ImplementationReference reference = GetReference(implementation);
			if (reference == null)
			{
				reference = new ImplementationReference(implementation, _configuration, OnEventPublished);
				_implementations[implementation.Type].Add(reference);
			}

			reference.AddReference();
		}

		private ImplementationReference GetReference(Implementation implementation)
		{
			if (!_implementations.ContainsKey(implementation.Type))
				return null;

			return _implementations[implementation.Type].Find(current => current.Name == implementation.Name);
		}

		private IEnumerator SetupImplementation(ImplementationReference reference)
		{
			yield return reference.SetLifecycle(Lifecycle.Load);
			yield return reference.SetLifecycle(Lifecycle.Setup);
			yield return reference.SetLifecycle(Lifecycle.Active);
		}

		private IEnumerator CleanupImplementation(ImplementationReference reference)
		{
			yield return reference.SetLifecycle(Lifecycle.Cleanup);
			yield return reference.SetLifecycle(Lifecycle.Unload);

			_implementations[reference.Type].Remove(reference);
			if (_implementations[reference.Type].Count == 0)
				_implementations.Remove(reference.Type);
		}

		private void OnEventPublished(string type)
		{
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
			public uint Count { get; private set; }

			public bool Referenced
			{
				get { return Count > 0; }
			}

			public string Type
			{
				get { return _implementation.Type; }
			}

			public string Name
			{
				get { return _implementation.Name; }
			}

			public bool Running
			{
				get { return Lifecycle != Lifecycle.None; }
			}

			public Lifecycle Lifecycle { get; private set; }

			private Object _asset;
			private readonly Action<string> _notify;
			private readonly Implementation _implementation;
			private readonly Configuration _config;

			public ImplementationReference(Implementation implementation, Configuration config, Action<string> notify)
			{
				_config = config;
				_notify = notify;
				_implementation = implementation;
			}

			public void AddReference()
			{
				Count++;
			}

			public void RemoveReference()
			{
				Count--;
			}

			public IEnumerator SetLifecycle(Lifecycle lifecycle)
			{
				// TODO: process all IFuseLifecycle
				
				Lifecycle = lifecycle;

				switch (Lifecycle)
				{
					case Lifecycle.Load:
						yield return Load();
						break;
					case Lifecycle.Setup:
						// TODO: add notifier hook for all IFuseNotifier
						break;
					case Lifecycle.Cleanup:
						// TODO: remove notifier hook for all IFuseNotifier
						break;
					case Lifecycle.Unload:
						AssetBundles.UnloadBundle(_implementation.Bundle, true);
						break;
				}

				yield return ProcessAttributes();
			}

			private IEnumerator Load()
			{
				switch (_config.Implementations.Load)
				{
					case LoadMethod.Baked:
						yield return AssetBundles.LoadBundle
						(
							_config.GetAssetPath(_implementation),
							null,
							null,
							Logger.Exception
						);
						break;
					case LoadMethod.Online:
						yield return AssetBundles.LoadBundle
						(
							_config.GetAssetUri(_implementation),
							_config.GetAssetVersion(_implementation),
							null,
							null,
							Logger.Exception
						);
						break;
				}

				yield return AssetBundles.LoadAssets(
					_implementation.Bundle,
					System.Type.GetType(_implementation.Type, true, true),
					result => { _asset = result[0]; },
					null,
					Logger.Exception);
			}

			private IEnumerator ProcessAttributes()
			{
				// TODO: process lifecycle for this implementation
				throw new NotImplementedException();
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