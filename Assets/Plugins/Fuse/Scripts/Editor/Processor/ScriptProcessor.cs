using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace Fuse.Editor
{
	/// <summary>
	/// Handles the generation and post-processing of scripts for <see cref="Fuse" />.
	/// </summary>
	public class ScriptProcessor : AssetPostprocessor
	{
		public const string BaseModulePath = "Assets/Scripts/Module";
		public static readonly string ModuleScriptsPath = BaseModulePath + "/{0}";

		private static string GetNamespaceName() { return typeof(Module).Namespace; }

		public static void CreateModule(string name, List<Type> implements)
		{
			var template = GetTemplate(name, implements);
			var path = string.Format(ModuleScriptsPath, template.Filename);
			if (File.Exists(path))
			{
				Logger.Warn("Existing Module (" + template.Filename + ") already created here.");
				return;
			}

			PathUtility.PreparePath(BaseModulePath);
			File.WriteAllText(path, template.Content);
			AssetDatabase.ImportAsset(path);
			AssetDatabase.SaveAssets();
		}

		private static FileTemplate GetTemplate(string name, List<Type> implements)
		{
			var allModules = new List<string>();
			var last = implements.Count - 1;
			for (var i = 0; i <= last; i++)
			{
				foreach (var line in GetModule(implements[i]))
					allModules.Add(line);

				if (i < last)
					allModules.Add("\n");
			}

			return new FileTemplate
			(
				name + ".cs",
				new[]
				{
					implements.Count > 0 ? "using System;\n" : string.Empty,
					implements.Contains(typeof(CoroutineAttribute)) ? "using System.Collections;" : string.Empty,
					$"using {GetNamespaceName()};\n",
					"\n",
					$"public class {name} : {nameof(Module)}\n",
					"{\n",
					implements.Count > 0 ? FileTemplate.Implements + "\n" : string.Empty,
					"}\n"
				},
				allModules.ToArray()
			);
		}

		private static string[] GetModule(Type type)
		{
			string name = type.Name.Replace(nameof(Attribute), string.Empty);
			string method = name;
			if (type == typeof(InvokeAttribute))
				method = "Start";

			string param = string.Empty;
			if (type == typeof(SubscribeAttribute))
				param = "(\"category.id\")";

			string returnValue = "void";
			if (type == typeof(CoroutineAttribute))
				returnValue = "IEnumerator";

			return new[]
			{
				"    [" + name + param + "]\n" +
				"    private " + returnValue + " " + method + "()\n" +
				"    {\n" +
				"        throw new NotImplementedException();\n" +
				"    }\n"
			};
		}

		private class FileTemplate
		{
			public const string Implements = "{IMPLEMENTS}";
			private readonly string[] _features;
			private readonly string[] _lines;
			public readonly string Filename;

			public FileTemplate(string filename, string[] lines, string[] features)
			{
				Filename = filename;
				_lines = lines;
				_features = features;
			}

			public string Content
			{
				get
				{
					string result = string.Empty;
					foreach (string line in _lines)
					{
						if (line.Contains(Implements))
						{
							if (_features.Length == 0)
								result += line.Replace(Implements, string.Empty);
							else
								foreach (string implementLine in _features)
									result += implementLine;

							continue;
						}

						result += line;
					}

					return result;
				}
			}
		}
	}
}