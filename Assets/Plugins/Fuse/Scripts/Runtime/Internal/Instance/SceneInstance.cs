/*
 * Copyright (2020) Fractality LLC - All Rights Reserved
 * 
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 */

using System.Collections;
using JetBrains.Annotations;
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
		private readonly string _sceneName;

		public SceneInstance(string path, Environment environment) : base(path, environment)
		{
			_sceneName = Constants.GetFileNameFromPath(Id, Constants.SceneExtension);
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

			yield return SceneManager.LoadSceneAsync(_sceneName, LoadSceneMode.Additive);
			Logger.Info("Loaded scene: " + Id);
			Events.Publish(SceneEvent.Id, new SceneEvent(_sceneName, SceneEvent.Event.Load));
		}

		protected override IEnumerator Unload()
		{
			yield return SceneManager.UnloadSceneAsync(_sceneName);
			Bundles.UnloadBundle(Constants.GetSceneBundleFromPath(Id), true);
			Logger.Info("Unloaded scene: " + Id);
			Events.Publish(SceneEvent.Id, new SceneEvent(_sceneName, SceneEvent.Event.Unload));
		}
	}
}