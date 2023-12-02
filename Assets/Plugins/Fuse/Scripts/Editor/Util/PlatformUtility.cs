using System;
using UnityEditor;

namespace Fuse.Editor
{
    public static class PlatformUtility
    {
        private static BuildTarget _cacheTarget;

        public static void SetPlatform(BuildTarget buildTarget)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(buildTarget.ToFuseBuildTarget().ToBuildTargetGroup(), buildTarget);
        }

        public static Tuple<BuildTarget, BuildTargetGroup> GetPlatform()
        {
            return new Tuple<BuildTarget, BuildTargetGroup>(
                EditorUserBuildSettings.activeBuildTarget,
                EditorUserBuildSettings.activeBuildTarget.ToFuseBuildTarget().ToBuildTargetGroup()
            );
        }

        public static void CacheCurrent()
        {
            _cacheTarget = EditorUserBuildSettings.activeBuildTarget;
        }

        public static void CacheRevert()
        {
            SetPlatform(_cacheTarget);
        }
    }
}