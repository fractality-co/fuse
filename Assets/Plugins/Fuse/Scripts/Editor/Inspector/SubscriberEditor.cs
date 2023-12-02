using UnityEditor;
using UnityEngine;

namespace Fuse.Editor
{
    [CustomEditor(typeof(Subscriber))]
    public class SubscriberEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawUtility.DrawHeaderText("SUBSCRIBER", "relay in events", 20, 42, 24);
            GUILayout.Space(10);

            DrawUtility.DrawLabel(
                "<i>Exposes reaction to published events. " +
                "Custom arguments can be passed for propagation.</i>",
                DrawUtility.LabelType.TitleDescription, 16);
            GUILayout.Space(20);

            DrawUtility.DrawHorizontalField("Event", "<i>Enter the string identifier of the event being published. " +
                                                     "Events published will contain custom EventArgs, if passed with any.</i>",
                serializedObject.FindProperty("event"));

            DrawUtility.DrawVerticalField("OnEvent", "<i>Objects to invoke methods or modify properties, when " +
                                                     "the above event has been published.</i>",
                serializedObject.FindProperty("onEvent"));
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}