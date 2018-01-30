using System;
using System.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

namespace uMVC
{
	public abstract class Controller
	{
		private static Unifier _main;

		private static Unifier Unifier
		{
			get { return _main ?? (_main = Object.FindObjectOfType<Unifier>()); }
		}

		public bool Active { get; private set; }

		public IEnumerator Load()
		{
			yield return Setup();
			Active = true;
		}

		public void Unload()
		{
			Active = false;
			Cleanup();
			Resources.UnloadUnusedAssets();
		}

		protected abstract IEnumerator Setup();
		protected abstract void Cleanup();

		protected void ChangeState(Enum state, bool clearAllControllers = false)
		{
			ChangeState(state.ToString(), clearAllControllers);
		}

		protected void ChangeState(string state, bool clearAllControllers = false)
		{
			Unifier.ChangeState(state, clearAllControllers);
		}

		protected Coroutine StartCoroutine(IEnumerator routine)
		{
			return Unifier.StartCoroutine(routine);
		}

		protected void StopCoroutine(Coroutine routine)
		{
			Unifier.StopCoroutine(routine);
		}

		protected IEnumerator LoadModel<T>(Action<T> onComplete, Action<float> onProgress = null) where T : Model
		{
			yield return Unifier.LoadModel(onComplete, onProgress);
		}

		protected IEnumerator LoadModel<T>(string path, Action<T> onComplete, Action<float> onProgress = null) where T : Model
		{
			yield return Unifier.LoadModel(path, onComplete, onProgress);
		}

		protected bool UnloadModel<T>(string path) where T : Model
		{
			return Unifier.UnloadModel<T>(path);
		}

		protected bool UnloadModel<T>(T model) where T : Model
		{
			return Unifier.UnloadModel(model);
		}

		protected IEnumerator LoadView<T>(string path, Action<T> onComplete, Action<float> onProgress = null) where T : View
		{
			yield return Unifier.LoadView(path, onComplete, onProgress);
		}

		protected bool UnloadView<T>(string path) where T : View
		{
			return Unifier.UnloadView<T>(path);
		}

		protected bool UnloadView<T>(T view) where T : View
		{
			return Unifier.UnloadView(view);
		}
	}
}