using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Fuse.Feature;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Fuse.Core
{
	/// <summary>
	/// Executor for the framework. You should not be interacting with this.
	/// </summary>
	public class Fuse : SingletonBehaviour<Fuse>
	{
		private Environment _environment;
		private Configuration _configuration;
		private List<State> _allStates;
		private State _root;

		private List<StateTransition> _transitions;
		private Dictionary<string, List<FeatureReference>> _features;
		private Dictionary<string, SceneReference> _scenes;
		private List<SceneReference> _loading;

		private IEnumerator Start()
		{
			_transitions = new List<StateTransition>();
			_features = new Dictionary<string, List<FeatureReference>>();
			_scenes = new Dictionary<string, SceneReference>();
			_loading = new List<SceneReference>();

#if DEVELOPMENT_BUILD || UNITY_EDITOR
			Logger.Enabled = true;
#else
			Logger.Enabled = false;
#endif

			Logger.Info("Starting ...");

			string localPath = string.Format(Constants.AssetsBakedPath, Constants.CoreBundleFile);
			yield return AssetBundles.LoadBundle(localPath, null, null, Logger.Exception);

			Logger.Info("Loaded baked core");

			yield return AssetBundles.LoadAsset<Configuration>(
				Constants.GetConfigurationAssetPath(),
				result => { _configuration = result; },
				null,
				Logger.Exception
			);

			Logger.Info("Loaded configuration");

			yield return AssetBundles.LoadAsset<Environment>(
				_configuration.Environment,
				result => { _environment = result; },
				null,
				Logger.Exception
			);

			Logger.Info("Loaded environment");

			if (_environment.Loading == LoadMethod.Online)
			{
				yield return AssetBundles.UnloadBundle(Constants.CoreBundle, false);
				yield return AssetBundles.LoadBundle(_environment.GetUri(Constants.CoreBundleFile, true), -1, null, null,
					Logger.Exception);

				yield return AssetBundles.LoadAsset<Configuration>(
					Constants.GetConfigurationAssetPath(),
					result => { _configuration = result; },
					null,
					Logger.Exception
				);

				yield return AssetBundles.LoadAsset<Environment>(
					_configuration.Environment,
					result => { _environment = result; },
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
			StopAllCoroutines();

			_transitions = null;
			_features = null;
			_configuration = null;
			_allStates = null;
			_root = null;

			AssetBundles.UnloadAllBundles(true);

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
				foreach (Feature feature in GetFeatures(_root))
					RemoveFeatureReference(feature);

				foreach (string scenePath in GetScenes(_root))
					RemoveSceneReference(scenePath);
			}

			_root = GetState(state);

			foreach (string loadingPath in GetLoading(_root))
			{
				SceneReference loading = new SceneReference(loadingPath, _environment);
				loading.AddReference();

				yield return loading.Process();

				_loading.Add(loading);
			}

			_transitions.Clear();
			foreach (Transition transition in GetTransitions(_root))
				_transitions.Add(new StateTransition(transition));

			foreach (Feature feature in GetFeatures(_root))
				AddFeatureReference(feature);

			foreach (string scenePath in GetScenes(_root))
				AddSceneReference(scenePath);

			foreach (KeyValuePair<string, List<FeatureReference>> pair in _features)
			{
				foreach (FeatureReference reference in pair.Value)
					if (!reference.Referenced)
						yield return CleanupFeature(reference);
			}

			List<string> inactiveScenes = new List<string>();
			foreach (KeyValuePair<string, SceneReference> pair in _scenes)
			{
				yield return pair.Value.Process();
				if (!pair.Value.Active)
					inactiveScenes.Add(pair.Key);
			}

			inactiveScenes.ForEach(key => _scenes.Remove(key));

			foreach (KeyValuePair<string, List<FeatureReference>> pair in _features)
			{
				foreach (FeatureReference reference in pair.Value)
					if (!reference.Running)
						yield return SetupFeature(reference);
			}

			foreach (SceneReference loading in _loading)
			{
				loading.RemoveReference();
				yield return loading.Process();
			}

			_loading.Clear();

			foreach (StateTransition transition in _transitions)
			{
				if (transition.Transition)
				{
					yield return SetState(transition.State);
					break;
				}
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

		private List<string> GetScenes(State state)
		{
			List<string> result = new List<string>();

			while (state != null)
			{
				result.AddRange(state.Scenes);
				state = state.IsRoot ? null : GetState(state.Parent);
			}

			return result;
		}

		private List<string> GetLoading(State state)
		{
			List<string> result = new List<string>();

			while (state != null)
			{
				if (!string.IsNullOrEmpty(state.Loading))
					result.Add(state.Loading);

				state = state.IsRoot ? null : GetState(state.Parent);
			}

			return result;
		}

		private List<Feature> GetFeatures(State state)
		{
			List<Feature> result = new List<Feature>();

			while (state != null)
			{
				result.AddRange(state.Features.Select(featurePath => new Feature(featurePath)));
				state = state.IsRoot ? null : GetState(state.Parent);
			}

			return result;
		}

		private void RemoveSceneReference(string scenePath)
		{
			_scenes[scenePath].RemoveReference();
		}

		private void AddSceneReference(string scenePath)
		{
			if (!_scenes.ContainsKey(scenePath))
				_scenes[scenePath] = new SceneReference(scenePath, _environment);

			_scenes[scenePath].AddReference();
		}

		private void RemoveFeatureReference(Feature feature)
		{
			GetReference(feature).RemoveReference();
		}

		private void AddFeatureReference(Feature feature)
		{
			if (!_features.ContainsKey(feature.Type))
				_features[feature.Type] = new List<FeatureReference>();

			FeatureReference reference = GetReference(feature);
			if (reference == null)
			{
				reference = new FeatureReference(feature, _environment, OnEventPublished, StartAsync, StopAsync);
				_features[feature.Type].Add(reference);
			}

			reference.AddReference();
		}

		private FeatureReference GetReference(Feature feature)
		{
			if (!_features.ContainsKey(feature.Type))
				return null;

			return _features[feature.Type].Find(current => current.Name == feature.Name);
		}

		private IEnumerator SetupFeature(FeatureReference reference)
		{
			Logger.Info("Setting up feature: " + reference.Type + "::" + reference.Name);

			yield return reference.SetLifecycle(Lifecycle.Load);
			yield return reference.SetLifecycle(Lifecycle.Setup);
			yield return reference.SetLifecycle(Lifecycle.Active);
		}

		private IEnumerator CleanupFeature(FeatureReference reference)
		{
			Logger.Info("Cleaning up feature: " + reference.Type + "::" + reference.Name);

			yield return reference.SetLifecycle(Lifecycle.Cleanup);
			yield return reference.SetLifecycle(Lifecycle.Unload);
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

		private void StartAsync(IEnumerator routine, Action<Coroutine> callback)
		{
			callback(StartCoroutine(routine));
		}

		private void StopAsync(Coroutine routine)
		{
			StopCoroutine(routine);
		}

		private class FeatureReference
		{
			public bool Referenced
			{
				get { return _count > 0; }
			}

			public string Type
			{
				get { return _feature.Type; }
			}

			public string Name
			{
				get { return _feature.Name; }
			}

			public bool Running
			{
				get { return _lifecycle != Lifecycle.None; }
			}

			private uint _count;
			private Lifecycle _lifecycle;
			private Object _asset;

			private readonly List<Coroutine> _coroutines = new List<Coroutine>();
			private readonly Action<string> _notify;
			private readonly Action<IEnumerator, Action<Coroutine>> _startAsync;
			private readonly Action<Coroutine> _stopAsync;
			private readonly Feature _feature;
			private readonly Environment _environment;

			public FeatureReference(Feature impl, Environment env, Action<string> notify,
				Action<IEnumerator, Action<Coroutine>> startAsync, Action<Coroutine> stopAsync)
			{
				_stopAsync = stopAsync;
				_startAsync = startAsync;
				_environment = env;
				_notify = notify;
				_feature = impl;
			}

			public void AddReference()
			{
				_count++;
			}

			public void RemoveReference()
			{
				_count--;
			}

			public IEnumerator SetLifecycle(Lifecycle lifecycle)
			{
				switch (lifecycle)
				{
					case Lifecycle.Load:
						yield return Load();
						ProcessListeners(true);
						ProcessInjections(lifecycle, _lifecycle);
						break;
					case Lifecycle.Setup:
					case Lifecycle.Active:
					case Lifecycle.Cleanup:
						ProcessInjections(lifecycle, _lifecycle);
						break;
					case Lifecycle.Unload:
						ProcessInjections(lifecycle, _lifecycle);
						ProcessListeners(false);
						_coroutines.ForEach(_stopAsync);
						_asset = null;
						AssetBundles.UnloadBundle(_feature.Bundle, true);
						break;
				}

				_lifecycle = lifecycle;
			}

			private IEnumerator Load()
			{
				switch (_environment.Loading)
				{
					case LoadMethod.Baked:
						yield return AssetBundles.LoadBundle
						(
							_environment.GetPath(_feature.BundleFile),
							null,
							null,
							Logger.Exception
						);
						break;
					case LoadMethod.Online:
						yield return AssetBundles.LoadBundle
						(
							_environment.GetUri(_feature.BundleFile, true),
							_environment.GetVersion(_feature.Bundle),
							null,
							null,
							Logger.Exception
						);
						break;
				}

				yield return AssetBundles.LoadAssets(
					_feature.Bundle,
					FindType(_feature.Type),
					result => { _asset = result[0]; },
					null,
					Logger.Exception);
			}

			private void ProcessListeners(bool active)
			{
				foreach (MemberInfo member in GetMembers())
				{
					foreach (object attribute in member.GetCustomAttributes(true))
					{
						IFuseNotifier notifier = attribute as IFuseNotifier;
						if (notifier != null)
						{
							if (active)
								notifier.AddListener(_notify);
							else
								notifier.RemoveListener(_notify);
						}
					}
				}
			}

			private void ProcessInjections(Lifecycle toEnter, Lifecycle toExit)
			{
				List<Pair<IFuseAttribute, MemberInfo>> attributes = new List<Pair<IFuseAttribute, MemberInfo>>();
				foreach (MemberInfo member in GetMembers())
				{
					foreach (object custom in member.GetCustomAttributes(true))
					{
						IFuseAttribute attribute = custom as IFuseAttribute;
						if (attribute != null)
							attributes.Add(new Pair<IFuseAttribute, MemberInfo>(attribute, member));
					}
				}

				attributes.Sort((a, b) => a.A.Order < b.A.Order ? -1 : 1);

				foreach (Pair<IFuseAttribute, MemberInfo> attribute in attributes)
				{
					Lifecycle active = attribute.A.Lifecycle;
					if (active == Lifecycle.None)
					{
						object[] possibleDefaults = attribute.A.GetType().GetCustomAttributes(typeof(DefaultLifecycleAttribute), true);
						if (possibleDefaults.Length > 0)
						{
							DefaultLifecycleAttribute defaultLifecycle = (DefaultLifecycleAttribute) possibleDefaults[0];
							active = defaultLifecycle.Lifecycle;
						}
						else
							active = (Lifecycle) ((DefaultValueAttribute) active.GetType().GetCustomAttributes(true)[0]).Value;
					}

					if (toEnter == active)
					{
						IFuseExecutor executor = attribute.A as IFuseExecutor;
						if (executor != null)
							executor.Execute(attribute.B, _asset);

						IFuseExecutorAsync executorAsync = attribute.A as IFuseExecutorAsync;
						if (executorAsync != null)
							_startAsync(executorAsync.Execute(attribute.B, _asset), coroutine => { _coroutines.Add(coroutine); });
					}

					if (toEnter == active || toExit == active)
					{
						IFuseLifecycle lifecycle = attribute.A as IFuseLifecycle;
						if (lifecycle != null)
						{
							if (toEnter == active)
								lifecycle.OnEnter(attribute.B, _asset);
							else if (toExit == active)
								lifecycle.OnExit(attribute.B, _asset);
						}
					}
				}
			}

			private IEnumerable<MemberInfo> GetMembers()
			{
				return _asset.GetType().FindMembers(MemberTypes.All, Constants.FeatureFlags, null, null);
			}

			private static Type FindType(string qualifiedTypeName)
			{
				Type t = System.Type.GetType(qualifiedTypeName, false, true);
				if (t != null)
					return t;

				foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
				{
					t = asm.GetType(qualifiedTypeName);
					if (t != null)
						return t;
				}

				return null;
			}
		}

		private class SceneReference
		{
			public bool Active
			{
				get { return _loaded || _count > 0; }
			}

			private readonly string _path;
			private readonly Environment _environment;

			private uint _count;
			private bool _loaded;

			public SceneReference(string path, Environment environment)
			{
				_path = path;
				_environment = environment;
			}

			public void AddReference()
			{
				_count++;
			}

			public void RemoveReference()
			{
				_count--;
			}

			public IEnumerator Process()
			{
				if (_count == 0 && _loaded)
					yield return Unload();
				else if (_count > 0 && !_loaded)
					yield return Load();
			}

			private IEnumerator Load()
			{
				switch (_environment.Loading)
				{
					case LoadMethod.Baked:
						yield return AssetBundles.LoadBundle
						(
							_environment.GetPath(Constants.GetSceneBundleFileFromPath(_path)),
							null,
							null,
							Logger.Exception
						);
						break;
					case LoadMethod.Online:
						yield return AssetBundles.LoadBundle
						(
							_environment.GetUri(Constants.GetSceneBundleFileFromPath(_path), true),
							_environment.GetVersion(Constants.GetSceneBundleFromPath(_path)),
							null,
							null,
							Logger.Exception
						);
						break;
				}

				yield return SceneManager.LoadSceneAsync(Constants.GetFileNameFromPath(_path, Constants.SceneExtension),
					LoadSceneMode.Additive);

				_loaded = true;
				Logger.Info("Loaded scene: " + _path);
			}

			private IEnumerator Unload()
			{
				yield return SceneManager.UnloadSceneAsync(Constants.GetFileNameFromPath(_path, Constants.SceneExtension));
				AssetBundles.UnloadBundle(Constants.GetSceneBundleFromPath(_path), true);

				_loaded = false;
				Logger.Info("Unloaded scene: " + _path);
			}
		}

		private class StateTransition
		{
			private readonly List<string> _events;

			public bool Transition
			{
				get { return _events.Count == 0; }
			}

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
				return Transition;
			}
		}
	}
}