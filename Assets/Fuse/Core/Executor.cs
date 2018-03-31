using System.Collections;
using UnityEngine;

namespace Fuse.Core
{
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

	public class Executor : SingletonBehaviour<Executor>, Fuse.IExecutor
	{
		private void OnEnable()
		{
			if (Valid) Fuse.Start(this);
		}

		private void OnDisable()
		{
			if (Valid) Fuse.Stop();
		}

		public Coroutine StartJob(IEnumerator method)
		{
			return StartCoroutine(method);
		}

		public void StopJob(Coroutine coroutine)
		{
			StopCoroutine(coroutine);
		}

		public void StopAllJobs()
		{
			StopAllCoroutines();
		}
	}
}