using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace iMVC
{
	[CreateAssetMenu(fileName = "State.asset", menuName = "iMVC/Create/State")]
	public class State : ScriptableObject
	{
		public string Name;
		public Transition[] Transitions;

		[AttributeTypeReference(typeof(ControllerAttribute))]
		public string[] Controllers;
	}

	[Serializable]
	public class Transition
	{
		[StateReference]
		public string To;

		public string[] Events;
	}

	public sealed class AttributeTypeReference : PropertyAttribute
	{
		public readonly Type BaseType;

		public AttributeTypeReference(Type baseType)
		{
			BaseType = baseType;
		}
	}

#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(AttributeTypeReference))]
	internal class TypeReferencePropertyDrawer : PropertyDrawer
	{
		private static List<string> _options;

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
				EditorGUI.Popup(position, 0, new[] {string.Empty});
			}

			EditorGUI.EndProperty();
		}

		private static List<string> GetImplementations(Type attributeType)
		{
			if (_options == null)
			{
				_options = new List<string>();
				foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
				{
					foreach (Type type in assembly.GetTypes())
					{
						if (!type.IsAbstract && type.GetCustomAttributes(attributeType, true).Length > 0)
						{
							_options.Add(type.Name);
						}
					}
				}
			}

			return _options;
		}
	}
#endif
}