using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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
					if (!reference.Referenced)
						yield return CleanupImplementation(reference);
			}

			foreach (KeyValuePair<string, List<ImplementationReference>> pair in _implementations)
			{
				foreach (ImplementationReference reference in pair.Value)
					if (!reference.Running)
						yield return SetupImplementation(reference);
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
				reference = new ImplementationReference(implementation, _configuration, OnEventPublished, StartAsync);
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

		private void StartAsync(IEnumerator routine)
		{
			StartCoroutine(routine);
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
			private readonly Action<IEnumerator> _async;
			private readonly Implementation _implementation;
			private readonly Configuration _config;

			public ImplementationReference(Implementation implementation, Configuration config, Action<string> notify,
				Action<IEnumerator> async)
			{
				_async = async;
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
				switch (lifecycle)
				{
					case Lifecycle.Load:
						yield return Load();
						ProcessListeners(true);
						ProcessInjections(lifecycle, Lifecycle);
						break;
					case Lifecycle.Setup:
						ProcessInjections(lifecycle, Lifecycle);
						break;
					case Lifecycle.Cleanup:
						ProcessInjections(lifecycle, Lifecycle);
						break;
					case Lifecycle.Unload:
						ProcessInjections(lifecycle, Lifecycle);
						ProcessListeners(false);
						AssetBundles.UnloadBundle(_implementation.Bundle, true);
						break;
				}

				Lifecycle = lifecycle;
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

			private void ProcessListeners(bool active)
			{
				foreach (MemberInfo member in _asset.GetType().GetMembers())
				{
					foreach (IFuseNotifier notifier in member.GetCustomAttributes(typeof(IFuseNotifier), true))
					{
						if (active)
							notifier.AddListener(_notify);
						else
							notifier.RemoveListener(_notify);
					}
				}
			}

			private void ProcessInjections(Lifecycle toEnter, Lifecycle toExit)
			{
				List<Pair<IFuseAttribute, MemberInfo>> attributes = new List<Pair<IFuseAttribute, MemberInfo>>();
				foreach (MemberInfo member in _asset.GetType().GetMembers())
				{
					foreach (object custom in member.GetCustomAttributes(typeof(IFuseAttribute), true))
						attributes.Add(new Pair<IFuseAttribute, MemberInfo>((IFuseAttribute) custom, member));
				}

				attributes.Sort((a, b) => a.A.Order < b.A.Order ? -1 : 1);

				foreach (Pair<IFuseAttribute, MemberInfo> attribute in attributes)
				{
					// we have a custom default value
					Lifecycle active = attribute.A.Lifecycle;
					if (active == Lifecycle.None)
						active = (Lifecycle)((DefaultValueAttribute)active.GetType().GetCustomAttributes(true)[0]).Value;
						
					if (IsSubclassOfRawGeneric(typeof(IFuseInjection<>), attribute.A.GetType()))
					{
						if (toEnter == active)
							continue;

						if (attribute.A is IFuseInjection<MethodInfo>)
							((IFuseInjection<MethodInfo>) attribute.A).Process(attribute.B as MethodInfo, _asset);
						else if (attribute.A is IFuseInjection<PropertyInfo>)
							((IFuseInjection<PropertyInfo>) attribute.A).Process(attribute.B as PropertyInfo, _asset);
						else if (attribute.A is IFuseInjection<FieldInfo>)
							((IFuseInjection<FieldInfo>) attribute.A).Process(attribute.B as FieldInfo, _asset);
						else if (attribute.A is IFuseInjection<EventInfo>)
							((IFuseInjection<EventInfo>) attribute.A).Process(attribute.B as EventInfo, _asset);
					}
					else if (IsSubclassOfRawGeneric(typeof(IFuseInjectionAsync<>), attribute.A.GetType()))
					{
						if (toEnter == active)
							continue;

						if (attribute.A is IFuseInjectionAsync<MethodInfo>)
							_async(((IFuseInjectionAsync<MethodInfo>) attribute.A).Process(attribute.B as MethodInfo, _asset));
						else if (attribute.A is IFuseInjectionAsync<PropertyInfo>)
							_async(((IFuseInjectionAsync<PropertyInfo>) attribute.A).Process(attribute.B as PropertyInfo, _asset));
						else if (attribute.A is IFuseInjectionAsync<FieldInfo>)
							_async(((IFuseInjectionAsync<FieldInfo>) attribute.A).Process(attribute.B as FieldInfo, _asset));
						else if (attribute.A is IFuseInjectionAsync<EventInfo>)
							_async(((IFuseInjectionAsync<EventInfo>) attribute.A).Process(attribute.B as EventInfo, _asset));
					}
					else if (IsSubclassOfRawGeneric(typeof(IFuseLifecycle<>), attribute.A.GetType()))
					{
						if (toEnter == active)
						{
							if (attribute.A is IFuseLifecycle<MethodInfo>)
								((IFuseLifecycle<MethodInfo>) attribute.A).OnEnter(attribute.B as MethodInfo, _asset);
							else if (attribute.A is IFuseLifecycle<PropertyInfo>)
								((IFuseLifecycle<PropertyInfo>) attribute.A).OnEnter(attribute.B as PropertyInfo, _asset);
							else if (attribute.A is IFuseLifecycle<FieldInfo>)
								((IFuseLifecycle<FieldInfo>) attribute.A).OnEnter(attribute.B as FieldInfo, _asset);
							else if (attribute.A is IFuseLifecycle<EventInfo>)
								((IFuseLifecycle<EventInfo>) attribute.A).OnEnter(attribute.B as EventInfo, _asset);
						}
						else if (toExit == active)
						{
							if (attribute.A is IFuseLifecycle<MethodInfo>)
								((IFuseLifecycle<MethodInfo>) attribute.A).OnExit(attribute.B as MethodInfo, _asset);
							else if (attribute.A is IFuseLifecycle<PropertyInfo>)
								((IFuseLifecycle<PropertyInfo>) attribute.A).OnExit(attribute.B as PropertyInfo, _asset);
							else if (attribute.A is IFuseLifecycle<FieldInfo>)
								((IFuseLifecycle<FieldInfo>) attribute.A).OnExit(attribute.B as FieldInfo, _asset);
							else if (attribute.A is IFuseLifecycle<EventInfo>)
								((IFuseLifecycle<EventInfo>) attribute.A).OnExit(attribute.B as EventInfo, _asset);
						}
					}
				}
			}

			private static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
			{
				while (toCheck != null && toCheck != typeof(object))
				{
					var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
					if (generic == cur)
					{
						return true;
					}

					toCheck = toCheck.BaseType;
				}

				return false;
			}
		}

		private class StateTransition
		{
			private readonly List<string> _events;

			public string State { get; private set; }

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
				return _events.Count == 0;
			}
		}
	}
}