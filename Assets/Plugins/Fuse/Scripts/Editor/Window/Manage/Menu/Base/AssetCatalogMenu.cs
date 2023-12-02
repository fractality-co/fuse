using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Fuse.Editor
{
    /// <summary>
    /// Base abstraction for sub-menus that edit groupings of assets. 
    /// </summary>
    public abstract class AssetCatalogMenu<T> : IMenu where T : Object
    {
        public Action Refresh { get; set; }

        protected abstract Type RequiredAttribute { get; }
        protected abstract bool NestByType { get; }
        protected abstract string AssetExtension { get; }
        protected abstract string BasePath { get; }
        protected abstract string Subtitle { get; }
        protected abstract string Description { get; }

        private T _selected;
        private readonly List<T> _catalog = new List<T>();
        private UnityEditor.Editor _selectionEditor;
        private Type _type;
        private Vector2 _scroll;

        public void Setup()
        {
            _type = typeof(T);
            Load();

            if (_catalog.Any())
                Select(_catalog.First(), false);
        }

        public void Cleanup()
        {
        }

        public void Draw(Rect window)
        {
            GUILayout.BeginArea(new Rect(window.x, window.y,
                window.width, window.height));

            _scroll = GUILayout.BeginScrollView(_scroll);
            GUILayout.BeginVertical();

            string title = RequiredAttribute == null
                ? typeof(T).Name
                : RequiredAttribute.Name.Replace("Attribute", string.Empty);
            if (title.Contains("Asset"))
                title = title.Replace("Asset", string.Empty);

            DrawUtility.DrawHeaderText(title.ToUpper(), Subtitle, 20, 42, 24);
            DrawUtility.DrawLabel(Description, DrawUtility.LabelType.TitleDescription, 16);
            GUILayout.Space(20);

            foreach (T instance in _catalog)
            {
                if (instance == null)
                {
                    GUILayout.EndVertical();
                    GUILayout.EndScrollView();
                    GUILayout.EndArea();

                    Load();
                    return;
                }

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                if (DrawUtility.DrawButton(DrawUtility.ButtonType.Resource, "<b>" + instance.name + "</b>",
                    string.Format(PathUtility.IconPath, title), null, false))
                    Select(instance, true);
                if (GUILayout.Button(DrawUtility.ButtonImage(DrawUtility.ButtonType.DeleteIcon),
                    new GUIStyle {contentOffset = new Vector2 {x = 5, y = 15}, fixedHeight = 32, fixedWidth = 32}))
                    Popup.Display<DeletePopup>(instance, Load);

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            if (DrawUtility.DrawButton(DrawUtility.ButtonType.Create))
                Popup.Display<CreatePopup>(typeof(T), BasePath, Load);

            GUILayout.Space(40);

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private string GetPath(T instance, string newName = "")
        {
            return BasePath + "/" + (NestByType ? instance.GetType().Name + "/" : string.Empty) +
                   (string.IsNullOrEmpty(newName) ? instance.name : newName) + "." + AssetExtension;
        }

        private void Select(T selection, bool fromAction)
        {
            if (fromAction)
                Open(selection);
            
            if (!typeof(T).IsSubclassOf(typeof(ScriptableObject)))
                return;

            if (_selectionEditor != null)
            {
                Object.DestroyImmediate(_selectionEditor);
                _selectionEditor = null;
            }

            _selected = selection;
            _selectionEditor = UnityEditor.Editor.CreateEditor(_selected);
        }

        private void Open(T selection)
        {
            if (selection is SceneAsset)
                EditorSceneManager.OpenScene(GetPath(selection), OpenSceneMode.Additive);
            else
                Selection.activeObject = selection;
        }

        private void Load()
        {
            AssetDatabase.Refresh();

            _catalog.Clear();
            foreach (string guid in AssetDatabase.FindAssets("t:" + typeof(T).Name))
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (!assetPath.Contains(BasePath))
                    continue;

                T instance = AssetUtility.FetchByPath<T>(assetPath);
                _catalog.Add(instance);
            }
        }
    }
}