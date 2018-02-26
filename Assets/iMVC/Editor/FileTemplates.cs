using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace iMVC.Editor
{
    public static class FileTemplates
    {
        [MenuItem("Window/iMVC/Implementation")]
        [MenuItem("Assets/iMVC/Implementation")]
        public static void CreateTemplate()
        {
            TemplateWindow window = EditorWindow.GetWindow<TemplateWindow>();
            window.OnCreate += (name, type) =>
            {
                switch (type)
                {
                    case TemplateWindow.TemplateType.Controller:
                        CreateTemplate<ControllerAttribute>(name);
                        break;
                    case TemplateWindow.TemplateType.Model:
                        CreateTemplate<ModelAttribute>(name);
                        break;
                    case TemplateWindow.TemplateType.View:
                        CreateTemplate<ViewAttribute>(name);
                        break;
                }
            };
        }

        private static string GetImplementationType<T>() where T : ImplementationAttribute
        {
            return typeof(T).Name.Replace("Attribute", string.Empty).Replace("iMVC.", string.Empty);
        }

        private static void CreateTemplate<T>(string name) where T : ImplementationAttribute
        {
            FileTemplate template = GetTemplate<T>(name);
            string path = "Assets/Scripts/" + typeof(T).Name.Replace(typeof(Attribute).Name, string.Empty);
            string assetPath = path + "/" + template.Filename;
            
            if (File.Exists(assetPath))
            {
                Debug.LogWarning("Existing implementation (" + template.Filename + ") already created here.");
                return;
            }

            File.WriteAllText(assetPath, template.Content);
            AssetDatabase.ImportAsset(assetPath);
            AssetDatabase.SaveAssets();

            Selection.activeObject = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
        }

        private static FileTemplate GetTemplate<T>(string name) where T : ImplementationAttribute
        {
            string type = GetImplementationType<T>();
            return new FileTemplate
            (
                name + ".cs",
                new[]
                {
                    "using iMVC;\n",
                    "using iMVC.Lifecycle;\n" +
                    "using UnityEngine;\n" +
                    "\n",
                    "[" + type + "]\n",
                    "public class " + name + " : ScriptableObject\n",
                    "{\n" +
                    "    [Setup]\n",
                    "    private void Setup()\n" +
                    "    {\n" +
                    "    \n" +
                    "    }\n",
                    "\n" +
                    "    [Cleanup]\n" +
                    "    private void Cleanup()\n" +
                    "    {\n" +
                    "    \n" +
                    "    }\n",
                    "}\n"
                }
            );
        }

        public class FileTemplate
        {
            public readonly string Filename;
            public readonly string[] Lines;

            public string Content
            {
                get
                {
                    string result = string.Empty;
                    foreach (string line in Lines)
                        result += line;
                    return result;
                }
            }

            public FileTemplate(string filename, string[] lines)
            {
                Filename = filename;
                Lines = lines;
            }
        }
    }

    public class TemplateWindow : EditorWindow
    {
        public Action<string, TemplateType> OnCreate;

        public enum TemplateType
        {
            Controller,
            Model,
            View
        }

        public string Name { get; private set; }
        public TemplateType Type { get; private set; }

        public TemplateWindow()
        {
            titleContent = new GUIContent("Implementation");
            minSize = maxSize = new Vector2(300, 100);
            Name = "NewController";
            Type = TemplateType.Controller;
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            GUILayout.BeginVertical();

            Name = EditorGUILayout.TextField("Name", Name);
            TemplateType lastType = Type;
            Type = (TemplateType) EditorGUILayout.EnumPopup("Type", Type);
            if (lastType != Type && Name.Contains(lastType.ToString()))
                Name = Name.Replace(lastType.ToString(), Type.ToString());

            GUILayout.Space(20);

            GUI.enabled = Name.Length > 0;
            if (GUILayout.Button("Create"))
            {
                if (OnCreate != null)
                    OnCreate(Name, Type);
                Close();
            }

            GUI.enabled = true;

            GUILayout.EndVertical();
        }
    }
}