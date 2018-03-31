using System.Collections;
using UnityEngine;

namespace Fuse.Core
{
	public class Bootstrap : MonoBehaviour, Fuse.IExecutor
	{
		private void Awake()
		{
			if (!Fuse.Running)
			{
				Fuse.Start(this);
				DontDestroyOnLoad(gameObject);
			}
			else
			{
				Logger.Warn("Should only have one " + typeof(Bootstrap).Name + " active, removing this instance (" + name + ")");
				Destroy(gameObject);
			}
		}

		private void OnApplicationQuit()
		{
			DestroyImmediate(gameObject);
		}

		private void OnDestroy()
		{
			Fuse.Stop();
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