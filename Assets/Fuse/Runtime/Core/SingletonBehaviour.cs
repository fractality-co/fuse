using UnityEngine;

namespace Fuse.Core
{
	[DisallowMultipleComponent]
	public abstract class SingletonBehaviour<T> : MonoBehaviour where T : MonoBehaviour
	{
		public static T Instance { get; private set; }

		protected bool Valid
		{
			get { return Instance == this; }
		}

		private void Awake()
		{
			if (Instance != null)
			{
				Destroy(gameObject);
				return;
			}

			DontDestroyOnLoad(gameObject);
			Instance = (T) (object) this;
		}
	}
}