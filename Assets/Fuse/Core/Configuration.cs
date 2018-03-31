using System;
using System.Collections.Generic;
using System.Linq;
using Fuse.Implementation;
using JetBrains.Annotations;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

// ReSharper disable UnassignedField.Global
// ReSharper disable NotAccessedField.Global
// ReSharper disable UnusedMember.Global
namespace Fuse.Core
{
	/// <summary>
	/// Data relating to your applications functionality then executed by <see cref="Fuse"/>.
	/// </summary>
	public class Configuration : ScriptableObject
	{
		[AssetReference(typeof(State))] public string Start;
		public Loading Core;
		public Loading Implementations;
		public CustomVersion[] CustomVersions;

		public string GetAssetPath(Implementation implementation)
		{
			return Implementations.GetPath(implementation.Bundle + Constants.BundleExtension);
		}

		public Uri GetAssetUri(Implementation implementation)
		{
			return Implementations.GetUri(implementation.Bundle + Constants.BundleExtension);
		}

		public uint GetAssetVersion(Implementation implementation)
		{
			foreach (CustomVersion custom in CustomVersions)
				if (custom.Implementation == implementation.Type)
					return custom.Version;

			return Implementations.Version;
		}
	}

	public enum LoadMethod
	{
		Baked,
		Online
	}

	[Serializable]
	public class Hosting
	{
		public string Develop;
		public string Release;
	}

	[Serializable]
	public class CustomVersion
	{
		[AttributeTypeReference(typeof(ImplementationAttribute))]
		public string Implementation;

		public uint Version;
	}

	[Serializable]
	public class Loading
	{
		public LoadMethod Load;
		public uint Version;

		[SerializeField, UsedImplicitly] private Hosting _host;

		public string GetPath(string asset)
		{
			return string.Format(Constants.AssetsBakedPath, asset);
		}

		public Uri GetUri(string asset)
		{
#if RELEASE
			return new Uri(new Uri(_host.Release), new Uri(asset));
#else
			return new Uri(new Uri(_host.Develop), new Uri(asset));
#endif
		}
	}

	public sealed class StateReference : PropertyAttribute
	{
	}

#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(StateReference))]
	internal class StateReferencePropertyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, GUIContent.none, property);

			position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

			List<string> options = GetStates();
			options.Add(State.None);

			if (options.Count > 0)
			{
				EditorGUI.BeginChangeCheck();

				int index = options.FindIndex(current => current == property.stringValue);
				index = EditorGUI.Popup(position, index, options.ToArray());

				if (EditorGUI.EndChangeCheck())
				{
					string value = options[index];
					property.stringValue = string.IsNullOrEmpty(value) ? State.None : value;
				}
			}
			else
			{
				EditorGUI.Popup(position, 0, new[] {string.Empty});
			}

			EditorGUI.EndProperty();
		}

		private List<string> GetStates()
		{
			return Resources.FindObjectsOfTypeAll<State>().Select(source => source.name).ToList();
		}
	}
#endif
}