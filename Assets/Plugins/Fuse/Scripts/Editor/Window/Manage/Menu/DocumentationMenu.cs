using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Fuse.Editor
{
    public class DocumentationMenu : IMenu
    {
        public Action Refresh { get; set; }

        private Vector2 _scroll;
        private string _selectedCategory;

        private List<string> _categories = new List<string>();

        private Dictionary<string, List<Tuple<string, string>>> _documents =
            new Dictionary<string, List<Tuple<string, string>>>();

        public void Setup()
        {
            _categories = _categories ?? new List<string>();
            _documents = _documents ?? new Dictionary<string, List<Tuple<string, string>>>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    var document = type.GetCustomAttribute<Document>(false);
                    if (document == null)
                        continue;

                    if (!_categories.Contains(document.Category))
                        _categories.Add(document.Category);

                    if (!_documents.ContainsKey(document.Category))
                        _documents.Add(document.Category, new List<Tuple<string, string>>());
                    _documents[document.Category].Add(new Tuple<string, string>(type.Name, document.Body));
                }
            }

            _categories.Sort();
        }

        public void Draw(Rect window)
        {
            GUILayout.BeginArea(window);
            _scroll = GUILayout.BeginScrollView(_scroll);

            DrawUtility.DrawHeaderText("DOCUMENTATION", "supporting information", 20, 36, 24);
            DrawUtility.DrawLabel(
                "<i>Access API for the framework to clarify or assist your development. The framework has more thorough documentation accessible below.</i>",
                DrawUtility.LabelType.TitleDescription, 16);
            GUILayout.Space(20);

            if (DrawUtility.DrawButton(DrawUtility.ButtonType.Resource, "Open Repository"))
                Application.OpenURL("https://github.com/jdharrison/fuse");
            if (DrawUtility.DrawButton(DrawUtility.ButtonType.Resource, "Open Documentation"))
                Application.OpenURL("https://docs.google.com/document/d/1gE4HEmffCe2lw0tdEf99m1MG-W5ZUNpr2wEVenfXilo");

            GUILayout.Space(20);
            
            DrawUtility.DrawLabel("<i>Reference API</i>", DrawUtility.LabelType.TitleDescription, 20);
            
            GUILayout.Space(10);
            
            var hasCategorySelected = !string.IsNullOrEmpty(_selectedCategory);
            if (!hasCategorySelected)
            {
                foreach (var category in _categories)
                {
                    if (DrawUtility.DrawButton(DrawUtility.ButtonType.Resource, category))
                        _selectedCategory = category;
                }
            }
            else
            {
                if (DrawUtility.DrawButton(DrawUtility.ButtonType.Resource, "BACK"))
                    _selectedCategory = string.Empty;

                foreach (var kvp in _documents)
                {
                    if (kvp.Key != _selectedCategory)
                        continue;

                    foreach (var (title, body) in kvp.Value)
                    {
                        if (DrawUtility.DrawButton(DrawUtility.ButtonType.Resource, title))
                            Popup.Display<GenericPopup>(GenericPopup.Build(title, "DOCUMENTATION", body));
                    }
                }
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        public void Cleanup()
        {
            _documents.Clear();
        }
    }
}