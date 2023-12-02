using UnityEditor;
using UnityEngine;

namespace Fuse.Editor
{
    [CustomEditor(typeof(Fuse))]
    public class FuseEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var subtitle = $"{License.Version}";
            DrawUtility.DrawHeaderText("FUSE", subtitle, 20, 42, 20);

            GUILayout.Space(10);

            var fuse = serializedObject.FindProperty("fuseLogo");
            fuse.objectReferenceValue =
                AssetUtility.FetchByPath<Sprite>(string.Format(PathUtility.IconPath, "FuseSplash"));

            DrawUtility.DrawLabel(
                "<i>FUSE is a robust and extendable application framework for Unity leveraging inversion of control</i>.",
                DrawUtility.LabelType.TitleDescription, 16);
            GUILayout.Space(20);

            var manageIcon = string.Format(PathUtility.IconPath, "Configuration");
            if (DrawUtility.DrawButton(DrawUtility.ButtonType.Resource, "<b>MANAGE</b>", manageIcon))
                FuseEditorWindow.Display(FuseEditorWindow.Menu.Configuration);

            serializedObject.ApplyModifiedProperties();
        }
    }
}