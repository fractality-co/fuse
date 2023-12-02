using System;
using UnityEditor;
using UnityEditorInternal;

namespace Fuse.Editor
{
    /// <summary>
    /// Handles the pipeline for builds via Unity Cloud Build.
    /// </summary>
    public static class UnityCloudBuildProcessor
    {
        /// <summary>
        /// Sets up the build pipeline for creating the player on the current active build target.
        /// This command will automatically prepare assets as necessary.
        /// To build alternative platforms, switch to that platform and run this command again.
        /// You can override the environment when running via CLI: '-environment "Debug"'.
        /// </summary>
        public static void Setup()
        {
            if (InternalEditorUtility.inBatchMode)
                PlayerProcessor.AttemptFetchEnvironment();

            var configuration =
                AssetUtility.FetchByPath<Configuration>(Constants.GetConfigurationAssetPath());
            var environment = AssetUtility.FetchByPath<Environment>(configuration.Environment);
            if (environment == null)
            {
                if (!InternalEditorUtility.inBatchMode)
                    throw new Exception(
                        "Unable to setup; no environment defined in configuration or valid one passed");

                Logger.Error("Unable to setup, no environment assigned in configuration or valid one passed.");
                EditorApplication.Exit(1);
                return;
            }

            var buildTarget = PlatformUtility.GetPlatform();
            foreach (var processor in BuildProcessor.Processors)
                processor.PreProcessPlayer(environment, buildTarget.Item1, buildTarget.Item2);

            Logger.Info("Successful setup for Unity Cloud Build");
        }

        /// <summary>
        /// Cleans up the active running build pipeline.
        /// </summary>
        public static void Cleanup()
        {
            var configuration = AssetUtility.FetchByPath<Configuration>(Constants.GetConfigurationAssetPath());
            var environment = AssetUtility.FetchByPath<Environment>(configuration.Environment);
            if (environment == null)
            {
                if (!InternalEditorUtility.inBatchMode)
                    throw new Exception(
                        "Unable to cleanup; no environment defined in configuration or valid one passed");

                Logger.Error("Unable to cleanup, no environment assigned in configuration or valid one passed.");
                EditorApplication.Exit(1);
                return;
            }

            var buildTarget = PlatformUtility.GetPlatform();
            foreach (var processor in BuildProcessor.Processors)
                processor.PostProcessPlayer(environment, buildTarget.Item1, buildTarget.Item2);

            Logger.Info("Successful cleanup for Unity Cloud Build");
        }
    }
}