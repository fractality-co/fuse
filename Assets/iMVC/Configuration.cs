using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace iMVC
{
	public class Configuration : ScriptableObject
	{
		public const string AssetName = "iMVC";
		public const string FullAssetName = AssetName + ".asset";
		public const string RootConfigurationPath = "Assets/Resources";
		public const string FullConfigurationPath = RootConfigurationPath + "/" + FullAssetName;
		public const string LoadConfigurationPath = "iMVC";
		public const string FullScriptsPath = "Assets/Scripts";

		private static Configuration _instance;

		public string FullLoadPath
		{
			get { return "Assets/" + LoadPath; }
		}

		public string LoadPath
		{
			get
			{
				switch (_loadMethod)
				{
					case ImplementationLoadMethod.Resources:
						return "Resources";

					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		public List<State> States
		{
			get { return _states; }
		}

		[Header("State Machine")]
		[StateReference, SerializeField]
		private string _start;

		[SerializeField]
		private List<State> _states;

		[Header("Implementation Assets")]
		[SerializeField]
		private ImplementationLoadMethod _loadMethod = ImplementationLoadMethod.Resources;

		public static Configuration Load()
		{
			if (_instance == null)
			{
#if UNITY_EDITOR
				_instance = AssetDatabase.LoadAssetAtPath<Configuration>(FullConfigurationPath);
#else
				_instance = Resources.Load<Configuration>(LoadConfigurationPath);
#endif
			}

			return _instance;
		}
	}

	internal enum ImplementationLoadMethod
	{
		Resources
	}

	[Serializable]
	public class State
	{
		[SerializeField]
		public string Name;

		[AttributeTypeReference(typeof(ControllerAttribute)), SerializeField]
		public string[] Controllers;
	}

	internal sealed class StateReference : PropertyAttribute
	{
	}

	internal sealed class AttributeTypeReference : PropertyAttribute
	{
		public readonly Type BaseType;

		public AttributeTypeReference(Type baseType)
		{
			BaseType = baseType;
		}
	}

#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(StateReference))]
	internal class StateReferencePropertyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, GUIContent.none, property);

			position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

			List<State> options = Configuration.Load().States;
			if (options.Count > 0)
			{
				EditorGUI.BeginChangeCheck();

				int index = options.FindIndex(current => current.Name == property.stringValue);
				index = EditorGUI.Popup(position, index, options.Select(state => state.Name).ToArray());

				if (EditorGUI.EndChangeCheck())
					property.stringValue = options[index].Name;
			}
			else
			{
				EditorGUI.LabelField(position, "No States Found");
			}

			EditorGUI.EndProperty();
		}
	}

	[CustomPropertyDrawer(typeof(AttributeTypeReference))]
	internal class TypeReferencePropertyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, GUIContent.none, property);

			position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
			AttributeTypeReference reference = (AttributeTypeReference) attribute;

			List<string> options = GetImplementations(reference.BaseType);
			if (options.Count > 0)
			{
				EditorGUI.BeginChangeCheck();

				int typeIndex = options.IndexOf(property.stringValue);
				typeIndex = EditorGUI.Popup(position, typeIndex, options.ToArray());

				if (EditorGUI.EndChangeCheck())
					property.stringValue = options[typeIndex];
			}
			else
			{
				EditorGUI.LabelField(position, "No Implementations Found");
			}

			EditorGUI.EndProperty();
		}

		private List<string> GetImplementations(Type attributeType)
		{
			List<string> result = new List<string>();

			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (Type type in assembly.GetTypes())
				{
					if (!type.IsAbstract && type.GetCustomAttributes(attributeType, true).Length > 0)
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