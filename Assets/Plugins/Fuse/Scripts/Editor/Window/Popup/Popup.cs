using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Fuse.Editor
{
    /// <summary>
    /// Generic non-resizable window for many purposes of showing a popup.
    /// </summary>
    public abstract class Popup : EditorWindow
    {
        [Flags]
        protected enum ActionLayout
        {
            None = 0,
            AcceptDecline = 1,
            Confirmation = 2,
            Input = 4,
            Toggle = 8
        }

        protected abstract string Title { get; }
        protected abstract string Subtitle { get; }
        protected abstract string Body { get; }
        protected virtual string Toggle => "";
        protected abstract ActionLayout Action { get; }

        /// <summary>
        /// The pop-ups current focus for operations.
        /// </summary>
        public Object Target;

        /// <summary>
        /// The pop-ups current payload for operations.
        /// </summary>
        public string Payload;

        /// <summary>
        /// The pop-ups current type for operations.
        /// </summary>
        public Type Type;

        /// <summary>
        /// The pop-ups binding back for when an operation caused a change.
        /// </summary>
        public Action Callback;

        protected bool Toggled { get; set; }
        protected string Input { get; private set; }

        protected readonly Vector2 Size;
        protected readonly float Padding = 20;

        protected Popup(float height = 300, float padding = 20)
        {
            maxSize = minSize = Size = new Vector2(400, height);
            Padding = padding;
        }

        protected Popup(Vector2 size, float padding = 10)
        {
            Padding = padding;
            maxSize = minSize = Size = size;
        }

        public static void Display<T>() where T : Popup
        {
            var texture = AssetUtility.FetchByPath<Texture2D>(PathUtility.MenuHeaderIconPath);
            var window = GetWindow<T>();
            window.titleContent = new GUIContent(texture, "FUSE - Application Framework");
            window.Show();
        }

        public static void Display<T>(Object target, Action callback = null) where T : Popup
        {
            var texture = AssetUtility.FetchByPath<Texture2D>(PathUtility.MenuHeaderIconPath);
            var window = GetWindow<T>();
            window.titleContent = new GUIContent(texture, "FUSE - Application Framework");
            window.Show();
            window.Target = target;
            window.Callback = callback;
        }

        public static void Display<T>(string message, Action callback = null) where T : Popup
        {
            var texture = AssetUtility.FetchByPath<Texture2D>(PathUtility.MenuHeaderIconPath);
            var window = GetWindow<T>();
            window.titleContent = new GUIContent(texture, "FUSE - Application Framework");
            window.Show();
            window.Payload = message;
            window.Callback = callback;
        }

        public static void Display<T>(Type type, string payload, Action callback = null) where T : Popup
        {
            var texture = AssetUtility.FetchByPath<Texture2D>(PathUtility.MenuHeaderIconPath);
            var window = GetWindow<T>();
            window.titleContent = new GUIContent(texture, "FUSE - Application Framework");
            window.Show();
            window.Type = type;
            window.Payload = payload;
            window.Callback = callback;
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(Padding, Padding, Size.x - Padding * 2, Size.y - Padding * 2));
            DrawUtility.DrawHeaderText(Title, Subtitle, 0, DrawUtility.GetDynamicSize(Title, -20), 28);
            GUILayout.FlexibleSpace();
            DrawUtility.DrawLabel(Body, DrawUtility.LabelType.Regular, 18);
            GUILayout.FlexibleSpace();

            GUILayout.BeginVertical();

            if ((Action & ActionLayout.Input) != ActionLayout.None)
            {
                Input = EditorGUILayout.TextArea(Input);
                GUI.enabled = !string.IsNullOrEmpty(Input);
                GUILayout.Space(10);
            }

            if ((Action & ActionLayout.Toggle) != ActionLayout.None)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(75);
                DrawUtility.DrawLabel("<i>" + Toggle + "</i>", DrawUtility.LabelType.InspectorDescription, 18);
                GUILayout.Space(5);
                Toggled = EditorGUILayout.Toggle(Toggled);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(15);
            }

            if ((Action & ActionLayout.Confirmation) != ActionLayout.None)
            {
                if (DrawUtility.DrawButton(DrawUtility.ButtonType.Accept))
                {
                    OnAccept();
                    Callback?.Invoke();
                    Close();
                }
            }

            if ((Action & ActionLayout.AcceptDecline) != ActionLayout.None)
            {
                GUILayout.BeginHorizontal();
                if (DrawUtility.DrawButton(DrawUtility.ButtonType.Accept))
                {
                    OnAccept();
                    Callback?.Invoke();
                    Close();
                }

                if (DrawUtility.DrawButton(DrawUtility.ButtonType.Decline))
                {
                    OnDecline();
                    Close();
                }

                GUILayout.EndHorizontal();
            }

            GUI.enabled = true;
            GUILayout.EndVertical();

            GUILayout.EndArea();
        }


        /// <summary>
        /// Invokes when user explicitly accept by action.
        /// </summary>
        protected virtual void OnAccept()
        {
        }

        /// <summary>
        /// Invokes when user explicitly declines by action.
        /// </summary>
        protected virtual void OnDecline()
        {
        }
    }
}