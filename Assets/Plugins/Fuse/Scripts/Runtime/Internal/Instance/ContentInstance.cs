/*
 * Copyright (2020) Fractality LLC - All Rights Reserved
 * 
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 */

using System.Collections;
using JetBrains.Annotations;

namespace Fuse
{
	/// <summary>
	/// Internal reference to consent and manages it's resources.
	/// </summary>
	[UsedImplicitly]
	public class ContentInstance : Instance
	{
		private readonly string _bundle;
		private Content _content;

		public ContentInstance(string id, Environment environment) : base(id, environment) { _bundle = Constants.GetFileNameFromPath(id, ".asset").ToLower().Replace(" ", string.Empty) + "_content"; }

		protected override IEnumerator Load()
		{
			bool success = false;
			while (!success)
			{
				yield return Bundles.LoadBundle(Environment,
					_bundle,
					result => { Logger.Info("Loaded bundle from content instance: " + _bundle); },
					null,
					Logger.Error);

				yield return Bundles.LoadAsset<Content>(Id, content =>
				{
					_content = content;
					Logger.Info($"Loaded content [{_content.name}]");
					success = true;
				}, null, Logger.Error);
				Events.Publish(ContentEvent.Id, new ContentEvent(_bundle, ContentEvent.Event.Load));
			}
		}

		protected override IEnumerator Unload()
		{
			Bundles.UnloadBundle(_bundle, false);
			yield return null;
			Events.Publish(ContentEvent.Id, new ContentEvent(_bundle, ContentEvent.Event.Unload));
		}
	}
}