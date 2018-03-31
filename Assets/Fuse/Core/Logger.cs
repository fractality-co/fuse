using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace Fuse.Core
{
	/// <summary>
	/// Wrapper class specific for Fuse logging.
	/// </summary>
	public static class Logger
	{
		private const string MessageFormat = "[Fuse] {0}";

		public static bool Enabled
		{
			get { return Debug.unityLogger.logEnabled; }
			set { Debug.unityLogger.logEnabled = value; }
		}

		public static void Info(string message)
		{
			Debug.Log(Format(message));
		}

		public static void Warn(string message)
		{
			Debug.LogWarning(Format(message));
		}

		public static void Error(string message)
		{
			Debug.LogError(Format(message));
		}

		public static void Exception(string message)
		{
#if UNITY_EDITOR
			if (EditorApplication.isPlaying)
				EditorApplication.isPaused = true;
#else
			Application.Quit();
#endif
			throw new Exception(Format(message));
		}

		private static string Format(string message)
		{
			return string.Format(MessageFormat, message);
		}
	}
}