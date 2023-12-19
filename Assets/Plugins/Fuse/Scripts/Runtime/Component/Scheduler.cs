using UnityEngine;

namespace Fuse
{
	/// <summary>
	/// Singleton global behaviour for scheduling coroutines onto.
	/// </summary>
	public class Scheduler : MonoBehaviour
	{
		private static Scheduler _instance;

		private void Awake()
		{
			_instance = this;
			DontDestroyOnLoad(gameObject);
		}

		private static Scheduler Create() { return new GameObject().AddComponent<Scheduler>(); }
		public static Scheduler Get() { return _instance != null ? _instance : Create(); }
	}
}