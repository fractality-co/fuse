using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace iMVC.Editor
{
	public class ScriptGenerator : AssetPostprocessor
	{
		private const string ImplementationScriptsPath = "Assets/Scripts";
		
		[MenuItem("Window/iMVC/Implementation %#a")]
		[MenuItem("Assets/iMVC/Implementation")]
		public static void CreateTemplate()
		{
			TemplateWindow window = EditorWindow.GetWindow<TemplateWindow>();
			window.OnCreate += (name, type, implements) =>
			{
				switch (type)
				{
					case TemplateWindow.TemplateType.Controller:
						CreateTemplate<ControllerAttribute>(name, implements);
						break;
					case TemplateWindow.TemplateType.Model:
						CreateTemplate<ModelAttribute>(name, implements);
						break;
					case TemplateWindow.TemplateType.View:
						CreateTemplate<ViewAttribute>(name, implements);
						break;
				}
			};
		}

		private static string GetNamespaceName()
		{
			// ReSharper disable once PossibleNullReferenceException
			return typeof(ScriptGenerator).Namespace.Replace(".Editor", string.Empty);
		}

		private static string GetImplementationType<T>() where T : ImplementationAttribute
		{
			return typeof(T).Name.Replace(typeof(Attribute).ToString(), string.Empty)
				.Replace(GetNamespaceName() + ".", string.Empty).Replace(typeof(Attribute).Name, string.Empty);
		}

		private static void CreateTemplate<T>(string name, List<Type> implements) where T : ImplementationAttribute
		{
			FileTemplate template = GetTemplate<T>(name, implements);

			string implementation = typeof(T).Name.Replace(typeof(Attribute).Name, string.Empty);
			string path = ImplementationScriptsPath + "/" + implementation;
			string assetPath = path + "/" + template.Filename;

			if (File.Exists(assetPath))
			{
				Logger.Warn("Existing implementation (" + template.Filename + ") already created here.");
				return;
			}

			Utils.PreparePath(path);
			File.WriteAllText(assetPath, template.Content);
			AssetDatabase.ImportAsset(assetPath);
			AssetDatabase.SaveAssets();

			Selection.activeObject = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
		}

		private static FileTemplate GetTemplate<T>(string name, List<Type> implements) where T : ImplementationAttribute
		{
			List<string> allImplementations = new List<string>();
			int last = implements.Count - 1;
			for (int i = 0; i <= last; i++)
			{
				foreach (string line in GetImplementation(implements[i]))
					allImplementations.Add(line);

				if (i < last)
					allImplementations.Add("\n");
			}

			string type = GetImplementationType<T>();
			return new FileTemplate
			(
				name + ".cs",
				new[]
				{
					implements.Count > 0 ? "using System;\n" : string.Empty,
					"using " + GetNamespaceName() + ";\n",
					"using UnityEngine;\n",
					"\n",
					"[" + type + "]\n",
					"public class " + name + " : ScriptableObject\n",
					"{\n",
					implements.Count > 0 ? FileTemplate.Implements + "\n" : string.Empty,
					"}\n"
				},
				allImplementations.ToArray()
			);
		}

		private static string[] GetImplementation(Type type)
		{
			string name = type.Name.Replace(typeof(Attribute).Name, string.Empty);

			string param = string.Empty;
			if (type == typeof(SubscribeAttribute) || type == typeof(PublishAttribute))
				param = "(\"EventType\")";
			else if (type == typeof(InputAttribute))
				param = "(\"ButtonName\")";
			else if (type == typeof(TickAttribute))
				param = "(1000)";

			return new[]
			{
				"	[" + name + param + "]\n" +
				"	private void " + name + "()\n" +
				"	{\n" +
				"		throw new NotImplementedException();\n" +
				"	}\n"
			};
		}

		public class FileTemplate
		{
			public const string Implements = "{IMPLEMENTS}";

			public readonly string Filename;
			public readonly string[] Lines;
			public readonly string[] Implementations;

			public string Content
			{
				get
				{
					string result = string.Empty;
					foreach (string line in Lines)
					{
						if (line.Contains(Implements))
						{
							if (Implementations.Length == 0)
								result += line.Replace(Implements, string.Empty);
							else
							{
								foreach (string implementLine in Implementations)
									result += implementLine;
							}

							continue;
						}

						result += line;
					}

					return result;
				}
			}

			public FileTemplate(string filename, string[] lines, string[] implementations)
			{
				Filename = filename;
				Lines = lines;
				Implementations = implementations;
			}
		}
	}

	public class TemplateWindow : EditorWindow
	{
		public Action<string, TemplateType, List<Type>> OnCreate;

		public enum TemplateType
		{
			Controller,
			Model,
			View
		}

		public string Name { get; private set; }
		public TemplateType Type { get; private set; }
		public Dictionary<Type, bool> LifecycleImplements { get; private set; }
		public Dictionary<Type, bool> EventsImplements { get; private set; }

		private static readonly Type[] LifecycleAttributes =
		{
			typeof(SetupAttribute),
			typeof(CleanupAttribute)
		};

		private static readonly Type[] EventsAttributes =
		{
			typeof(TickAttribute),
			typeof(InputAttribute),
			typeof(SubscribeAttribute)
		};

		private bool _showLifecycle = true;
		private bool _showEvents = true;

		public TemplateWindow()
		{
			titleContent = new GUIContent("Implementation");
			minSize = new Vector2(300, 250);
			maxSize = new Vector2(minSize.x, 500);
			Name = "NewController";
			Type = TemplateType.Controller;

			LifecycleImplements = new Dictionary<Type, bool>();
			foreach (var type in LifecycleAttributes)
				LifecycleImplements[type] = true;

			EventsImplements = new Dictionary<Type, bool>();
			foreach (var type in EventsAttributes)
				EventsImplements[type] = false;
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

			GUILayout.Space(10);

			_showLifecycle = EditorGUILayout.Foldout(_showLifecycle, "Lifecycle (limited)");
			if (_showLifecycle)
			{
				foreach (var type in LifecycleAttributes)
					LifecycleImplements[type] = GUILayout.Toggle(LifecycleImplements[type],
						type.Name.Replace("Attribute", string.Empty));

				GUILayout.Space(5);
			}

			_showEvents = EditorGUILayout.Foldout(_showEvents, "Events (limited)");
			if (_showEvents)
			{
				foreach (var type in EventsAttributes)
					EventsImplements[type] = GUILayout.Toggle(EventsImplements[type],
						type.Name.Replace("Attribute", string.Empty));
			}

			GUILayout.Space(10);
			GUILayout.Label("For all features, please open script with Editor.");
			GUILayout.Space(20);

			GUILayout.FlexibleSpace();
			GUI.enabled = Name.Length > 0;
			if (GUILayout.Button("Create"))
			{
				List<Type> implements = new List<Type>();
				foreach (KeyValuePair<Type, bool> lifecycle in LifecycleImplements)
					if (lifecycle.Value)
						implements.Add(lifecycle.Key);

				foreach (KeyValuePair<Type, bool> events in EventsImplements)
					if (events.Value)
						implements.Add(events.Key);

				if (OnCreate != null)
					OnCreate(Name, Type, implements);
				Close();
			}

			GUI.enabled = true;

			GUILayout.EndVertical();
		}
	}
}