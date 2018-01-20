using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace uMVC
{
	public abstract class View : MonoBehaviour
	{
		public const string ShowEvent = "ShowEvent";
		public const string HideEvent = "HideEvent";

		private readonly Dictionary<string, List<Action>> _listeners = new Dictionary<string, List<Action>>();

		[SerializeField] private bool _showOnStart;
		[SerializeField] private string _containerId;
		[SerializeField] private UnityEvent _onShow;
		[SerializeField] private UnityEvent _onHide;

		private bool _loaded;

		public string ContainerId
		{
			get { return _containerId; }
		}

		private void Awake()
		{
			gameObject.SetActive(false);
		}

		private void OnDestroy()
		{
			Unload(false);
		}

		public IEnumerator Load()
		{
			if (_loaded)
				Unload(false);

			yield return Setup();
			_loaded = true;

			if (_showOnStart)
				Show();
		}

		public void Unload(bool destroy = true)
		{
			if (!_loaded)
				return;

			_loaded = false;
			ClearListeners();
			Cleanup();

			if (destroy)
				Destroy(gameObject);
		}

		public void Show()
		{
			if (!_loaded)
				throw new InvalidOperationException("Attempting to show view but it has not been loaded");

			gameObject.SetActive(true);
			_onShow.Invoke();
			Notify(ShowEvent);
		}

		public void Hide()
		{
			if (!_loaded)
				throw new InvalidOperationException("Attemping to hide view but it has not been loaded");

			_onHide.Invoke();
			Notify(HideEvent);
		}

		protected abstract IEnumerator Setup();
		protected abstract void Cleanup();

		// TODO: ideally, this should be protected and not exposed but for now we want it so the Inspector can change
		public void Notify(string type)
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