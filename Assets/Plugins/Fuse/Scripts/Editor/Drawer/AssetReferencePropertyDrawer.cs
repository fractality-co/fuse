using UnityEditor;
using UnityEngine;

namespace Fuse.Editor
{
    /// <summary>
    /// Draws an object field and assigns the path (string) to be persisted.
    /// </summary>
    [CustomPropertyDrawer(typeof(AssetReference))]
    public class AssetReferencePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position = EditorGUI.PrefixLabel(position, label);

            var reference = (AssetReference) attribute;

            if (!reference.Type.IsSubclassOf(typeof(Object)))
            {
                EditorGUI.LabelField(position, "Invalid assigned type on AssetReference.");
                return;
            }

            var assetPath = property.stringValue;
            Object asset = null;
            if (!string.IsNullOrEmpty(assetPath))
                asset = AssetUtility.FetchByPath<Object>(assetPath);

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
}