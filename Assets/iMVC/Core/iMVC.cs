using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace iMVC
{
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	public class iMVC : MonoBehaviour
	{
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