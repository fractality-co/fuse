using System;
using UnityEditor;
using UnityEngine;

namespace Fuse.Editor
{
    /// <summary>
    /// Handles the post-processing for build settings.
    /// </summary>
    public class BuildSettingsProcessor : BuildProcessor
    {
        public new const int CallbackOrder = 100;

        protected override int Callback => CallbackOrder;

        private Texture2D[] _icons;
        private bool _development;
        private AndroidBuildType _androidBuildType;
        private XcodeBuildConfig _iOSBuildType;
        private XboxBuildSubtarget _xboxBuildTarget;
        private PS4BuildSubtarget _ps4BuildTarget;

        public override void PreProcessPlayer(Environment environment, BuildTarget buildTarget,
            BuildTargetGroup buildTargetGroup)
        {
            if (environment.icon != null)
            {
                _icons = PlayerSettings.GetIconsForTargetGroup(BuildTargetGroup.Unknown);
                PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, new[] {environment.icon});
            }

            _xboxBuildTarget = EditorUserBuildSettings.xboxBuildSubtarget;
            EditorUserBuildSettings.xboxBuildSubtarget = GetXboxBuildSubtarget(environment);

            _ps4BuildTarget = EditorUserBuildSettings.ps4BuildSubtarget;
            EditorUserBuildSettings.ps4BuildSubtarget = GetPS4BuildSubtarget(environment);

            _development = EditorUserBuildSettings.development;
            EditorUserBuildSettings.development = IsDevelopment(environment);

            _androidBuildType = EditorUserBuildSettings.androidBuildType;
            EditorUserBuildSettings.androidBuildType = GetAndroidBuildType(environment);

            _iOSBuildType = EditorUserBuildSettings.iOSXcodeBuildConfig;
            EditorUserBuildSettings.iOSXcodeBuildConfig = GetiOSBuildType(environment);
        }

        public override void PostProcessPlayer(Environment environment,
            BuildTarget buildTarget,
            BuildTargetGroup buildTargetGroup)
        {
            if (_icons != null)
                PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, _icons);

            EditorUserBuildSettings.development = _development;
            EditorUserBuildSettings.androidBuildType = _androidBuildType;
            EditorUserBuildSettings.iOSXcodeBuildConfig = _iOSBuildType;
            EditorUserBuildSettings.xboxBuildSubtarget = _xboxBuildTarget;
            EditorUserBuildSettings.ps4BuildSubtarget = _ps4BuildTarget;
        }

        private PS4BuildSubtarget GetPS4BuildSubtarget(Environment environment)
        {
            switch (environment.mode)
            {
                case RuntimeMode.Develop:
                    return PS4BuildSubtarget.Package;
                case RuntimeMode.Release:
                    return PS4BuildSubtarget.Iso;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static bool IsDevelopment(Environment environment)
        {
            switch (environment.mode)
            {
                case RuntimeMode.Develop:
                    return true;
                case RuntimeMode.Release:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static AndroidBuildType GetAndroidBuildType(Environment environment)
        {
            switch (environment.mode)
            {
                case RuntimeMode.Develop:
                    return AndroidBuildType.Debug;
                case RuntimeMode.Release:
                    return AndroidBuildType.Release;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static XcodeBuildConfig GetiOSBuildType(Environment environment)
        {
            switch (environment.mode)
            {
                case RuntimeMode.Develop:
                    return XcodeBuildConfig.Debug;
                case RuntimeMode.Release:
                    return XcodeBuildConfig.Release;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static XboxBuildSubtarget GetXboxBuildSubtarget(Environment environment)
        {
            switch (environment.mode)
            {
                case RuntimeMode.Develop:
                    return XboxBuildSubtarget.Debug;
                case RuntimeMode.Release:
                    return XboxBuildSubtarget.Master;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}