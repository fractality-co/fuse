using UnityEngine;

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

		private static string Format(string message)
		{
			return string.Format(MessageFormat, message);
		}
	}
}