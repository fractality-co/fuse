using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace uMVC
{
	public abstract class View : MonoBehaviour
	{
		private readonly Dictionary<string, List<Action<object>>> _listeners = new Dictionary<string, List<Action<object>>>();

		private IEnumerator Start()
		{
			yield return Load();
			Setup();
		}

		private void OnDestroy()
		{
			ClearListeners();
			Cleanup();
		}

		protected abstract IEnumerator Load();
		protected abstract void Setup();
		protected abstract void Cleanup();

		public void Notify<T>(string type, T param = default(T))
		{
			GetListeners(type).ForEach(callback => { callback(param); });
		}

		public void AddListener<T>(string type, Action<T> response)
		{
			GetListeners(type).Add(response as Action<object>);
		}

		public void RemoveListener<T>(string type, Action<T> response)
		{
			GetListeners(type).Remove(response as Action<object>);
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

		private List<Action<object>> GetListeners(string type)
		{
			if (!_listeners.ContainsKey(type))
				_listeners[type] = new List<Action<object>>();

			return _listeners[type];
		}
	}
}