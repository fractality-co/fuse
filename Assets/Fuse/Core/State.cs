using System;
using System.Collections.Generic;
using System.Reflection;
using Fuse.Implementation;
using UnityEditor;
using UnityEngine;

namespace Fuse.Core
{
	/// <summary>
	/// Each state operates as a state machine and a state.
	/// Data defined here controls how <see cref="Executor"/> should handle implementations.
	/// </summary>
	public class State : ScriptableObject
	{
		public bool IsRoot
		{
			get { return string.IsNullOrEmpty(Parent); }
		}

		[StateReference, Tooltip("Optionally, you can make this state a sub-state to another existing one.")]
		public string Parent;

		public Transition[] Transitions;

		public Implementation[] Implementations;
	}

	[Serializable]
	public class Implementation
	{
		public string Bundle
		{
			get { return Type.ToLower().Trim(); }
		}

		[AttributeTypeReference(typeof(ImplementationAttribute))]
		public string Type;

		public string Name;

		public Implementation(string type, string name)
		{
			Type = type;
			Name = name;
		}
	}

	[Serializable]
	public class Transition
	{
		[StateReference] public string To;

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

			List<string> options = GetTypes(reference.BaseType);
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

		private static List<string> GetTypes(Type attributeType)
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

	[CustomPropertyDrawer(typeof(Implementation))]
	internal class ImplementationReferencePropertyDrawer : PropertyDrawer
	{
		private static List<string> _options;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, GUIContent.none, property);

			position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

			List<string> options = GetImplementations(property.serializedObject.FindProperty("Type").stringValue);
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

		private static List<string> GetImplementations(string type)
		{
			if (_options == null)
			{
				_options = new List<string>();
				foreach (string guid in AssetDatabase.FindAssets(string.Format("t:{0}", type)))
				{
					string[] assetPath = AssetDatabase.GUIDToAssetPath(guid).Split('/');
					_options.Add(assetPath[assetPath.Length - 1]);
				}
			}

			return _options;
		}
	}
#endif
}