using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
	/// Data relating to your applications functionality then executed by <see cref="Executor"/>.
	/// </summary>
	public class Configuration : ScriptableObject
	{
		[StateReference] public string Start;
		public Loading LoadImplementations; // TODO: more control over individual implementation / versioning
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
	public class Loading
	{
		public LoadMethod Load;
		public uint Version;

		[SerializeField, UsedImplicitly] private Hosting _host;

		public string GetPath(string asset)
		{
			return Application.streamingAssetsPath + Path.DirectorySeparatorChar + asset;
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
			options = options.FindAll(current => !string.IsNullOrEmpty(current)); // currate the list

			if (options.Count > 0)
			{
				EditorGUI.BeginChangeCheck();

				int index = options.FindIndex(current => current == property.stringValue);
				index = EditorGUI.Popup(position, index, options.ToArray());

				if (EditorGUI.EndChangeCheck())
					property.stringValue = options[index];
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