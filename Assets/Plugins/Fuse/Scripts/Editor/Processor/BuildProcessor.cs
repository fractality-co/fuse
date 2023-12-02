using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Fuse.Editor
{
    /// <summary>
    /// Handles the generation and post-processing of <see cref="Fuse" />.
    /// </summary>
    [MeansImplicitUse]
    [InitializeOnLoad]
    [Document("Extendable",
        "Create a new class based off this to extend the Fuse pipeline as you see fit when making builds. "+ 
        "The inherent benefit of leveraging these processors, is that you can invoke any other pipelines as necessary. " + 
        "Unity does not allow you to run any pipeline while theirs runs." + 
        "\n\nThis supports both a pre and post processor steps for the pipeline.")]
    public abstract class BuildProcessor : IPreprocessBuildWithReport
    {
        public const int CallbackOrder = 1000;
        public int callbackOrder => CallbackOrder;

        public static readonly List<BuildProcessor> Processors = new List<BuildProcessor>();

        static BuildProcessor()
        {
            Processors.Clear();
            Type selfType = typeof(BuildProcessor);
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            List<Type> createProcessors = new List<Type>();
            foreach (Assembly assembly in assemblies)
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type == selfType)
                        continue;

                    if (type.IsSubclassOf(selfType))
                        createProcessors.Add(type);
                }
            }

            foreach (Type processor in createProcessors)
                Processors.Add(Activator.CreateInstance(processor) as BuildProcessor);

            SortProcessors();
        }

        protected abstract int Callback { get; }

        public abstract void PreProcessPlayer(Environment environment, BuildTarget buildTarget,
            BuildTargetGroup buildTargetGroup);

        public abstract void PostProcessPlayer(Environment environment, BuildTarget buildTarget,
            BuildTargetGroup buildTargetGroup);

        public void OnPreprocessBuild(BuildReport report)
        {
            if (!AssetDatabase.IsValidFolder(AssetProcessor.GetAssetIntegration()))
                Logger.Exception("Please ensure that you integrate assets manually for manual builds via Fuse/Advanced/Integrate");
        }

        private static void SortProcessors()
        {
            Processors.Sort((a, b) => a.Callback.CompareTo(b.Callback));
        }
    }
}