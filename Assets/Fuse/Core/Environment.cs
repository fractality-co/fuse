using System;
using JetBrains.Annotations;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace Fuse.Core
{
	public class Environment : ScriptableObject
	{
		public LoadMethod Loading;

		[Tooltip("The root host uri for downloading assets, note that asset bundles loaded will append the platform")]
		public string HostUri;

		[Tooltip("Negative value turns off version control")]
		public int DefaultVersion = -1;

		public CustomVersion[] CustomVersion;

		public string GetPath(string filePath)
		{
			return string.Format(Constants.AssetsBakedPath, filePath);
		}

		public Uri GetUri(string filePath, bool platformUnique)
		{
			string subPath = string.Empty;

			if (platformUnique)
			{
#if UNITY_EDITOR
				subPath = Constants.GetPlatformName(EditorUserBuildSettings.activeBuildTarget) + "/";
#else
				subPath = Constants.GetPlatformName(Application.platform) + "/";
#endif
			}

			return new Uri(HostUri + "/" + subPath + filePath);
		}

		public int GetVersion(string bundle)
		{
			foreach (CustomVersion custom in CustomVersion)
				if (custom.Bundle == bundle)
					return custom.Version;

			return DefaultVersion;
		}
	}

	[Serializable]
	public class CustomVersion
	{
		[AssetBundleReference]
		public string Bundle;

		[UsedImplicitly, Tooltip("Negative value turns off version control")]
		public int Version = -1;
	}

	public enum LoadMethod
	{
		Baked,
		Online
	}
}

#if UNITY_EDITOR
public static class BuildTargetExtensions
{
	public static RuntimePlatform ToRuntimePlatform(this BuildTarget buildTarget)
	{
		switch (buildTarget)
		{
			case BuildTarget.StandaloneOSX:
				return RuntimePlatform.OSXPlayer;
			case BuildTarget.StandaloneWindows64:
			case BuildTarget.StandaloneWindows:
				return RuntimePlatform.WindowsPlayer;
			case BuildTarget.iOS:
				return RuntimePlatform.IPhonePlayer;
			case BuildTarget.Android:
				return RuntimePlatform.Android;
			case BuildTarget.StandaloneLinux:
			case BuildTarget.StandaloneLinux64:
			case BuildTarget.StandaloneLinuxUniversal:
				return RuntimePlatform.LinuxPlayer;
			case BuildTarget.WebGL:
				return RuntimePlatform.WebGLPlayer;
			case BuildTarget.WSAPlayer:
				return RuntimePlatform.WSAPlayerX86;
			case BuildTarget.Tizen:
				return RuntimePlatform.TizenPlayer;
			case BuildTarget.PSP2:
				return RuntimePlatform.PSP2;
			case BuildTarget.PS4:
				return RuntimePlatform.PS4;
			case BuildTarget.PSM:
				return RuntimePlatform.PSM;
			case BuildTarget.XboxOne:
				return RuntimePlatform.XboxOne;
			case BuildTarget.WiiU:
				return RuntimePlatform.WiiU;
			case BuildTarget.tvOS:
				return RuntimePlatform.tvOS;
			case BuildTarget.Switch:
				return RuntimePlatform.Switch;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}
}
#endif