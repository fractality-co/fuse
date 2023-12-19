/*
 * Copyright (2020) Fractality LLC - All Rights Reserved
 * 
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Fuse
{
    /// <summary>
    /// Manages all settings that are unique per environment (e.g. debug, release, production).
    /// This will get overriden in <see cref="Configuration"/> based off your selections in the build pipeline.
    /// </summary>
    [Document("Core",
        "Manages all settings that are unique per environment (e.g. debug, release, production). " +
        "This also controls how assets are loaded (baked or online), server URIs and more." + 
        "\n\nIn a module you can retrieve the active environment with the injection attribute:" +
        "\n\n[Inject] private Environment _env")]
    public class Environment : ScriptableObject
    {
        /// <summary>
        /// The root host uri for downloading assets, note that asset bundles loaded will append the platform (lower-cased).
        /// </summary>
        public string hostUri;

        /// <summary>
        /// Defines where assets should be loaded from; baked will automatically be bundled.
        /// </summary>
        public LoadMethod loading;

        /// <summary>
        /// Defines how the build should run; develop turns on debug options, release turns off all debug options.
        /// </summary>
        public RuntimeMode mode;

        /// <summary>
        /// Configurable application icon per environment.
        /// </summary>
        public Texture2D icon;

        /// <summary>
        /// Configurable server URL mappings based on environment.
        /// </summary>
        public List<Server> servers;

        /// <summary>
        /// Configurable mapping of key/values based on environment.
        /// </summary>
        public List<StringProperty> properties;

        /// <summary>
        /// Custom player build options to be passed to the build pipeline.
        /// </summary>
        [BuildOptions] public int build = 513; // (BuildOptions.Development | BuildOptions.AllowDebugging)

        /// <summary>
        /// Custom asset build options to be passed to the asset pipeline.
        /// </summary>
        [BuildAssetBundleOptions] public int assets = 256; // BuildAssetBundleOptions.ChunkBasedCompression

        public string GetPath(string filePath)
        {
            return string.Format(Constants.AssetsBakedPath, filePath);
        }

        /// <summary>
        /// Retrieves what the hosting URI is with the specified sub file path passed and whether this is a unique file or not.
        /// </summary>
        public Uri GetFileUri(string filePath, bool platformUnique)
        {
            var subPath = string.Empty;

            if (platformUnique)
                subPath = Application.platform.GetBuildPlatformName() + "/";

            return new Uri(hostUri + "/" + subPath + filePath);
        }

        /// <summary>
        /// Return property value based on property key passed in.
        /// </summary>
        public string GetProperty(string key)
        {
            var property = properties.FirstOrDefault(prop => prop.key == key);
            return property == null ? string.Empty : property.value;
        }

        public Uri GetServer(string id, string[] relativeUri)
        {
            var server = FindServer(id);
            if (server == null)
                return null;

            UriBuilder builder;
            if (string.IsNullOrEmpty(server.Uri))
            {
                builder = new UriBuilder {Host = server.Host, Port = server.Port};
            }
            else
                builder = new UriBuilder(server.Uri);

            for (int i = 0; i < relativeUri.Length; i++)
            {
                if (i > 0)
                    builder.Path += "/";

                builder.Path += relativeUri[i];
            }

            return builder.Uri;
        }

        public Server FindServer(string id)
        {
            return servers.FirstOrDefault(server => server.Id == id);
        }
    }

    [Serializable]
    public class Server
    {
        public string Id;
        public string Uri;
        public string Host;
        public int Port;
    }

    /// <summary>
    /// Defines where assets are built to and loaded from.
    /// </summary>
    public enum LoadMethod
    {
        /// <summary>
        /// Built to and loaded from within StreamingAssets.
        /// </summary>
        Baked,

        /// <summary>
        /// Only build core assets, but load all other assets from a remote online source (e.g. S3, GCS).
        /// The core is built and loaded from StreamingAssets at first, then re-directed to remote.
        /// </summary>
        Online
    }

    /// <summary>
    /// Defines how the build should run and what features to turn on/off to assist in debugging or optimize for release.
    /// </summary>
    public enum RuntimeMode
    {
        /// <summary>
        /// Turns on all debugging features, and puts build into development build mode.
        /// </summary>
        Develop,

        /// <summary>
        /// Turns off all debugging features, and optimizes for release.
        /// </summary>
        Release
    }
}