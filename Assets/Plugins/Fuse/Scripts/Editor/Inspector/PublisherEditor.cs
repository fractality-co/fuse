using UnityEditor;
using UnityEngine;

namespace Fuse.Editor
{
    [CustomEditor(typeof(Publisher))]
    public class PublisherEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawUtility.DrawHeaderText("PUBLISHER", "relay out events", 20, 42, 24);
            GUILayout.Space(10);

            DrawUtility.DrawLabel(
                "<i>Exposes ability to publish events. " +
                "Custom arguments can be passed for propagation.</i>",
                DrawUtility.LabelType.TitleDescription, 16);
            GUILayout.Space(20);

            DrawUtility.DrawHorizontalField("Event", "<i>Enter the string identifier of the event being published. " +
                                                     "Events published will contain custom EventArgs, if passed with any.</i>",
                serializedObject.FindProperty("eventId"));
            
            DrawUtility.DrawHorizontalField("Delay", "<i>Enter an optional delay in seconds for a DelayedPublish.</i>", serializedObject.FindProperty("delay"));

            if (DrawUtility.DrawButton(DrawUtility.ButtonType.Resource, "PUBLISH EVENT"))
                (serializedObject.targetObject as Publisher)?.Publish();

            serializedObject.ApplyModifiedProperties();
        }
    }
}