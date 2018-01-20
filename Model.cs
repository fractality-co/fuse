using System.Collections;
using UnityEngine;

namespace uMVC
{
	public abstract class Model : ScriptableObject
	{
		public IEnumerator Load()
		{
			yield return Setup();
		}

		public void Unload()
		{
			Cleanup();
			Destroy(this);
			Resources.UnloadUnusedAssets();
		}

		protected abstract IEnumerator Setup();
		protected abstract void Cleanup();
	}
}