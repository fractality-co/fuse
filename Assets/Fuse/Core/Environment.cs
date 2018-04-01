using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Fuse.Core
{
	public class Environment : ScriptableObject
	{
		public LoadMethod Loading;
		public string HostUri;

		[Tooltip("Negative value turns off version control")]
		public int DefaultVersion = -1;

		public CustomVersion[] CustomVersion;

		public string GetPath(string filePath)
		{
			return string.Format(Constants.AssetsBakedPath, filePath);
		}

		public Uri GetUri(string filePath)
		{
			return new Uri(new Uri(HostUri), new Uri(filePath));
		}

		public int GetVersion(Implementation implementation)
		{
			foreach (CustomVersion custom in CustomVersion)
				if (custom.Bundle == implementation.Bundle)
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