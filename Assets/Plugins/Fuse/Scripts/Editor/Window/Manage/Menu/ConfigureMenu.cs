using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Fuse.Editor
{
    /// <summary>
    /// Sub-menu for editing <see cref="Configuration"/>.
    /// </summary>
    public class ConfigureMenu : IMenu
    {
        public Action Refresh { get; set; }

        private UnityEditor.Editor _editor;

        public void Setup()
        {
            var configuration = AssetUtility.FetchByPath<Configuration>(Constants.GetConfigurationAssetPath());
            _editor = UnityEditor.Editor.CreateEditor(configuration);
        }

        public void Cleanup()
        {
            Object.DestroyImmediate(_editor);
            _editor = null;
        }

        public void Draw(Rect window)
        {
            GUILayout.BeginArea(window);
            _editor.OnInspectorGUI();
            GUILayout.EndArea();
        }
    }
}