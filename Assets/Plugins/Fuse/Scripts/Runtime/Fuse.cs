using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Fuse
{
	/// <summary>
	/// Executor for the framework.
	/// Don't interact with this.
	/// </summary>
	public class Fuse : SingletonBehaviour<Fuse>
	{
		#region Variables

		private const int DefaultLoaderSortingOrder = 1000;

		public Sprite fuseLogo;

		private readonly List<State> _states = new List<State>();
		private readonly List<TransitionReference> _transitions = new List<TransitionReference>();
		private readonly Dictionary<string, Instance> _instances = new Dictionary<string, Instance>();
		private Configuration _configuration;
		private Environment _environment;
		private Loader _loader;
		private State _state;
		private bool _transitioning;

		#endregion

		#region Bootstrap

		private IEnumerator Start()
		{
			Screen.sleepTimeout = -1; // turn off sleep timeout while in app

			Logger.Enabled = Debug.isDebugBuild;
			Logger.Info("Starting ...");

			var success = false;
			var failure = false;
			while (!success)
			{
				yield return new WaitForSeconds(0.25f);
				failure = false;

				var localPath = string.Format(Constants.AssetsBakedPath, Constants.CoreBundleFile);
				yield return Bundles.LoadLocalBundle(localPath, null, null, msg =>
				{
					Logger.Error(msg);
					failure = true;
				});
				if (failure) continue;
				Logger.Info("Loaded core content");

				yield return Bundles.LoadAsset<Configuration>(
					Constants.GetConfigurationAssetPath(),
					result => { _configuration = result; },
					null,
					msg =>
					{
						Logger.Error(msg);
						failure = true;
					});
				if (failure) continue;
				Logger.Info("Loaded core configuration via baked");

				// ReSharper disable once RedundantLogicalConditionalExpressionOperand
				if (_configuration.Loader != null)
				{
					_loader = Instantiate(_configuration.Loader);
					yield return _loader.Show();
				}
				else
				{
					if (fuseLogo == null)
					{
						Logger.Error(
							"Plugin validation unsuccessful. Restart Unity, then open the scene with the Fuse script and select it in the inspector then try again. " +
							"If you have tampered with the plugin, please contact us (fuse@fractality.co) if you are need of assistance or unable to afford a license!"
						);
						yield break;
					}

					_loader = BuildDefault(fuseLogo);
					yield return _loader.Show();
				}

				_loader.Progress(0.1f);
				_loader.Step("Fetching environment");

				yield return Bundles.LoadAsset<Environment>(
					_configuration.Environment,
					result => { _environment = result; },
					null,
					msg =>
					{
						Logger.Error(msg);
						failure = true;
					});
				if (failure) continue;
				Logger.Info("Loaded core environment via baked");

				_loader.Step("Retrieving content");
				switch (_environment.loading)
				{
					case LoadMethod.Baked:
						// we've already loaded baked
						yield return new WaitForSeconds(0.25f);
						Logger.Info("Loaded core environment via baked");
						break;
					case LoadMethod.Online:
						yield return Bundles.UnloadBundle(Constants.CoreBundle, false);
						yield return Bundles.LoadBundle(_environment, Constants.CoreBundleFile, null, null,
							msg =>
							{
								Logger.Error(msg);
								failure = true;
							});
						if (failure) continue;
						Logger.Info("Updated core content via online");

						yield return Bundles.LoadAsset<Configuration>(
							Constants.GetConfigurationAssetPath(),
							result => { _configuration = result; },
							_loader.Progress,
							msg =>
							{
								Logger.Error(msg);
								failure = true;
							});
						if (failure) continue;
						Logger.Info("Updated configuration via online");

						yield return Bundles.LoadAsset<Environment>(
							_configuration.Environment,
							result => { _environment = result; },
							null,
							msg =>
							{
								Logger.Error(msg);
								failure = true;
							});
						if (failure) continue;
						Logger.Info("Updated environment via online");
						break;
					default:
						Logger.Error("Unknown LoadMethod assigned");
						continue;
				}

				_loader.Progress(0.2f);
				_loader.Step("Fetching states");

				yield return Bundles.LoadAssets<State>(
					Constants.CoreBundle,
					result =>
					{
						_states.Clear();
						_states.AddRange(result);
					},
					_loader.Progress,
					msg =>
					{
						Logger.Error(msg);
						failure = true;
					});
				if (failure) continue;
				Logger.Info("Loaded all configured states");

				_loader.Step("Preloading dependencies");
				_loader.Progress(0.3f);

				Logger.Info("Preloading global dependencies ...");
				yield return PreloadGlobal();
				_loader.Progress(0.4f);

				Logger.Info("Preloading all dependencies referenced in state machine ...");
				foreach (var state in _states)
					yield return Preload(state);

				_loader.Step("Loading global content");
				_loader.Progress(0.5f);

				yield return LoadAll<ContentInstance>(_configuration.Content, new string[] { });
				Logger.Info("Loaded global scenes");
				_loader.Progress(0.6f);

				_loader.Step("Loaded content");
				yield return LoadAll<SceneInstance>(_configuration.Scenes, new string[] { });
				Logger.Info("Started global scenes");
				_loader.Progress(0.7f);

				_loader.Step("Starting modules");
				yield return LoadAll<ModuleInstance>(_configuration.Modules, new string[] { });
				Logger.Info("Started global modules");

				_loader.Step("Initializing state");
				_loader.Progress(0.85f);

				var startState = GetState(_configuration.Start);
				if (startState == null)
				{
					Logger.Exception("You must assign a valid state to FUSE configuration");
					yield break;
				}

				Events.SubscribeAll(OnEventTransition);
				yield return SetState(startState);
				_loader.Progress(1f);
				_loader.Step("Complete");

				if (_loader != null)
				{
					yield return _loader.Hide();
					Destroy(_loader.gameObject);
					_loader = null;
				}

				success = true;
			}

			Logger.Info("Started");
		}

		private IEnumerator Preload(State state)
		{
			foreach (var scene in state.Scenes)
				yield return Preload(Constants.GetSceneBundleFileFromPath(scene));

			foreach (var content in state.Content)
				yield return Preload(Constants.GetContentBundleFileFromPath(content));

			Logger.Info($"Preloaded state [{state.name}]");
		}

		private IEnumerator PreloadGlobal()
		{
			foreach (var scene in _configuration.Scenes)
				yield return Preload(Constants.GetSceneBundleFileFromPath(scene));

			foreach (var content in _configuration.Content)
				yield return Preload(Constants.GetContentBundleFileFromPath(content));

			Logger.Info($"Preloaded global");
		}

		private IEnumerator Preload(string bundle)
		{
			var first = true;
			var success = false;
			while (!success)
			{
				if (!first)
					yield return new WaitForSeconds(1f);

				yield return Bundles.LoadBundle(_environment, bundle, result => { success = true; }, null, Logger.Error);
				first = false;
			}

			Logger.Info($"Preloaded ({_environment.loading}) bundle: " + bundle);
		}

		private static Loader BuildDefault(Sprite sprite)
		{
			var loader = new GameObject("Loader");

			var canvas = loader.AddComponent<Canvas>();
			canvas.overrideSorting = true;
			canvas.sortingOrder = DefaultLoaderSortingOrder;
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;

			var background = new GameObject("Background");
			background.transform.SetParent(loader.transform);
			var backgroundRawImage = background.AddComponent<RawImage>();
			backgroundRawImage.color = Color.black;
			backgroundRawImage.rectTransform.anchoredPosition = Vector2.zero;
			backgroundRawImage.rectTransform.sizeDelta = new Vector2(Screen.width, Screen.height);

			var logo = new GameObject("Logo");
			logo.transform.SetParent(loader.transform);
			var logoImage = logo.AddComponent<Image>();
			logoImage.sprite = sprite;
			logoImage.rectTransform.anchoredPosition = Vector2.zero;
			logoImage.rectTransform.sizeDelta = new Vector2(512, 512);

			var canvasGroup = loader.AddComponent<CanvasGroup>();
			canvasGroup.alpha = 0;

			var canvasGroupLoader = loader.AddComponent<CanvasGroupLoader>();
			return canvasGroupLoader;
		}

		#endregion

		#region State

		private IEnumerator SetState(State state)
		{
			if (_transitioning)
			{
				Logger.Warn(
					$"Attempting to transition to a state: {state}, but we are already in an active transition.");
				yield break;
			}

			_transitioning = true;
			if (state == null)
			{
				Logger.Exception("You must have a valid state assigned " + nameof(Configuration));
				yield break;
			}

			if (_state != null)
			{
				yield return UnloadAll<ModuleInstance>(_state.Modules, state.Modules);
				yield return UnloadAll<ContentInstance>(_state.Content, state.Content);
				yield return UnloadAll<SceneInstance>(_state.Scenes, state.Scenes);
			}

			var previous = _state;
			_state = state;

			_transitions.Clear();
			foreach (var transition in _state.Transitions)
				_transitions.Add(new TransitionReference(transition));

			var hasPrevious = previous != null;
			yield return LoadAll<SceneInstance>(_state.Scenes, !hasPrevious ? new string[] { } : previous.Scenes);
			yield return LoadAll<ContentInstance>(_state.Content, !hasPrevious ? new string[] { } : previous.Content);
			yield return LoadAll<ModuleInstance>(_state.Modules, !hasPrevious ? new string[] { } : previous.Modules);

			_transitioning = false;
			Logger.Info("Set state: " + _state);
		}

		private State GetState(string state)
		{
			var stateName = Path.GetFileNameWithoutExtension(state);
			return _states.Find(current => current.name == stateName);
		}

		private void OnEventTransition(string id, EventArgs args)
		{
			foreach (var transition in _transitions)
			{
				if (transition.Transition || !transition.ProcessEvent(id))
					continue;

				var transitionState = GetState(transition.State);
				if (transitionState == null)
				{
					Logger.Warn(
						$"Unable to find state ({transition.State}) to transition even though it was listening to event");
					continue;
				}

				StartCoroutine(SetState(transitionState));
				break;
			}
		}

		#endregion

		#region Instances

		private IEnumerator LoadAll<T>(IEnumerable<string> ids, string[] previous) where T : Instance
		{
			foreach (var id in ids)
				yield return Load<T>(id, previous);
		}

		private IEnumerator Load<T>(string id, IEnumerable<string> previous) where T : Instance
		{
			if (previous.Contains(id))
				yield break;

			if (string.IsNullOrEmpty(id))
			{
				Logger.Warn($"Attempting to load an empty {nameof(T)} ID");
				yield break;
			}

			var instance = Get<T>(id);
			if (instance == null)
			{
				instance = Set<T>(id);
				if (instance == null)
				{
					Logger.Error("Unable to build instance for: " + nameof(T) + " with id " + id);
					yield break;
				}
			}

			yield return instance.AddReference();
		}

		private IEnumerator UnloadAll<T>(IEnumerable<string> ids, string[] next) where T : Instance
		{
			foreach (var id in ids)
				yield return Unload<T>(id, next);
		}

		private IEnumerator Unload<T>(string id, IEnumerable<string> next) where T : Instance
		{
			if (next.Contains(id))
				yield break; // if used in next state, just ignore to cancel out

			if (string.IsNullOrEmpty(id))
			{
				Logger.Warn($"Attempting to unload an {nameof(T)} with empty ID");
				yield break;
			}

			var instance = Get<T>(id);
			if (instance == null)
			{
				Logger.Error("Unable to locate instance to unload: " + id);
				yield break;
			}

			yield return instance.RemoveReference();
		}

		private T Set<T>(string id) where T : Instance
		{
			var instance = (T)Activator.CreateInstance(typeof(T), id, _environment);
			_instances.Add(id, instance);
			return instance;
		}

		private T Get<T>(string id) where T : Instance
		{
			if (!_instances.ContainsKey(id))
				return default;

			return _instances[id] as T;
		}

		#endregion
	}
}