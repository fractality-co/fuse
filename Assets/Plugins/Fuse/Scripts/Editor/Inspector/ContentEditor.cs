using UnityEditor;
using UnityEngine;

namespace Fuse.Editor
{
    [CustomEditor(typeof(Content))]
    public class ContentEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawUtility.DrawHeaderText(target.name, "CONTENT", 20, 42, 20);
            GUILayout.Space(10);

            DrawUtility.DrawLabel(
                "<i>Content has a generic serializable model wrapped in their own asset bundle. Assign this to a state or globally to make it available.</i>",
                DrawUtility.LabelType.TitleDescription, 16);
            GUILayout.Space(20);

            DrawUtility.DrawVerticalField("PROPERTIES",
                "<i>Store properties persisted into this bundle. Once content is loaded, you can inject Content by it's name and extract values.</i>",
                serializedObject.FindProperty("properties"));
            
            DrawUtility.DrawVerticalField("OBJECTS",
                "<i>Store a hard reference to the object persisted into this bundle. Once content is loaded, you can inject Content by it's name and extract values.</i>",
                serializedObject.FindProperty("assets"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}