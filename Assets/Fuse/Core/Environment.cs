using System;
using Fuse.Implementation;
using JetBrains.Annotations;
using UnityEngine;

namespace Fuse.Core
{
	public class Environment : ScriptableObject
	{
		public LoadMethod Loading;
		public string HostUri;
		public uint DefaultVersion;
		public CustomVersion[] CustomVersion;

		public string GetPath(string filePath)
		{
			return string.Format(Constants.AssetsBakedPath, filePath);
		}

		public Uri GetUri(string filePath)
		{
			return new Uri(new Uri(HostUri), new Uri(filePath));
		}

		public uint GetVersion(Implementation implementation)
		{
			foreach (CustomVersion custom in CustomVersion)
				if (custom.Implementation == implementation.Type)
					return custom.Version;

			return DefaultVersion;
		}
	}

	[Serializable]
	public class CustomVersion
	{
		[AttributeTypeReference(typeof(ImplementationAttribute))]
		public string Implementation;

		[UsedImplicitly]
		public uint Version;
	}

	public enum LoadMethod
	{
		Baked,
		Online
	}
}