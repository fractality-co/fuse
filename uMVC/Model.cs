using UnityEngine;

namespace uMVC
{
	public abstract class Model : ScriptableObject
	{
		public abstract void Setup();
		public abstract void Cleanup();
	}
}