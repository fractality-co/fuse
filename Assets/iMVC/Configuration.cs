using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace iMVC
{
	public class Configuration : ScriptableObject
	{
		public const string AssetName = "iMVC";
		public const string FullAssetName = AssetName + ".asset";
		public const string RootConfigurationPath = "Assets/Resources";
		public const string FullConfigurationPath = RootConfigurationPath + "/" + FullAssetName;
		public const string LoadConfigurationPath = "iMVC";
		public const string FullScriptsPath = "Assets/Scripts";

		private static Configuration _instance;

		public string FullLoadPath
		{
			get { return "Assets/" + LoadPath; }
		}

		public string LoadPath
		{
			get
			{
				switch (_loadMethod)
				{
					case AssetLoadMethod.Resources:
						return "Resources";

					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		[Header("State Machine")] [SerializeField]
		private string _start;

		[SerializeField] private List<State> _states;

		[Header("Implementation Assets")] [SerializeField]
		private AssetLoadMethod _loadMethod = AssetLoadMethod.Resources;

		public static Configuration Load()
		{
#if UNITY_EDITOR
			return AssetDatabase.LoadAssetAtPath<Configuration>(FullConfigurationPath);
#else
			if (_instance == null)
				_instance = Resources.Load<Configuration>(LoadConfigurationPath);
			return _instance;
#endif
		}
	}

	[Serializable]
	internal class State
	{
		public string Name;
	}

	internal enum AssetLoadMethod
	{
		Resources
	}
}