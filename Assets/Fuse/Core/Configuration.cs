using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Fuse.Core
{
	/// <summary>
	/// Data relating to the core functionality of <see cref="Fuse"/>.
	/// </summary>
	public class Configuration : ScriptableObject
	{
		public string Host
		{
			get
			{
				switch (Mode)
				{
					case BuildMode.Develop:
						return _host.Develop;
					case BuildMode.Release:
						return _host.Release;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		[StateReference]
		public string Start;

		public BuildMode Mode;

		public LoadMethod Load;

		[SerializeField]
		private Hosting _host;
	}

	public enum LoadMethod
	{
		Baked,
		Hybrid,
		Online
	}

	public enum BuildMode
	{
		Develop,
		Release
	}

	[Serializable]
	public class Hosting
	{
		public string Develop;
		public string Release;
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