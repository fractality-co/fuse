using UnityEditor;
using UnityEngine;

namespace Fuse.Editor
{
    /// <summary>
    /// Draws a drop down of asset bundle build options for our asset pipeline.
    /// </summary>
    [CustomPropertyDrawer(typeof(BuildAssetBundleOptions))]
    public class BuildAssetBundleOptionsPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, GUIContent.none, property);
            var current = (UnityEditor.BuildAssetBundleOptions) property.intValue;
            current = (UnityEditor.BuildAssetBundleOptions) EditorGUI.EnumFlagsField(position, label, current);
            property.intValue = (int) current;
            EditorGUI.EndProperty();
        }
    }
}