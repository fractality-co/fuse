using UnityEditor;
using UnityEngine;

namespace Fuse.Editor
{
    [CustomEditor(typeof(Environment))]
    public class EnvironmentEditor : UnityEditor.Editor
    {
        private Vector2 _scroll;

        public override void OnInspectorGUI()
        {
            DrawUtility.DrawHeaderText(serializedObject.targetObject.name.ToUpper(), "ENVIRONMENT", 20, 32, 20);

            GUILayout.Space(10);

            DrawUtility.DrawHorizontalField("MODE",
                "<i>Defines how the build should run; develop turns on debug options, release turns off all debug options.</i>",
                serializedObject.FindProperty("mode"));

            var loadingProperty = serializedObject.FindProperty("loading");
            DrawUtility.DrawHorizontalField("LOAD",
                "<i>Defines where assets should be loaded from; baked will automatically be bundled.</i>",
                loadingProperty);

            if (loadingProperty.boolValue)
            {
                DrawUtility.DrawHorizontalField("HOST (URI)",
                    "<i>The root host uri for downloading assets, note that asset bundles loaded will append the platform (lower-cased) from Unity's BuildTarget enum.\n\n" +
                    "For example, 'https://host.com/bucket' at runtime will build to become 'https://host.com/bucket/ios/core.unity3d' to load the core on iOS.</i>",
                    serializedObject.FindProperty("hostUri"));
            }

            GUILayout.Space(10);

            DrawUtility.DrawHorizontalField("BUILD ICON",
                "<i>Optionally, assign an icon that overrides ProjectSettings application icon when building.</i>",
                serializedObject.FindProperty("icon"));

            DrawUtility.DrawHorizontalField("BUILD OPTIONS",
                "<i>Custom player build options to be passed to the internal Unity build pipeline.</i>",
                serializedObject.FindProperty("build"));

            DrawUtility.DrawHorizontalField("ASSET OPTIONS",
                "<i>Custom asset build options to be passed to the internal Unity build pipeline.</i>",
                serializedObject.FindProperty("assets"));

            GUILayout.Space(10);

            DrawUtility.DrawVerticalField("SERVERS",
                "<i>Configurable server URL mappings based on environment. " +
                "Useful for handling environment based URI schemes for any server (i.e. http or socket).</i>",
                serializedObject.FindProperty("servers"));
            
            DrawUtility.DrawVerticalField("PROPERTIES",
                "<i>Store properties persisted into this environment. You can inject this and extract values for auth or more.</i>",
                serializedObject.FindProperty("properties"));

            serializedObject.ApplyModifiedProperties();

            GUILayout.Space(30);

            if (DrawUtility.DrawButton(DrawUtility.ButtonType.Delete))
                Popup.Display<DeletePopup>(target, () => { });

            GUILayout.Space(40);
        }
    }
}