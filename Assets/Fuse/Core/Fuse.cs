using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Fuse.Core
{
	/// <summary>
	/// Core executor and bootstrap for the framework.
	/// You should not be interacting with this.
	/// </summary>
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	public class Fuse : MonoBehaviour
	{
		private void Start()
		{
			FuseLogger.Info("Started Fuse");
		}

		private void OnApplicationQuit()
		{
			FuseLogger.Info("Application quitting; immediately stopping Fuse ...");
			DestroyImmediate(gameObject);
		}

		private void OnDestroy()
		{
			Clean();
			FuseLogger.Info("Stopped Fuse");
		}

		private void Clean()
		{
			Resources.UnloadUnusedAssets();
			GC.Collect();
		}
	}
}