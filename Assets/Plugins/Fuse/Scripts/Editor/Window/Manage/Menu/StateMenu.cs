using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Fuse.Editor
{
    /// <summary>
    /// Sub-menu for managing <see cref="State"/>s.
    /// </summary>
    public class StateMenu : IMenu
    {
        public static Vector2 LastCursor;
        
        private static readonly GUIStyle TransitionStyle = new GUIStyle {normal = {background = null}};
        private const int TransitionSize = 18;

        public Action Refresh { get; set; }

        private readonly List<State> _catalog = new List<State>();
        private GUIStyle _nodeStyle;
        private State _drag;
        private bool _transition;
        private State _transitionFrom;
        private Configuration _config;

        public void Setup()
        {
            _nodeStyle = new GUIStyle
            {
                normal =
                {
                    background = DrawUtility.ButtonImage(DrawUtility.ButtonType.Resource),
                    textColor = new Color(0.75f, 0.75f, 0.75f)
                },
                alignment = TextAnchor.MiddleLeft,
                font = DrawUtility.GetFont("FuturaMedium"),
                fontSize = 18,
                border = new RectOffset(12, 12, 12, 12)
            };

            LoadStates();
        }

        public void Cleanup()
        {
        }

        public void Draw(Rect window)
        {
            GUILayout.BeginArea(window);
            GUILayout.Space(20);
            DrawUtility.DrawHeaderText("STATE", "manage application flow", 0, 42, 24);
            GUILayout.Space(20);
            DrawUtility.DrawLabel(
                "<i>Manage the application's flow by creating States, which start or stop Module(s), load Scene(s) and transition to other States from published events.</i>",
                DrawUtility.LabelType.TitleDescription, 16);
            GUILayout.Space(20);

            var background = new Rect(0, 205, 405, 600);
            GraphBackground.DrawGraphBackground(background, background);

            var origin = new Vector2(200, background.y + 1);
            var originEnd = new Vector2(200, background.y + 11);
            DrawTransition(null, origin, originEnd);
            DrawStartTransition(originEnd);

            foreach (var state in _catalog)
            {
                if (state == null)
                    continue;

                DrawTransitions(state);
            }

            foreach (var state in _catalog)
            {
                if (state == null)
                    continue;

                DrawState(state);
            }

            GUILayout.EndArea();
            ProcessEvents(Event.current);
        }

        private void DrawState(State state)
        {
            var style = new GUIStyle(_nodeStyle) {fontSize = DrawUtility.GetDynamicSize(state.NodeTitle, 3)};
            GUI.Box(state.NodeRect, "   <b>" + state.NodeTitle + "</b>", style);

            var iconRect = state.NodeRect;
            iconRect.width = iconRect.height = 38;
            iconRect.x = state.NodeRect.x + state.NodeRect.width - iconRect.width - 10;
            iconRect.y = state.NodeRect.y + state.NodeRect.height / 2 - iconRect.height / 2;
            GUI.Label(iconRect, DrawUtility.IconImage("State"));
        }

        private void DrawStartTransition(Vector2 origin)
        {
            var filename = Path.GetFileNameWithoutExtension(_config.Start);
            var start = _catalog.Find(state => state != null && state.name == filename);
            if (start != null)
            {
                DrawTransition(start, origin, start.NodeRect.center - new Vector2(0, start.NodeRect.height / 2 - 10));
                GUI.changed = true;
            }
        }

        private void DrawTransitions(State data)
        {
            if (_transition && data == _transitionFrom && data != null)
            {
                DrawTransition(_transitionFrom, data.NodeRect.center, Event.current.mousePosition);
                GUI.changed = true;
            }

            if (data.NodeTransitions != null)
            {
                foreach (var transition in data.NodeTransitions)
                {
                    var state = AssetUtility.FetchByPath<State>(transition.To);
                    if (state == null)
                        continue;

                    DrawTransition(data, data.NodeRect.center, state.NodeRect.center);
                }
            }
        }

        private void DrawTransition(Object data, Vector2 start, Vector2 end)
        {
            var delta = end - start;
            var offset = new Vector2(delta.y > 0 ? 10 : -10, 0);

            start += offset;
            end += offset;

            Handles.DrawBezier(
                start,
                end,
                start,
                end,
                new Color(1, 1, 1),
                null,
                4f
            );


            if (data != null)
            {
                var rotate = (float) (Math.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
                var height = TransitionSize;
                var rect = new Rect(start.x + delta.x / 2f - height / 2f, start.y + delta.y / 2f - height / 2f, height,
                    height);
                rect.position += delta * 0.04f;
                GUI.BeginGroup(rect);
                GUIUtility.RotateAroundPivot(rotate, new Vector2(height / 2f, height / 2f));
                if (GUI.Button(new Rect(0, 0, height, height), DrawUtility.IconImage("Arrow"), TransitionStyle))
                    Selection.activeObject = data;
                GUIUtility.RotateAroundPivot(-rotate, new Vector2(height / 2f, height / 2f));
                GUI.EndGroup();
            }
        }


        private void LoadStates()
        {
            _config = AssetUtility.FetchByPath<Configuration>(Constants.GetConfigurationAssetPath());
            _catalog.Clear();
            foreach (var guid in AssetDatabase.FindAssets("t:" + nameof(State)))
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (!assetPath.Contains(Constants.StatesAssetPath))
                    continue;

                var state = AssetUtility.FetchByPath<State>(assetPath);
                _catalog.Add(state);
            }
        }

        private void ProcessEvents(Event e)
        {
            switch (e.type)
            {
                case EventType.MouseDown:
                    var hit = HitTest(e.mousePosition);
                    if (_transition)
                    {
                        if (hit != null)
                        {
                            if (_transitionFrom.NodeTransitions == null)
                                _transitionFrom.NodeTransitions = new List<Transition>();
                            _transitionFrom.NodeTransitions.Add(new Transition(AssetDatabase.GetAssetPath(hit),
                                "event.id"));
                            Selection.activeObject = null;
                            EditorUtility.SetDirty(_transitionFrom);
                            AssetDatabase.SaveAssets();
                            Selection.activeObject = _transitionFrom;
                        }

                        _transition = false;
                        _transitionFrom = null;
                        e.Use();
                        return;
                    }

                    if (e.button == 0 && hit != null)
                    {
                        Selection.activeObject = hit;
                        e.Use();
                        return;
                    }

                    if (e.button == 1)
                    {
                        LastCursor = e.mousePosition;

                        if (hit != null)
                            ProcessContextMenu(hit);
                        else
                            ShowBackgroundContextMenu();

                        e.Use();
                    }

                    break;
                case EventType.MouseUp:
                    if (_drag != null)
                    {
                        EditorUtility.SetDirty(_drag);
                        AssetDatabase.SaveAssets();
                        _drag = null;
                    }

                    break;
                case EventType.MouseDrag:
                    if (_drag == null)
                        _drag = HitTest(e.mousePosition);

                    if (_drag != null)
                    {
                        var dragRect = _drag.NodeRect;
                        dragRect.position += e.delta;
                        dragRect.position = ClampPosition(dragRect);
                        _drag.NodeRect = dragRect;
                        Refresh?.Invoke();
                    }

                    break;
            }
        }

        private static Vector2 ClampPosition(Rect rect)
        {
            const int minY = 250;
            const int minX = 0;
            const int maxX = 225;
            const int maxY = 750;

            var position = rect.position;
            if (position.y < minY)
                position = new Vector2(position.x, minY);
            if (position.y > maxY)
                position = new Vector2(position.x, maxY);
            if (position.x < minX)
                position = new Vector2(minX, position.y);
            if (position.x > maxX)
                position = new Vector2(maxX, position.y);

            return position;
        }

        private void ProcessContextMenu(State state)
        {
            GenericMenu genericMenu = new GenericMenu();
            genericMenu.AddItem(new GUIContent("Delete State"), false, () => { ShowDeletePopup(state); });
            genericMenu.AddItem(new GUIContent("Create Transition"), false, () =>
            {
                _transition = true;
                _transitionFrom = state;
                Refresh?.Invoke();
            });
            genericMenu.ShowAsContext();
        }

        private void ShowBackgroundContextMenu()
        {
            GenericMenu genericMenu = new GenericMenu();
            genericMenu.AddItem(new GUIContent("Create State"), false, ShowCreatePopup);
            genericMenu.ShowAsContext();
        }

        private void ShowCreatePopup()
        {
            Popup.Display<CreatePopup>(typeof(State), Constants.StatesAssetPath, Refreshed);
        }

        private void ShowDeletePopup(State state)
        {
            Popup.Display<DeletePopup>(state, Refreshed);
        }

        private State HitTest(Vector2 point)
        {
            foreach (var state in _catalog)
            {
                if (state.NodeRect.Contains(point))
                    return state;
            }

            return null;
        }

        private void Refreshed()
        {
            LoadStates();
            Refresh?.Invoke();
        }
    }
}