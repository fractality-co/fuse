/*
 * Copyright (2021) Fractality LLC - All Rights Reserved
 * 
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 */

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Fuse
{
    /// <summary>
    /// Serializable head of content in <see cref="Fuse"/>, which is within it's own asset bundle.
    /// </summary>
    [Document("Core",
        "Represents an asset bundle folder with a configurable ScriptableObject for assigned properties or objects. " +
        "You may place any Unity assets or data freely within the folder." +
        "\n\nIn a module you can retrieve the Content object with the injection attribute:" +
        "\n\n[Inject(\"name\")] private Content _content")]
    public class Content : ScriptableObject
    {
        public const string DefaultName = "Default";

        public List<StringProperty> properties;
        public List<ObjectProperty> assets;

        public T Load<T>(string path) where T : Object
        {
            return Bundles.LoadAsset<T>(path);
        }

        /// <summary>
        /// Return string value based on property key passed in.
        /// </summary>
        public int GetNumber(string key)
        {
            var property = GetStringProperty(key);
            return property == null ? default : int.Parse(property.value);
        }

        /// <summary>
        /// Return string value based on property key passed in.
        /// </summary>
        public string GetValue(string key)
        {
            return GetStringProperty(key)?.value;
        }

        /// <summary>
        /// Return Object value based on property key passed in.
        /// </summary>
        public T GetValue<T>(string key) where T : Object
        {
            var property = GetObjectProperty(key);
            return property?.value as T;
        }

        public bool HasValue(string key)
        {
            var prop = GetStringProperty(key);
            if (prop != null)
                return true;

            return GetObjectProperty(key) != null;
        }

        private StringProperty GetStringProperty(string key)
        {
            return properties.FirstOrDefault(property => property.key == key);
        }

        private ObjectProperty GetObjectProperty(string key)
        {
            return assets.FirstOrDefault(property => property.key == key);
        }
    }
}