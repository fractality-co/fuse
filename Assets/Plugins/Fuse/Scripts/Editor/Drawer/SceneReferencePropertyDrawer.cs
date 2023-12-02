using UnityEditor;
using UnityEngine;

namespace Fuse.Editor
{
    /// <summary>
    /// Draws an object field and assigns the path (string) to be persisted.
    /// </summary>
    [CustomPropertyDrawer(typeof(SceneReference))]
    public class SceneReferencePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position = EditorGUI.PrefixLabel(position, label);

            var assetPath = property.stringValue;
            Object asset = null;
            if (!string.IsNullOrEmpty(assetPath))
                asset = AssetUtility.FetchByPath<SceneAsset>(assetPath);

            EditorGUI.BeginChangeCheck();
            asset = EditorGUI.ObjectField(position, asset, typeof(SceneAsset), false);
            if (EditorGUI.EndChangeCheck())
            {
                assetPath = AssetDatabase.GetAssetPath(asset);
                if (!assetPath.Contains(Constants.ScenesAssetPath))
                {
                    Logger.Warn("Scene assigned is not one within Fuse @ " + Constants.ScenesAssetPath);
                    return;
                }

                property.stringValue = assetPath;
            }
        }
    }
}