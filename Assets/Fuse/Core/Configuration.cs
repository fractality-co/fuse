using UnityEngine;

namespace Fuse.Core
{
	/// <summary>
	/// Data relating to your applications functionality then executed by <see cref="Fuse"/>.
	/// </summary>
	public class Configuration : ScriptableObject
	{
		[AssetReference(typeof(State))]
		public string Start;

		[AssetReference(typeof(Environment))]
		public string Environment;
	}
}