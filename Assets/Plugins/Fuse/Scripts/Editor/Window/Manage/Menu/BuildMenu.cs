using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Fuse.Editor
{
    /// <summary>
    /// Sub-menu for building player and assets.
    /// </summary>
    public class BuildMenu : IMenu
    {
        public Action Refresh { get; set; }

        private const string SuccessfulMessage =
            "The export for the {0} pipeline was successful. All output is located at root of this Unity project @ {1}/{2}/*.\n\nPlatforms built: {3}.";

        private const string UnsuccessfulMessage =
            "The export for the {0} pipeline was <b>not</b> successful.\n\nPlease view the console logs, address then try again.";

        private enum Pipeline
        {
            Build,
            Assets
        }

        private bool _advanced;
        private Configuration _configuration;
        private Environment _environment;
        private Pipeline _pipeline = Pipeline.Build;
        private FuseBuildTarget _buildTarget;
        private Vector2 _scroll;

        public void Setup()
        {
            _configuration = AssetUtility.FetchByPath<Configuration>(Constants.GetConfigurationAssetPath());
            _environment = AssetUtility.FetchByPath<Environment>(_configuration.Environment);
            _buildTarget = PlatformUtility.GetPlatform().Item1.ToFuseBuildTarget();
        }

        public void Cleanup()
        {
        }

        public void Draw(Rect window)
        {
            GUILayout.BeginArea(window);
            _scroll = GUILayout.BeginScrollView(_scroll);
            DrawUtility.DrawHeaderText("BUILD", "export artifacts", 20, 42, 24);
            DrawUtility.DrawLabel(
                "<i>Export an application build or asset artifacts for a target platform with custom environment.</i>",
                DrawUtility.LabelType.TitleDescription, 16);
            GUILayout.Space(20);

            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            DrawUtility.DrawLabel("<b>ENVIRONMENT</b>", DrawUtility.LabelType.Inspector, 18);
            _environment = (Environment) EditorGUILayout.ObjectField(string.Empty, _environment, typeof(Environment),
                false);
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
            DrawUtility.DrawLabel(
                "<i>The assigned environment which will instruct the pipeline for how it should build." +
                "This is by default the environment found within your configuration.</i>",
                DrawUtility.LabelType.InspectorDescription, 14);
            GUILayout.EndVertical();

            GUILayout.Space(20);

            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            DrawUtility.DrawLabel("<b>PLATFORMS</b>", DrawUtility.LabelType.Inspector, 18);
            _buildTarget = (FuseBuildTarget) EditorGUILayout.EnumFlagsField(_buildTarget);
            GUILayout.EndHorizontal();
            DrawUtility.DrawLabel(
                "<i>Defines which platforms to export for; this is by default the platform you have selected in your editor.</i>",
                DrawUtility.LabelType.InspectorDescription, 14);
            GUILayout.Space(5);

            GUILayout.EndVertical();

            GUILayout.Space(20);

            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            DrawUtility.DrawLabel("<b>PIPELINE</b>", DrawUtility.LabelType.Inspector, 18);
            _pipeline = (Pipeline) EditorGUILayout.EnumPopup(_pipeline);
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
            DrawUtility.DrawLabel(
                "<i>Controls which pipeline to run to export the corresponding artifact(s).</i>",
                DrawUtility.LabelType.InspectorDescription, 14);
            GUILayout.EndVertical();

            GUILayout.Space(40);

            if (DrawUtility.DrawButton(DrawUtility.ButtonType.Export))
            {
                ExecutePipeline(_pipeline, _buildTarget);
                GUIUtility.ExitGUI();
            }

            GUILayout.Space(40);

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void ExecutePipeline(Pipeline pipeline, FuseBuildTarget buildTarget)
        {
            if (_environment == null)
            {
                Popup.Display<GenericPopup>(
                    GenericPopup.Build(
                        "PIPELINE",
                        "ERROR",
                        "Please specify environment when building pipeline. Within the build menu, at ENVIRONMENT press the right selection widget.")
                );
                return;
            }

            PlatformUtility.CacheCurrent();

            var successful = true;
            var results = new List<BuildResult>();
            foreach (FuseBuildTarget target in Enum.GetValues(typeof(FuseBuildTarget)))
            {
                if ((target & buildTarget) == FuseBuildTarget.None)
                    continue;

                var result = ExecuteBuild(pipeline, target, _environment);
                results.Add(result);

                if (!result.Successful)
                {
                    successful = false;
                    results.Clear();
                    break;
                }
            }

            PlatformUtility.CacheRevert();

            var title = successful ? "COMPLETE" : "FAILURE";
            var subtitle = pipeline.ToString().ToUpper() + " PIPELINE";

            var platforms = results[0].BuildTarget.ToBuildTarget().ToString();
            for (var index = 1; index < results.Count; index++)
                platforms += ", " + results[index].BuildTarget.ToBuildTarget();
            var directory = pipeline == Pipeline.Build ? Constants.EditorBuildsPath : Constants.EditorBundlePath;
            var description = successful
                ? string.Format(SuccessfulMessage, pipeline, directory, _environment.name, platforms)
                : string.Format(UnsuccessfulMessage, pipeline);

            Popup.Display<GenericPopup>(GenericPopup.Build(title, subtitle, description));
        }

        private BuildResult ExecuteBuild(Pipeline pipeline, FuseBuildTarget buildTarget, Environment environment)
        {
            BuildResult result = new BuildResult();

            switch (pipeline)
            {
                case Pipeline.Build:
                    var report = PlayerProcessor.Build(buildTarget.ToBuildTarget(), buildTarget.ToBuildTargetGroup(),
                        _environment);
                    result = new BuildResult(report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded,
                        PlayerProcessor.GetOutputPath(buildTarget.ToBuildTarget(), environment), buildTarget);
                    break;
                case Pipeline.Assets:
                    var manifest = AssetProcessor.Build(buildTarget.ToBuildTarget(), environment);
                    result = new BuildResult(manifest != null,
                        AssetProcessor.GetOutputPath(buildTarget.ToBuildTarget()), buildTarget);
                    break;
            }

            return result;
        }

        private struct BuildResult
        {
            public FuseBuildTarget BuildTarget;
            public string BuildPath;
            public bool Successful;

            public BuildResult(bool successful, string buildPath, FuseBuildTarget buildTarget)
            {
                Successful = successful;
                BuildPath = buildPath;
                BuildTarget = buildTarget;
            }
        }
    }
}