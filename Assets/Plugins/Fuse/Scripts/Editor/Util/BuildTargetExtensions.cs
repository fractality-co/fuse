using System;
using UnityEditor;
using UnityEngine;

namespace Fuse.Editor
{
    /// <summary>
    /// Extension to allow mapping from <see cref="BuildTarget"/>s to runtime platform.
    /// </summary>
    public static class BuildTargetExtensions
    {
        public static RuntimePlatform ToRuntimePlatform(this BuildTarget buildTarget)
        {
            switch (buildTarget)
            {
                case BuildTarget.StandaloneOSX:
                    return RuntimePlatform.OSXPlayer;
                case BuildTarget.StandaloneWindows64:
                case BuildTarget.StandaloneWindows:
                    return RuntimePlatform.WindowsPlayer;
                case BuildTarget.StandaloneLinux64:
                    return RuntimePlatform.LinuxPlayer;
                case BuildTarget.WSAPlayer:
                    return RuntimePlatform.WSAPlayerX86;
                case BuildTarget.tvOS:
                    return RuntimePlatform.tvOS;
                case BuildTarget.iOS:
                    return RuntimePlatform.IPhonePlayer;
                case BuildTarget.Android:
                    return RuntimePlatform.Android;
                case BuildTarget.WebGL:
                    return RuntimePlatform.WebGLPlayer;
                case BuildTarget.PS4:
                    return RuntimePlatform.PS4;
                case BuildTarget.XboxOne:
                    return RuntimePlatform.XboxOne;
                case BuildTarget.Switch:
                    return RuntimePlatform.Switch;
                case BuildTarget.Stadia:
                    return RuntimePlatform.Stadia;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static string GetPlatformName(this BuildTarget buildTarget)
        {
            return buildTarget.ToRuntimePlatform().GetPlatformName();
        }
    }
}