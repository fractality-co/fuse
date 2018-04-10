using System;
using System.Collections.Generic;
using System.Linq;
using Fuse.Feature;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace Fuse.Core
{
	/// <summary>
	/// Each state operates as a state machine and a state.
	/// Data defined here controls how <see cref="Fuse"/> should handle features.
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

		[AssetReference(typeof(ScriptableObject), typeof(FeatureAttribute))]
		public string[] Features;

#if UNITY_EDITOR
		[AssetReference(typeof(SceneAsset), Constants.ScenesAssetPath)]
#endif
		public string[] Scenes;
		
#if UNITY_EDITOR
		[AssetReference(typeof(SceneAsset), Constants.ScenesAssetPath)]
#endif
		[Tooltip("If specified, we will add and remove this scene when transitioning and loading to a state.")]
		public string Loading;
	}

	[Serializable]
	public class Feature
	{
		public string BundleFile
		{
			get { return string.Format(Constants.FeatureBundleFile, Type.ToLower().Trim()); }
		}

		public string Bundle
		{
			get { return string.Format(Constants.FeatureBundle, Type.ToLower().Trim()); }
		}

		public readonly string Type;
		public readonly string Name;

		public Feature(string path)
		{
			string[] parts = path.Split(Constants.DefaultSeparator);
			Name = parts[parts.Length - 1].Replace(Constants.AssetExtension, string.Empty);
			Type = parts[Constants.FeatureFolderDepth - 1];
		}
	}

	[Serializable]
	public class Transition
	{
		[AssetReference(typeof(State))]
		public string To;

		[UsedImplicitly]
		public string[] Events;
	}

	public sealed class AssetBundleReference : PropertyAttribute
	{
	}

	public sealed class AssetReference : PropertyAttribute
	{
		public readonly Type Type;
		public readonly Type RequiredAttribute;
		public readonly string RequiredSubpath;

		public AssetReference(Type type)
		{
			Type = type;
		}

		public AssetReference(Type type, Type requiredAttribute)
		{
			Type = type;
			RequiredAttribute = requiredAttribute;
		}

		public AssetReference(Type type, string requiredSubpath)
		{
			Type = type;
			RequiredSubpath = requiredSubpath;
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

				assetPath = AssetDatabase.GetAssetPath(asset);
				if (!string.IsNullOrEmpty(assetPath) && !string.IsNullOrEmpty(reference.RequiredSubpath) &&
				    !assetPath.Contains(reference.RequiredSubpath))
				{
					Logger.Warn("Asset assigned does not meet required subpath: " + reference.RequiredSubpath);
					return;
				}

				property.stringValue = assetPath;
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