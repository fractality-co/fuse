using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using UnityEditorInternal;

namespace Fuse.Editor
{
    /// <summary>
    /// Handles the pipeline for builds.
    /// </summary>
    public static class PlayerProcessor
    {
        private static bool CreateXcodeProject => EditorUserBuildSettings.GetPlatformSettings(BuildPipeline.GetBuildTargetName(BuildTarget.StandaloneOSX), "CreateXcodeProject").ToLower() == "true";

        private static Environment _environment;

        /// <summary>
        /// Runs the build pipeline for creating the player on the current active build target.
        /// For command line, you can override the environment by passing the name you want to select as: '-environment "Debug"'.
        /// </summary>
        public static void Build()
        {
            if (InternalEditorUtility.inBatchMode)
                AttemptFetchEnvironment();

            Configuration configuration =
                AssetUtility.FetchByPath<Configuration>(Constants.GetConfigurationAssetPath());
            Environment environment = AssetUtility.FetchByPath<Environment>(configuration.Environment);

            var buildTarget = PlatformUtility.GetPlatform();
            Build(buildTarget.Item1, buildTarget.Item2, environment);
        }

        /// <summary>
        /// Runs the build pipeline for creating the player on the build target.
        /// This command will automatically prepare assets as necessary.
        /// </summary>
        public static BuildReport Build(BuildTarget buildTarget, BuildTargetGroup buildTargetGroup,
            Environment environment)
        {
            if (!BuildPipeline.IsBuildTargetSupported(buildTargetGroup, buildTarget))
            {
                if (!InternalEditorUtility.inBatchMode)
                    throw new Exception(
                        "Unable to build for target because this build target is not installed in this editor.");

                Logger.Error("Unable to build for target because this build target is not installed in this editor.");
                EditorApplication.Exit(1);
                return null;
            }

            if (environment == null)
            {
                if (!InternalEditorUtility.inBatchMode)
                    throw new Exception(
                        "Unable to build player; no environment defined in configuration or valid one passed");

                Logger.Error("Unable to build player, no environment assigned in configuration or valid one passed.");
                EditorApplication.Exit(1);
                return null;
            }

            foreach (BuildProcessor processor in BuildProcessor.Processors)
                processor.PreProcessPlayer(environment, buildTarget, buildTargetGroup);

            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                    scenes.Add(scene);
            }

            _environment = environment;

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes.Select(scene => scene.path).ToArray(),
                target = buildTarget,
                targetGroup = buildTargetGroup,
                locationPathName = GetOutputPath(buildTarget, _environment),
                options = (UnityEditor.BuildOptions) environment.build
            });
            
            if (report.summary.totalErrors > 0)
                Logger.Error("Encountered errors when trying to build player; please resolve and try again");
            else
                Logger.Info("Built player for: " + buildTarget.GetPlatformName());

            AssetProcessor.Cleanup();
            return report;
        }

        [PostProcessBuild]
        private static void PostProcess(BuildTarget target, string pathToBuiltProject)
        {
            foreach (BuildProcessor processor in BuildProcessor.Processors)
                processor.PostProcessPlayer(_environment, target, target.ToFuseBuildTarget().ToBuildTargetGroup());
        }

        /// <summary>
        /// Returns the location of where assets are built to.
        /// </summary>
        public static string GetOutputPath(BuildTarget buildTarget, Environment environment)
        {
            var fileOrDirectoryOutput = PlayerSettings.productName + GetExtension(buildTarget, environment);
            return Path.Combine(Constants.EditorBuildsPath, environment.name, buildTarget.GetPlatformName(),
                fileOrDirectoryOutput);
        }

        /// <summary>
        /// Since Unity does not give us access in anyway what the output they have internally, we have to build our own mapping.
        /// </summary>
        private static string GetExtension(BuildTarget buildTarget, Environment environment)
        {
            switch (buildTarget)
            {
                case BuildTarget.StandaloneOSX:
                    return IsProject(buildTarget, environment) ? ".xcodeproj" : ".app";
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return ".exe";
                case BuildTarget.Android:
                    return IsProject(buildTarget, environment) ? string.Empty : ".apk";
                case BuildTarget.iOS:
                case BuildTarget.tvOS:
                    return string.Empty; // requires you to always generate a project and use Apple's ecosystem
                case BuildTarget.XboxOne:
                case BuildTarget.WSAPlayer:
                    return string.Empty; // requires you to always generate a project and use Microsoft's ecosystem
                case BuildTarget.StandaloneLinux64:
                    return ".x86_64";
                case BuildTarget.WebGL:
                    return string.Empty; // always generates a folder with the contents inside it
                case BuildTarget.PS4:
                    return ".pkg";
                case BuildTarget.Switch:
                    return ".nsp";
                case BuildTarget.Stadia:
                    return string.Empty; // always generates a folder with the contents inside it
                case BuildTarget.LinuxHeadlessSimulation:
                    return string.Empty; // always generates a folder with the contents inside it
                default:
                    // by default, we assume it's going to export a directory or project
                    // for you to use; if this is not the case please use the support menu to reach out
                    // for a support request and we will work to address this ASAP!
                    return string.Empty;
            }
        }

        private static bool IsProject(BuildTarget buildTarget, Environment environment)
        {
            switch (buildTarget)
            {
                case BuildTarget.StandaloneOSX:
                    return CreateXcodeProject;
                case BuildTarget.Android:
                    return EditorUserBuildSettings.exportAsGoogleAndroidProject;
                default:
                    throw new NotSupportedException("Unknown mapping for determining if platform is set as project.");
            }
        }

        /// <summary>
        /// Attempt to fetch the environment from command line arguments or from environment variable.
        /// If available, this will override environment set in <see cref="Configuration"/>.
        /// </summary>
        public static void AttemptFetchEnvironment()
        {
            Configuration configuration =
                AssetUtility.FetchByPath<Configuration>(Constants.GetConfigurationAssetPath());

            bool environmentWasPassed = false;
            string environmentPassed = string.Empty;

            string environment = System.Environment.GetEnvironmentVariable("FUSE_ENV");
            if (string.IsNullOrEmpty(environment))
            {
                environmentPassed = environment;
                environmentWasPassed = true;
            }

            if (environmentWasPassed)
            {
                if (string.IsNullOrEmpty(environmentPassed))
                {
                    Logger.Warn("Environment key passed in command line but no value was supplied");
                    return;
                }

                foreach (string envGuid in AssetDatabase.FindAssets("t:Environment"))
                {
                    string path = AssetDatabase.GUIDToAssetPath(envGuid);
                    string name = Path.GetFileName(path);
                    if (string.IsNullOrEmpty(name))
                        continue;

                    if (string.Equals(name, environmentPassed, StringComparison.CurrentCultureIgnoreCase))
                    {
                        Logger.Info("Assigned override environment based on CLI/environment: " + environmentPassed);
                        configuration.Environment = path;
                        break;
                    }
                }
            }
        }
    }
}