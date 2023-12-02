using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Fuse.Editor
{
    /// <summary>
    /// Draws a drop down of bundles but stores the bundle name (string).
    /// </summary>
    [CustomPropertyDrawer(typeof(ModuleReference))]
    public class ModuleReferencePropertyDrawer : PropertyDrawer
    {
        private static readonly List<string> Options = new List<string>();
        private static DateTime _lastRefresh;
        private static bool _first = true;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (Options.Count == 0 && (_first || (DateTime.Now - _lastRefresh).Seconds > 5))
            {
                _first = false;
                _lastRefresh = DateTime.Now;

                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (type.IsAbstract || !type.IsSubclassOf(typeof(Module)))
                            continue;

                        Options.Add(type.FullName);
                    }
                }
            }

            EditorGUI.BeginProperty(position, GUIContent.none, property);
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            if (Options.Count > 0)
            {
                EditorGUI.BeginChangeCheck();

                var typeIndex = Options.IndexOf(property.stringValue);
                typeIndex = EditorGUI.Popup(position, typeIndex, Options.ToArray());

                if (EditorGUI.EndChangeCheck())
                    property.stringValue = Options[typeIndex];
            }
            else
            {
                EditorGUI.Popup(position, 0, new[] {string.Empty});
            }

            EditorGUI.EndProperty();
        }
    }
}