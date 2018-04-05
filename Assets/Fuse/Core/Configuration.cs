using UnityEngine;

namespace Fuse.Core
{
	/// <summary>
	/// Data relating to your applications functionality then executed by <see cref="Fuse"/>.
	/// </summary>
	public class Configuration : ScriptableObject
	{
		[AssetReference(typeof(State), Constants.CoreAssetPath)]
		public string Start;

		[AssetReference(typeof(Environment), Constants.CoreAssetPath)]
		public string Environment;
	}
}