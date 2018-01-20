using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
#endif

namespace uMVC
{
	/// <summary>
	/// Acts as the boot strap script for your application.
	/// Handles states and their controllers, along with where to place views.
	/// </summary>
	public class Unifier : MonoBehaviour
	{
		[SerializeField] private string _initialState;
		[SerializeField] private List<State> _states;
		[SerializeField] private List<ContainerReference> _containers;

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

		private void OnDestroy()
		{
			_active.ForEach(controller => controller.Cleanup());
			_active.Clear();
		}

		public void ChangeState(string state)
		{
			StartCoroutine(ChangingState(state));
		}

		private IEnumerator ChangingState(string state)
		{
			if (_current != null)
			{
				Debug.Log("Leaving State: " + _current.Name);

				_active.ForEach(controller => controller.Cleanup());
				_active.Clear();

				yield return null;

				Resources.UnloadUnusedAssets();
				GC.Collect();

				yield return null;
			}

			_current = _states.Find(current => current.Name == state);
			foreach (string controller in _current.Controllers)
			{
				Type type = Type.GetType(controller);
				if (type == null)
					throw new Exception("Invalid Controller type passed: " + controller);

				_active.Add((Controller) Activator.CreateInstance(type));
			}

			foreach (Controller controller in _active)
				yield return controller.Setup();

			Debug.Log("Entering State: " + _current.Name);
		}

		public Transform GetContainer(string id)
		{
			var result = _containers.Find(current => current.Id == id);
			return result != null ? result.Container.transform : null;
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

		public string[] Controllers
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
					if (type.IsSubclassOf(baseType))
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