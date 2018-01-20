using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace uMVC
{
	public abstract class Controller
	{
		private static Unifier _main;

		private static Unifier Main
		{
			get { return _main ?? (_main = Object.FindObjectOfType<Unifier>()); }
		}

		private static readonly Dictionary<string, Model> LoadedModels = new Dictionary<string, Model>();
		private static readonly Dictionary<string, View> LoadedViews = new Dictionary<string, View>();

		public abstract IEnumerator Setup();
		public abstract void Cleanup();

		protected void ChangeState(Enum state)
		{
			ChangeState(state.ToString());
		}

		protected void ChangeState(string state)
		{
			Main.ChangeState(state);
		}

		protected IEnumerator LoadModel<T>(string path, Action<T> onComplete, Action<float> onProgress = null) where T : Model
		{
			if (LoadedModels.ContainsKey(path))
			{
				if (onProgress != null) onProgress(1f);
				onComplete(LoadedModels[path] as T);
				yield break;
			}

			T model = null;
			yield return LoadingAsset<T>
			(
				path,
				result => { model = result; },
				onProgress
			);

			yield return model.Load();

			LoadedModels[path] = model;
			onComplete(model);
		}

		protected void UnloadModel<T>(T model) where T : Model
		{
			KeyValuePair<string, Model> reference;
			foreach (KeyValuePair<string, Model> pair in LoadedModels)
			{
				if (pair.Value != model) continue;
				reference = pair;
				break;
			}

			model.Unload();
			LoadedModels.Remove(reference.Key);
			Resources.UnloadUnusedAssets();
		}

		protected IEnumerator LoadView<T>(string path, Action<T> onComplete, Action<float> onProgress = null) where T : View
		{
			if (LoadedViews.ContainsKey(path))
			{
				if (onProgress != null) onProgress(1f);
				onComplete(LoadedViews[path] as T);
				yield break;
			}

			GameObject viewAsset = null;
			yield return LoadingAsset<GameObject>
			(
				path,
				asset => { viewAsset = asset; },
				onProgress
			);

			T view = Object.Instantiate(viewAsset).GetComponent<T>();
			view.transform.SetParent(Main.GetContainer(view.ContainerId), false);
			view.gameObject.name = view.gameObject.name.Replace("(Clone)", string.Empty);

			yield return view.Load();

			LoadedViews[path] = view;
			onComplete(view);
		}

		protected void UnloadView<T>(T view) where T : View
		{
			KeyValuePair<string, View> reference;
			foreach (KeyValuePair<string, View> pair in LoadedViews)
			{
				if (pair.Value != view) continue;
				reference = pair;
				break;
			}

			view.Unload();
			LoadedViews.Remove(reference.Key);
			Resources.UnloadUnusedAssets();
		}

		private void LoadAsset<T>(string path, Action<T> onComplete, Action<float> onProgress = null) where T : Object
		{
			Main.StartCoroutine(LoadingAsset(path, onComplete, onProgress));
		}

		private static IEnumerator LoadingAsset<T>(string path, Action<T> onComplete, Action<float> onProgress = null)
			where T : Object
		{
			ResourceRequest request = Resources.LoadAsync<T>(path);
			while (!request.isDone)
			{
				yield return request;

				if (onProgress != null)
					onProgress(request.progress);
			}
			onComplete(request.asset as T);
		}

		private void UnloadAsset<T>(T reference) where T : Object
		{
			Resources.UnloadAsset(reference);
			Resources.UnloadUnusedAssets();
		}
	}
}