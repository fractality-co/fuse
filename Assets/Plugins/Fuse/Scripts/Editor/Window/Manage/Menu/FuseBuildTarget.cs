using System;
using UnityEditor;

namespace Fuse.Editor
{
    /// <summary>
    /// Copy of the Unity build targets.
    /// </summary>
    [Flags]
    public enum FuseBuildTarget
    {
        None = 0,
        StandaloneOSX = 1,
        StandaloneWindows = 2,
        iOS = 4,
        Android = 8,
        StandaloneWindows64 = 16,
        WebGL = 32,
        WSAPlayer = 64,
        StandaloneLinux64 = 128,
        PS4 = 256,
        XboxOne = 512,
        tvOS = 1024,
        Switch = 2048,
        Lumin = 4096,
        Stadia = 8192,
        CloudRendering = 16384,
    }

    public static class FuseBuildTargetExtensions
    {
        public static BuildTarget ToBuildTarget(this FuseBuildTarget fuseBuildTarget)
        {
            return (BuildTarget) Enum.Parse(typeof(BuildTarget), fuseBuildTarget.ToString(), true);
        }

        public static BuildTargetGroup ToBuildTargetGroup(this FuseBuildTarget fuseBuildTarget)
        {
            switch (fuseBuildTarget)
            {
                case FuseBuildTarget.StandaloneOSX:
                case FuseBuildTarget.StandaloneWindows:
                case FuseBuildTarget.StandaloneWindows64:
                case FuseBuildTarget.StandaloneLinux64:
                    return BuildTargetGroup.Standalone;
                default:
                    return (BuildTargetGroup) Enum.Parse(typeof(BuildTargetGroup), fuseBuildTarget.ToString(), true);
            }
        }

        public static FuseBuildTarget ToFuseBuildTarget(this BuildTarget buildTarget)
        {
            return (FuseBuildTarget) Enum.Parse(typeof(FuseBuildTarget), buildTarget.ToString(), true);
        }
    }
}