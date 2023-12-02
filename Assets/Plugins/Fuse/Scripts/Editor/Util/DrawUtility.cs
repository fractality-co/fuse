using System;
using UnityEditor;
using UnityEngine;

namespace Fuse.Editor
{
    /// <summary>
    /// Helper class to abstract away GUI draw functions.
    /// </summary>
    public static class DrawUtility
    {
        private static readonly GUIStyle ButtonStyle = new GUIStyle
        {
            fixedWidth = 170,
            fixedHeight = 50,
            alignment = TextAnchor.MiddleCenter,
            imagePosition = ImagePosition.ImageOnly
        };

        private static readonly GUIStyle ResourceStyle = new GUIStyle
        {
            fixedWidth = 200,
            fixedHeight = 63,
            alignment = TextAnchor.MiddleCenter,
            imagePosition = ImagePosition.ImageOnly
        };

        public enum ButtonType
        {
            Export,
            Create,
            Resource,
            Delete,
            Duplicate,
            Select,
            CreateIcon,
            DeleteIcon,
            Accept,
            Decline
        }

        public enum LabelType
        {
            Title,
            Subtitle,
            Regular,
            Inspector,
            InspectorDescription,
            Resource,
            TitleDescription
        }

        public const string TitleFontName = "Quantum";
        public const string RegularFontName = "FuturaMedium";
        public const string InspectorFontName = "FuturaMedium";
        public const string ResourceFontName = "FuturaMedium";

        public static readonly string FontPath = PathUtility.BasePath + "/2D/Fonts/{0}.otf";

        public static void DrawVerticalField(string name, string description, SerializedProperty property)
        {
            GUILayout.Space(10);
            GUILayout.BeginVertical();
            DrawLabel("<b>" + name + "</b>", LabelType.Inspector, 18);
            GUILayout.Space(5);
            DrawLabel(description, LabelType.InspectorDescription, 14);
            GUILayout.Space(5);
            EditorGUILayout.PropertyField(property, new GUIContent(GrayscaleBarImage()));
            GUILayout.Space(15);
            GUILayout.EndVertical();
        }

        public static void DrawHorizontalField(string name, string description, SerializedProperty property)
        {
            GUILayout.Space(10);
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            DrawLabel("<b>" + name + "</b>", LabelType.Inspector, 18);
            EditorGUILayout.PropertyField(property, new GUIContent(string.Empty));
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
            DrawLabel(description, LabelType.InspectorDescription, 14);
            GUILayout.Space(15);
            GUILayout.EndVertical();
        }

        public static void DrawLabel(string label, LabelType type, int size)
        {
            GUIStyle style;
            switch (type)
            {
                case LabelType.Title:
                    style = TitleFont(size);
                    break;
                case LabelType.Subtitle:
                    style = SubtitleFont(size);
                    break;
                case LabelType.Regular:
                    style = RegularFont(size);
                    break;
                case LabelType.Inspector:
                    style = InspectorFont(size);
                    break;
                case LabelType.InspectorDescription:
                    style = InspectorDescriptionFont(size);
                    break;
                case LabelType.Resource:
                    style = ResourceFont(size);
                    break;
                case LabelType.TitleDescription:
                    style = TitleDescriptionFont(size);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            GUILayout.Label(label, style);
        }

        public static void DrawHeaderImage(Texture2D title, GUIStyle titleStyle, string subtitle,
            int subtitleSize = 32, float spacer = 20)
        {
            GUIStyle style = new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                imagePosition = ImagePosition.ImageOnly
            };

            GUILayout.Space(spacer);
            GUILayout.Label(BarImage(), style);
            GUILayout.Space(5);
            GUILayout.Label(title, titleStyle);

            if (!string.IsNullOrEmpty(subtitle))
                GUILayout.Label(subtitle, SubtitleFont(subtitleSize));

            GUILayout.Space(5);
            GUILayout.Label(BarImage(), style);
            GUILayout.Space(spacer);
        }

        public static void DrawHeaderText(string title, string subtitle, float spacer = 20, int titleSize = 64,
            int subtitleSize = 20)
        {
            GUIStyle style = new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                imagePosition = ImagePosition.ImageOnly
            };

            GUILayout.Space(spacer);
            GUILayout.Label(BarImage(), style);
            GUILayout.Space(5);
            GUILayout.Label(title, TitleFont(titleSize));

            if (!string.IsNullOrEmpty(subtitle))
                GUILayout.Label(subtitle, SubtitleFont(subtitleSize));

            GUILayout.Space(5);

            GUILayout.Label(BarImage(), style);
            GUILayout.Space(spacer);
        }

        public static bool DrawButton(ButtonType buttonType, string text = "", string icon = "",
            GUIStyle style = null, bool flexible = true, bool layout = true, Rect container = default,
            bool interactive = true)
        {
            if (layout)
                GUILayout.BeginHorizontal();

            var pressed = false;

            if (flexible && layout)
                GUILayout.FlexibleSpace();

            style = style ?? (buttonType == ButtonType.Resource ? ResourceStyle : ButtonStyle);
            container = container == Rect.zero
                ? GUILayoutUtility.GetRect(new GUIContent(ButtonImage(buttonType)), style)
                : container;

            if (interactive && GUI.Button(container, ButtonImage(buttonType), style))
                pressed = true;
            else if (!interactive)
            {
                GUI.Label(container, ButtonImage(buttonType), style);
            }

            if (!string.IsNullOrEmpty(text))
            {
                GUI.Label(new Rect(container.x + 20, container.y, container.width - 20 - 32, container.height), text,
                    ResourceFont(GetDynamicSize(text)));
            }

            if (!string.IsNullOrEmpty(icon))
            {
                const int iconSize = 32;
                GUI.DrawTexture(
                    new Rect(container.x + container.width - iconSize - 10,
                        container.y + container.height * 0.5f - iconSize * 0.5f, iconSize, iconSize),
                    AssetUtility.FetchByPath<Texture2D>(icon));
            }

            if (flexible && layout)
                GUILayout.FlexibleSpace();

            if (layout)
                GUILayout.EndHorizontal();

            return pressed;
        }

        public static int GetDynamicSize(string text, int reduction = 0)
        {
            int fontSize = 20;
            if (text.Length > 20)
                fontSize = 13;
            else if (text.Length > 15)
                fontSize = 15;
            else if (text.Length > 10)
                fontSize = 18;
            return fontSize - reduction;
        }

        public static Texture2D FuseIconImage()
        {
            return AssetUtility.FetchByPath<Texture2D>(string.Format(PathUtility.IconPath, "MenuHeader"));
        }

        public static Texture2D FuseLogoImage()
        {
            return AssetUtility.FetchByPath<Texture2D>(string.Format(PathUtility.IconPath, "FuseFull"));
        }

        public static Texture2D BarImage()
        {
            return AssetUtility.FetchByPath<Texture2D>(PathUtility.BarIconPath);
        }

        public static Texture2D GrayscaleBarImage()
        {
            return AssetUtility.FetchByPath<Texture2D>(PathUtility.GrayscaleBarIconPath);
        }

        public static Texture2D ButtonImage(ButtonType buttonType)
        {
            return AssetUtility.FetchByPath<Texture2D>(
                PathUtility.ButtonPath.Replace(PathUtility.KeyButton, buttonType.ToString()));
        }

        public static Texture2D IconImage(string name)
        {
            return AssetUtility.FetchByPath<Texture2D>(string.Format(PathUtility.IconPath, name));
        }

        public static Font GetFont(string name)
        {
            return AssetUtility.FetchByPath<Font>(string.Format(FontPath, name));
        }

        public static GUIStyle TitleFont(int fontSize = 24)
        {
            return CustomFont(string.Format(TitleFontName), fontSize, TextAnchor.MiddleCenter,
                new Color(0.391f, 0.515f, 0.749f));
        }

        public static GUIStyle SubtitleFont(int fontSize = 24)
        {
            return CustomFont(string.Format(TitleFontName), fontSize, TextAnchor.MiddleCenter,
                new Color(0.341f, 0.465f, 0.749f));
        }

        public static GUIStyle RegularFont(int fontSize = 24)
        {
            return CustomFont(string.Format(RegularFontName), fontSize, TextAnchor.MiddleCenter,
                new Color(0.75f, 0.75f, 0.75f));
        }

        public static GUIStyle InspectorFont(int fontSize = 20)
        {
            return CustomFont(string.Format(InspectorFontName), fontSize, TextAnchor.MiddleLeft,
                new Color(0.75f, 0.75f, 0.75f));
        }

        public static GUIStyle ResourceFont(int fontSize = 20)
        {
            return CustomFont(string.Format(ResourceFontName), fontSize, TextAnchor.MiddleLeft,
                new Color(0.75f, 0.75f, 0.75f));
        }

        public static GUIStyle TitleDescriptionFont(int fontSize = 20)
        {
            return CustomFont(string.Format(InspectorFontName), fontSize, TextAnchor.MiddleCenter,
                new Color(0.6f, 0.6f, 0.65f));
        }

        public static GUIStyle InspectorDescriptionFont(int fontSize = 20)
        {
            return CustomFont(string.Format(InspectorFontName), fontSize, TextAnchor.MiddleLeft,
                new Color(0.6f, 0.6f, 0.65f));
        }

        public static GUIStyle CustomFont(string fontName, int fontSize, TextAnchor anchor, Color color)
        {
            return new GUIStyle
            {
                font = GetFont(fontName),
                fontSize = fontSize,
                alignment = anchor,
                normal = {textColor = color},
                wordWrap = true,
                richText = true
            };
        }
    }
}