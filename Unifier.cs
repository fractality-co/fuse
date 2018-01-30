using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;

#endif

namespace uMVC
{
	/// <summary>
	/// Acts as the boot strap script for your application.
	/// Manages all resources in a single location.
	/// Handles states and their controllers, along with where to place views.
	/// </summary>
	public class Unifier : MonoBehaviour
	{
		private static readonly Dictionary<string, Model> LoadedModels = new Dictionary<string, Model>();
		private static readonly Dictionary<string, View> LoadedViews = new Dictionary<string, View>();

		[SerializeField] private string _initialState;
		[SerializeField] private List<State> _states;
		[SerializeField] private List<ContainerReference> _containers;

		private bool _quitting;
		private bool _transition;
		private State _current;
		private readonly List<Controller> _active = new List<Controller>();

		private void Awake()
		{
			Application.runInBackground = true;
		}

		private void Start()
		{
			ChangeState(_initialState);
		}

		private void OnApplicationQuit()
		{
			try
			{
				_quitting = true;

				foreach (Controller controller in _active)
					controller.Unload();

				foreach (KeyValuePair<string, Model> pair in LoadedModels)
					pair.Value.Unload();

				foreach (KeyValuePair<string, View> pair in LoadedViews)
					pair.Value.Unload();
			}
			catch (Exception)
			{
				// since our application is quitting, we don't care about handling it cleanly
			}
		}

		private void OnDestroy()
		{
			_active.ForEach(controller => controller.Unload());
			_active.Clear();
		}

		public IEnumerator LoadModel<T>(Action<T> onComplete, Action<float> onProgress = null) where T : Model
		{
			string modelName = typeof(T).Name;
			if (LoadedModels.ContainsKey(modelName))
			{
				if (onProgress != null) onProgress(1f);
				onComplete(LoadedModels[modelName] as T);
				yield break;
			}

			T model = ScriptableObject.CreateInstance<T>();
			LoadedModels[modelName] = model;

			yield return model.Load();

			onComplete(model);
		}

		public IEnumerator LoadModel<T>(string path, Action<T> onComplete, Action<float> onProgress = null) where T : Model
		{
			string modelName = typeof(T).Name;
			if (LoadedModels.ContainsKey(modelName))
			{
				if (onProgress != null) onProgress(1f);
				onComplete(LoadedModels[modelName] as T);
				yield break;
			}

			T model = null;
			yield return LoadingAsset<T>
			(
				path,
				result => { model = result; },
				onProgress
			);

			model = model ?? ScriptableObject.CreateInstance<T>();
			LoadedModels[modelName] = model;

			yield return model.Load();

			onComplete(model);
		}

		public bool UnloadModel<T>(string path) where T : Model
		{
			if (!LoadedModels.ContainsKey(path))
			{
				if(!_quitting)
					Debug.LogError("[uMVC] Trying to unload model but we have no occurence of it!");

				return false;
			}

			UnloadModel(LoadedModels[path] as T);
			return true;
		}

		public bool UnloadModel<T>(T model) where T : Model
		{
			string referenceKey = string.Empty;
			foreach (KeyValuePair<string, Model> pair in LoadedModels)
			{
				if (pair.Value != model) continue;
				referenceKey = pair.Key;
				break;
			}

			if (referenceKey == string.Empty)
			{
				if(!_quitting)
					Debug.LogError("[uMVC] Trying to unload model but we have no occurence of it!");

				return false;
			}

			model.Unload();
			LoadedModels.Remove(referenceKey);
			return true;
		}

		public IEnumerator LoadView<T>(string path, Action<T> onComplete, Action<float> onProgress = null) where T : View
		{
			if (LoadedViews.ContainsKey(path))
			{
				if (onProgress != null) onProgress(1f);
				onComplete(LoadedViews[path] as T);
				yield break;
			}

			GameObject viewAsset = null;
			yield return LoadingAsset<GameObject>
			(
				path,
				asset => { viewAsset = asset; },
				onProgress
			);

			T view = Instantiate(viewAsset).GetComponent<T>();
			view.transform.SetParent(GetContainer(view.ContainerId), false);
			view.gameObject.name = view.gameObject.name.Replace("(Clone)", string.Empty);
			LoadedViews[path] = view;

			yield return view.Load();

			onComplete(view);
		}

		public bool UnloadView<T>(string path) where T : View
		{
			if (!LoadedViews.ContainsKey(path))
			{
				if(!_quitting)
					Debug.LogError("[uMVC] Trying to unload view that has not been loaded yet!");

				return false;
			}

			UnloadView(LoadedViews[path] as T);
			return true;
		}

		public bool UnloadView<T>(T view) where T : View
		{
			KeyValuePair<string, View>? reference = null;
			foreach (KeyValuePair<string, View> pair in LoadedViews)
			{
				if (pair.Value != view) continue;
				reference = pair;
				break;
			}

			if (reference == null)
			{
				if(!_quitting)
					Debug.LogError("[uMVC] Trying to unload model but we have no occurence of it!");

				return false;
			}

			view.Unload();
			LoadedViews.Remove(reference.Value.Key);
			return true;
		}

		public void ChangeState(string state, bool clearAllControllers = false)
		{
			StartCoroutine(ChangingState(state, clearAllControllers));
		}

		private IEnumerator ChangingState(string state, bool clearAllControllers = false)
		{
			yield return new WaitUntil(() => _transition == false);

			_transition = true;

			State next = _states.Find(current => current.Name == state);
			if (next == null)
			{
				Debug.LogError("[uMVC] Unable to find valid state for: " + state + ". Check States within Unifier.");
#if UNITY_EDITOR
				EditorApplication.isPlaying = false;
#else
				Application.Quit();
#endif
				yield break;
			}

			if (_current != null)
			{
				Debug.Log("[uMVC] Leaving State: " + _current.Name);

				for (int i = _active.Count - 1; i >= 0; i--)
				{
					Controller active = _active[i];

					// if our controller is assigned to the next state, don't clean it up
					if (!clearAllControllers && next.Controllers.Any(current => current == active.GetType().Name))
						continue;

					active.Unload();
					_active.RemoveAt(i);
				}

				yield return null;

				Resources.UnloadUnusedAssets();
				GC.Collect();

				yield return null;
			}

			_current = next;
			foreach (string controller in _current.Controllers)
			{
				Type type = Type.GetType(controller);
				if (type == null)
					throw new Exception("[uMVC] Invalid Controller type passed: " + controller);

				_active.Add((Controller) Activator.CreateInstance(type));
			}

			foreach (Controller controller in _active)
				yield return controller.Load();

			Debug.Log("[uMVC] Entering State: " + _current.Name);
			_transition = false;
		}

		private Transform GetContainer(string id)
		{
			var result = _containers.Find(current => current.Id == id);
			return result != null ? result.Container.transform : null;
		}

		private void LoadAsset<T>(string path, Action<T> onComplete, Action<float> onProgress = null) where T : Object
		{
			StartCoroutine(LoadingAsset(path, onComplete, onProgress));
		}

		private static IEnumerator LoadingAsset<T>(string path, Action<T> onComplete, Action<float> onProgress = null)
			where T : Object
		{
			ResourceRequest request = Resources.LoadAsync<T>(path);
			while (!request.isDone)
			{
				yield return request;

				if (onProgress != null)
					onProgress(request.progress);
			}
			onComplete(request.asset as T);
		}
	}

	[Serializable]
	public class State
	{
		[SerializeField] private string _name;

		[TypeReference(typeof(Controller)), SerializeField] private string[] _controllers;

		public string Name
		{
			get { return _name; }
		}

		public IEnumerable<string> Controllers
		{
			get { return _controllers; }
		}
	}

	[Serializable]
	public class ContainerReference
	{
		public enum ViewType
		{
			UserInterface,
			World
		}

#pragma warning disable 649
		[SerializeField] private string _id;

		[SerializeField] private ViewType _type;

		[SerializeField] private GameObject _container;
#pragma warning restore 649

		public string Id
		{
			get { return _id; }
		}

		public ViewType Type
		{
			get { return _type; }
		}

		public Canvas Canvas
		{
			get { return _type == ViewType.UserInterface ? _container.GetComponent<Canvas>() : null; }
		}

		public GameObject Container
		{
			get { return _container; }
		}
	}

	public class TypeReference : PropertyAttribute
	{
		public Type BaseType { get; private set; }

		public TypeReference(Type baseType)
		{
			BaseType = baseType;
		}
	}

#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(TypeReference))]
	public class TypeReferencePropertyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, GUIContent.none, property);

			position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
			TypeReference reference = (TypeReference) attribute;

			List<string> options = GetSubclassTypes(reference.BaseType);
			if (options.Count > 0)
			{
				EditorGUI.BeginChangeCheck();

				int typeIndex = options.IndexOf(property.stringValue);
				typeIndex = EditorGUI.Popup(position, typeIndex, options.ToArray());

				if (EditorGUI.EndChangeCheck())
				{
					property.stringValue = options[typeIndex];
				}
			}
			else
			{
				EditorGUI.LabelField(position, "No " + reference.BaseType + " Types Found");
			}

			EditorGUI.EndProperty();
		}

		private List<string> GetSubclassTypes(Type baseType)
		{
			List<string> result = new List<string>();
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				foreach (Type type in assembly.GetTypes())
				{
					if (type.IsSubclassOf(baseType) && !type.IsAbstract)
					{
						result.Add(type.Name);
					}
				}
			}
			return result;
		}
	}
#endif
}