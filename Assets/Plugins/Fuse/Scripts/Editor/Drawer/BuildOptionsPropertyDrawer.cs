using UnityEditor;
using UnityEngine;

namespace Fuse.Editor
{
    /// <summary>
    /// Draws a drop down of build options for our build pipeline.
    /// </summary>
    [CustomPropertyDrawer(typeof(BuildOptions))]
    public class BuildOptionsPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, GUIContent.none, property);
            var current = (UnityEditor.BuildOptions) property.intValue;
            current = (UnityEditor.BuildOptions) EditorGUI.EnumFlagsField(position, label, current);
            property.intValue = (int) current;
            EditorGUI.EndProperty();
        }
    }
}