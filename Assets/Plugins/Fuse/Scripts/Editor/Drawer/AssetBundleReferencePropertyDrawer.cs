using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Fuse.Editor
{
    /// <summary>
    /// Draws a drop down of bundles but stores the bundle name (string).
    /// </summary>
    [CustomPropertyDrawer(typeof(AssetBundleReference))]
    public class AssetBundleReferencePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, GUIContent.none, property);

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            var options = AssetDatabase.GetAllAssetBundleNames().ToList();
            if (options.Count > 0)
            {
                EditorGUI.BeginChangeCheck();

                var typeIndex = options.IndexOf(property.stringValue);
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
}