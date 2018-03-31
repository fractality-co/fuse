using System;
using System.Collections.Generic;
using System.IO;
using Fuse.Core;
using Fuse.Implementation;
using UnityEditor;
using UnityEngine;
using Logger = Fuse.Core.Logger;

namespace Fuse.Editor
{
	/// <summary>
	/// Handles the generation and post-processing of scripts for <see cref="Fuse"/>.
	/// </summary>
	public class ScriptProcessor : AssetPostprocessor
	{
		private const string DevelopMenuItem = "Fuse/Mode/Develop %&d";
		private const string ReleaseMenuItem = "Fuse/Mode/Release %&r";
		private const string ReleaseDefine = "RELEASE";

		private static bool ReleaseMode
		{
			get { return EditorUserBuildSettings.selectedBuildTargetGroup.HasScriptingDefine(ReleaseDefine); }
		}

		[MenuItem(DevelopMenuItem)]
		public static void SetDevelopMode()
		{
			if (EditorUserBuildSettings.selectedBuildTargetGroup.HasScriptingDefine(ReleaseDefine))
				EditorUserBuildSettings.selectedBuildTargetGroup.RemoveScriptingDefine(ReleaseDefine);
		}

		[MenuItem(ReleaseMenuItem)]
		public static void SetReleaseMode()
		{
			if (!EditorUserBuildSettings.selectedBuildTargetGroup.HasScriptingDefine(ReleaseDefine))
				EditorUserBuildSettings.selectedBuildTargetGroup.AddScriptingDefine(ReleaseDefine);
		}

		[MenuItem(ReleaseMenuItem, true)]
		public static bool SetReleaseModeValidate()
		{
			Menu.SetChecked(DevelopMenuItem, !ReleaseMode);
			Menu.SetChecked(ReleaseMenuItem, ReleaseMode);
			return true;
		}

		[MenuItem("Fuse/New/Implementation %&i")]
		public static void ShowCreateImplementationWindow()
		{
			CreateImplementationWindow window = EditorWindow.GetWindow<CreateImplementationWindow>();
			window.OnCreate += CreateImplementation<ImplementationAttribute>;
		}

		private static string GetNamespaceName()
		{
			return typeof(ImplementationAttribute).Namespace;
		}

		private static string GetImplementationType<T>() where T : ImplementationAttribute
		{
			return typeof(T).Name.Replace(typeof(Attribute).ToString(), string.Empty)
				.Replace(GetNamespaceName() + ".", string.Empty).Replace(typeof(Attribute).Name, string.Empty);
		}

		private static void CreateImplementation<T>(string name, List<Type> implements) where T : ImplementationAttribute
		{
			FileTemplate template = GetTemplate<T>(name, implements);

			string implementation = typeof(T).Name.Replace(typeof(Attribute).Name, string.Empty);
			string path = Constants.ImplementationScriptsPath + "/" + implementation;
			string assetPath = path + "/" + template.Filename;

			if (File.Exists(assetPath))
			{
				Logger.Warn("Existing implementation (" + template.Filename + ") already created here.");
				return;
			}

			EditorUtils.PreparePath(path);
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

		private class FileTemplate
		{
			public const string Implements = "{IMPLEMENTS}";

			public readonly string Filename;

			public string Content
			{
				get
				{
					string result = string.Empty;
					foreach (string line in _lines)
					{
						if (line.Contains(Implements))
						{
							if (_implementations.Length == 0)
								result += line.Replace(Implements, string.Empty);
							else
							{
								foreach (string implementLine in _implementations)
									result += implementLine;
							}

							continue;
						}

						result += line;
					}

					return result;
				}
			}

			private readonly string[] _lines;
			private readonly string[] _implementations;

			public FileTemplate(string filename, string[] lines, string[] implementations)
			{
				Filename = filename;
				_lines = lines;
				_implementations = implementations;
			}
		}
	}

	public class CreateImplementationWindow : EditorWindow
	{
		public Action<string, List<Type>> OnCreate;

		private string Name { get; set; }
		private Dictionary<Type, bool> LifecycleImplements { get; set; }
		private Dictionary<Type, bool> EventsImplements { get; set; }

		private static readonly Type[] LifecycleAttributes =
		{
			typeof(SetupAttribute),
			typeof(CleanupAttribute)
		};

		private static readonly Type[] EventsAttributes =
		{
			typeof(TickAttribute),
			typeof(SubscribeAttribute)
		};

		private bool _showLifecycle = true;
		private bool _showEvents = true;

		public CreateImplementationWindow()
		{
			titleContent = new GUIContent("Implementation");
			minSize = new Vector2(300, 250);
			maxSize = new Vector2(minSize.x, 500);
			Name = "";

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

			GUILayout.Space(10);

			_showLifecycle = EditorGUILayout.Foldout(_showLifecycle, "Lifecycle");
			if (_showLifecycle)
			{
				foreach (var type in LifecycleAttributes)
					LifecycleImplements[type] = GUILayout.Toggle(LifecycleImplements[type],
						type.Name.Replace("Attribute", string.Empty));

				GUILayout.Space(5);
			}

			_showEvents = EditorGUILayout.Foldout(_showEvents, "Events");
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
					OnCreate(Name, implements);
				Close();
			}

			GUI.enabled = true;

			GUILayout.EndVertical();
		}
	}
}