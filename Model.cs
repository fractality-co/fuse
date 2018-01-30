using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace uMVC
{
	public abstract class Model : ScriptableObject
	{
		private readonly Dictionary<string, List<Action>> _listeners = new Dictionary<string, List<Action>>();

		private static Unifier _main;

		private static Unifier Unifier
		{
			get { return _main ?? (_main = FindObjectOfType<Unifier>()); }
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
		}

		protected abstract IEnumerator Setup();
		protected abstract void Cleanup();

		protected Coroutine StartCoroutine(IEnumerator routine)
		{
			return Unifier.StartCoroutine(routine);
		}

		protected void StopCoroutine(Coroutine routine)
		{
			Unifier.StopCoroutine(routine);
		}

		protected IEnumerator LoadModel<T>(string path, Action<T> onComplete, Action<float> onProgress = null) where T : Model
		{
			yield return Unifier.LoadModel(path, onComplete, onProgress);
		}

		protected bool UnloadModel<T>(T model) where T : Model
		{
			return Unifier.UnloadModel(model);
		}

		protected void Notify(string type)
		{
			GetListeners(type).ForEach(callback => { callback(); });
		}

		public void AddListener(string type, Action response)
		{
			GetListeners(type).Add(response);
		}

		public void RemoveListener(string type, Action response)
		{
			GetListeners(type).Remove(response);
		}

		public void ClearListeners(string type)
		{
			if (_listeners.ContainsKey(type))
				_listeners[type].Clear();
		}

		public void ClearListeners()
		{
			_listeners.Clear();
		}

		private List<Action> GetListeners(string type)
		{
			if (!_listeners.ContainsKey(type))
				_listeners[type] = new List<Action>();

			return _listeners[type];
		}
	}
}