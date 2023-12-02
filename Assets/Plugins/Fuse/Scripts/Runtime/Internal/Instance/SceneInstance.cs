using System;
using System.Collections;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fuse
{
    /// <summary>
    /// Internal reference to scene and manages it's resources.
    /// </summary>
    [UsedImplicitly]
    public class SceneInstance : Instance
    {
        private readonly string _bundle;

        public SceneInstance(string path, Environment environment) : base(path, environment)
        {
            _bundle = Constants.GetSceneBundleFileFromPath(path);
        }

        protected override IEnumerator Load()
        {
            bool success = false;
            while (!success)
            {
                yield return Bundles.LoadBundle(Environment,
                    _bundle,
                    result =>
                    {
                        Logger.Info("Loaded bundle from scene instance: " + _bundle);
                        success = true;
                    },
                    null,
                    Logger.Error);
            }

            yield return SceneManager.LoadSceneAsync(Constants.GetFileNameFromPath(Id, Constants.SceneExtension), LoadSceneMode.Additive);
            Logger.Info("Loaded scene: " + Id);
        }

        protected override IEnumerator Unload()
        {
            yield return SceneManager.UnloadSceneAsync(
                Constants.GetFileNameFromPath(Id, Constants.SceneExtension));
            Bundles.UnloadBundle(Constants.GetSceneBundleFromPath(Id), true);
            Logger.Info("Unloaded scene: " + Id);
        }
    }
}