using UnityEditor;
using UnityEngine;

namespace Fuse.Editor
{
    [CustomEditor(typeof(State))]
    public class StateEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawUtility.DrawHeaderText(serializedObject.targetObject.name.ToUpper(), "STATE", 15, 32, 20);

            DrawUtility.DrawVerticalField("MODULES",
                "<i>Modules assigned will be loaded and started before entering state (and unloaded upon exit if not assigned to next).</i>",
                serializedObject.FindProperty("Modules"));

            DrawUtility.DrawVerticalField("CONTENT",
                "<i>Content assigned will be loaded before entering state (and unloaded upon exit if not assigned to next).</i>",
                serializedObject.FindProperty("Content"));

            DrawUtility.DrawVerticalField("SCENES",
                "<i>Scenes assigned will be loaded when entering state (and unloaded upon exit if not assigned to next).</i>",
                serializedObject.FindProperty("Scenes"));

            DrawUtility.DrawVerticalField("TRANSITIONS",
                "<i>Configure what are applicable transition conditions to other States. " +
                "The framework will listen for events published from Modules and/or Components to transition to another state.</i>",
                serializedObject.FindProperty("Transitions"));

            serializedObject.ApplyModifiedProperties();

            GUILayout.Space(40);

            if (DrawUtility.DrawButton(DrawUtility.ButtonType.Delete))
                Popup.Display<DeletePopup>(target, () => { Selection.activeObject = null; });

            GUILayout.Space(40);
        }
    }
}