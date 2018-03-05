﻿using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace iMVC
{
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	public class iMVC : MonoBehaviour
	{
		private Configuration _config;

		private void Awake()
		{
			_config = Configuration.Load();

			if (_config == null)
			{
				Logger.Error("Unable to start iMVC, can't load core Configuration!");
				Destroy(gameObject);
			}
		}

		private void Start()
		{
			Logger.Info("Started iMVC");
		}

		private void OnApplicationQuit()
		{
			Logger.Info("Application quitting; immediately stopping iMVC ...");
			DestroyImmediate(gameObject);
		}

		private void OnDestroy()
		{
			_config = null;
			Clean();

			Logger.Info("Stopped iMVC");
		}

		private void Clean()
		{
			Resources.UnloadUnusedAssets();
			GC.Collect();
		}
	}
}