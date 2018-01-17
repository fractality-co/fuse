using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace uMVC
{
	public class Application : MonoBehaviour
	{
		[Header("View")]
		public Canvas WorldCanvas;
		public Canvas ScreenCanvas;

		[Header("Controller")]
		[TypeReference(typeof(Controller))]
		public string[] Controllers;

		private readonly List<Controller> _instances = new List<Controller>();

		private void Awake()
		{
			foreach (string controllerType in Controllers)
			{
				Type type = Type.GetType(controllerType);
				if (type == null)
					throw new Exception("Invalid Controller type passed: " + controllerType);

				_instances.Add((Controller) Activator.CreateInstance(type));
			}
		}

		private IEnumerator Start()
		{
			foreach (Controller instance in _instances)
				yield return instance.Load();

			_instances.ForEach(instance => instance.Setup());
		}

		private void OnDestroy()
		{
			_instances.ForEach(instance => instance.Cleanup());
			_instances.Clear();
		}
	}
}