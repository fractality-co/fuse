/*
 * Copyright (2020) Fractality LLC - All Rights Reserved
 * 
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 */

using UnityEngine;

namespace Fuse
{
	/// <summary>
	/// Data relating to your applications functionality then executed by <see cref="Fuse" />.
	/// </summary>
	[Document("Core",
		"Represents the boot up configuration for the application, along with the global dependencies to start/load. " +
		"The assigned Environment controls how the application should be built and ran as well." +
		"\n\nThis is fully managed by Fuse and should require no direct management or loading.")]
	public class Configuration : ScriptableObject
	{
		/// <summary>
		/// Allows a hard-referenced visualizer that will show up when <see cref="Fuse"/> is loading or resolving state.
		/// This feature is only available in Professional or Enterprise./>
		/// </summary>
		public Loader Loader;

		/// <summary>
		/// The primary entrypoint for the application, this should have <see cref="Fuse"/> in that scene.
		/// </summary>
		[SceneReference(InBundles = false)] public string Main;

		/// <summary>
		/// The first <see cref="State"/> to go to once <see cref="Fuse"/> is ready.
		/// </summary>
		[AssetReference(typeof(State), Constants.CoreAssetPath)] public string Start;

		/// <summary>
		/// Defines what build mode, hosting paths and more that change based on environment.
		/// </summary>
		[AssetReference(typeof(Environment), Constants.CoreAssetPath)] public string Environment;

		/// <summary>
		/// Upon start of <see cref="Fuse"/>, start up these modules for the entire life of the application.
		/// These are not bound to any states, and will always be available.
		/// </summary>
		[ModuleReference] public string[] Modules;

		/// <summary>
		/// Upon start of <see cref="Fuse"/>, start up these modules for the entire life of the application.
		/// These are not bound to any states, and will always be available.
		/// </summary>
		[SceneReference] public string[] Scenes;

		/// <summary>
		/// Upon start of <see cref="Fuse"/>, load content for the entire life of the application.
		/// These are not bound to any states, and will always be available.
		/// </summary>
		[AssetReference(typeof(Content), Constants.ContentAssetPath)] public string[] Content;
	}
}