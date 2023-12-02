using System;
using UnityEngine;

namespace Fuse
{
    /// <summary>
    /// Wrapper class specific for Fuse logging.
    /// </summary>
    public static class Logger
    {
        private const string MessageFormat = "[Fuse] {0}";

        public static bool Enabled
        {
            get => Debug.unityLogger.logEnabled;
            set => Debug.unityLogger.logEnabled = value;
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
            throw new Exception(Format(message));
        }

        private static string Format(string message)
        {
            return string.Format(MessageFormat, message);
        }
    }
}