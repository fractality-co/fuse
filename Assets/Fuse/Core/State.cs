using System;
using System.Collections.Generic;
using System.Linq;
using Fuse.Implementation;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace Fuse.Core
{
	/// <summary>
	/// Each state operates as a state machine and a state.
	/// Data defined here controls how <see cref="Fuse"/> should handle implementations.
	/// </summary>
	public class State : ScriptableObject
	{
		public bool IsRoot
		{
			get { return string.IsNullOrEmpty(Parent); }
		}

		[AssetReference(typeof(State))]
		public string Parent;

		public Transition[] Transitions;

		[AssetReference(typeof(ScriptableObject), typeof(ImplementationAttribute))]
		public string[] Implementations;
	}

	[Serializable]
	public class Implementation
	{
		public string BundleFile
		{
			get { return Bundle + Constants.BundleExtension; }
		}

		public string Bundle
		{
			get { return Type.ToLower().Trim(); }
		}

		public readonly string Type;
		public readonly string Name;

		public Implementation(string type, string name)
		{
			Type = type;
			Name = name;
		}

		public Implementation(string path)
		{
			string[] parts = path.Split(Constants.DefaultSeparator);
			Name = parts[parts.Length - 1].Replace(Constants.AssetExtension, string.Empty);
			Type = parts[parts.Length - 2];
		}
	}

	[Serializable]
	public class Transition
	{
		[AssetReference(typeof(State))]
		public string To;

		public string[] Events;
	}

	public sealed class AssetBundleReference : PropertyAttribute
	{
	}

	public sealed class AssetReference : PropertyAttribute
	{
		public readonly Type Type;
		public readonly Type RequiredAttribute;

		public AssetReference(Type type, Type requiredAttribute = null)
		{
			Type = type;
			RequiredAttribute = requiredAttribute;
		}
	}

#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(AssetReference))]
	public class AssetReferencePropertyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			position = EditorGUI.PrefixLabel(position, label);

			AssetReference reference = (AssetReference) attribute;

			if (!reference.Type.IsSubclassOf(typeof(Object)))
			{
				EditorGUI.LabelField(position, "Invalid assigned type on AssetReference.");
				return;
			}

			string assetPath = property.stringValue;
			Object asset = null;
			if (!string.IsNullOrEmpty(assetPath))
				asset = AssetDatabase.LoadAssetAtPath(assetPath, reference.Type);

			EditorGUI.BeginChangeCheck();
			asset = EditorGUI.ObjectField(position, asset, reference.Type, false);
			if (EditorGUI.EndChangeCheck())
			{
				if (reference.RequiredAttribute != null &&
				    asset.GetType().GetCustomAttributes(reference.RequiredAttribute, true).Length == 0)
				{
					Logger.Warn("Invalid assignment. Requires a attribute of: " + reference.RequiredAttribute);
					return;
				}

				property.stringValue = AssetDatabase.GetAssetPath(asset);
			}
		}
	}

	[CustomPropertyDrawer(typeof(AssetBundleReference))]
	internal class AssetBundleReferencePropertyDrawer : PropertyDrawer
	{
		private static List<string> _options;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, GUIContent.none, property);

			position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

			List<string> options = AssetDatabase.GetAllAssetBundleNames().ToList();
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
	}
#endif
}