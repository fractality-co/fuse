using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace uMVC
{
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