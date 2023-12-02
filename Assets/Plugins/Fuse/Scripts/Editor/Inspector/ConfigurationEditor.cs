using UnityEditor;
using UnityEngine;

namespace Fuse.Editor
{
    [CustomEditor(typeof(Configuration))]
    public class ConfigurationEditor : UnityEditor.Editor
    {
        private Vector2 _scroll;

        public override void OnInspectorGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            var subtitle = $"{License.Version}";
            DrawUtility.DrawHeaderText("FUSE", subtitle, 20, 42, 20);
            DrawUtility.DrawLabel("<i>Manage the core application configuration, run-time environment, and the initial boot-up settings.</i>",
                DrawUtility.LabelType.TitleDescription, 16);
            GUILayout.Space(10);

            Configuration config = target as Configuration;
            if (config != null)
            {
                DrawUtility.DrawHorizontalField("START",
                    "<i>Defines the initial state for the application to start once all dependencies are resolved.</i>",
                    serializedObject.FindProperty("Start"));

                DrawUtility.DrawHorizontalField("LOADER",
                    "<i>Controls the display for when when the application is loading.</i>",
                    serializedObject.FindProperty("Loader"));
                GUI.enabled = true;

                DrawUtility.DrawHorizontalField("ENVIRONMENT",
                    "<i>Assign the active environmental configuration; such as development (debug) vs production (release).</i>",
                    serializedObject.FindProperty("Environment"));

                DrawUtility.DrawVerticalField("MODULES",
                    "<i>The assigned modules will be loaded before start and globally accessible, never unloading for the life of the application.</i>",
                    serializedObject.FindProperty("Modules"));

                DrawUtility.DrawVerticalField("SCENES",
                    "<i>The assigned scenes will be loaded before start, never unloading for the life of the application.</i>",
                    serializedObject.FindProperty("Scenes"));

                DrawUtility.DrawVerticalField("CONTENT",
                    "<i>The assigned content will be loaded before start and globally accessible, never unloading for the life of the application.</i>",
                    serializedObject.FindProperty("Content"));

                serializedObject.ApplyModifiedProperties();
            }

            EditorGUILayout.Space(40);

            EditorGUILayout.EndScrollView();
        }
    }
}