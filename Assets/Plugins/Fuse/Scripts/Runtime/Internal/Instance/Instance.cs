/*
 * Copyright (2020) Fractality LLC - All Rights Reserved
 * 
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 */

using System.Collections;
using UnityEngine;

namespace Fuse
{
    /// <summary>
    /// The base of all resource tracking that is leveraged internally within the executor <see cref="Fuse"/>.
    /// </summary>
    public abstract class Instance
    {
        public readonly string Id;
        public bool Active => Loaded || Count > 0;

        protected readonly Environment Environment;
        protected uint Count;
        protected bool Loaded;

        /// <summary>
        /// The base of all resource tracking that is leveraged internally within the executor <see cref="Fuse"/>.
        /// </summary>
        public Instance(string id, Environment environment)
        {
            Id = id;
            Environment = environment;
        }

        public IEnumerator AddReference()
        {
            Count++;
            yield return Process();
        }

        public IEnumerator RemoveReference()
        {
            Count--;
            yield return Process();
        }

        protected abstract IEnumerator Load();
        protected abstract IEnumerator Unload();

        private IEnumerator Process()
        {
            if (Count == 0 && Loaded)
            {
                yield return Unload();
                yield return Resources.UnloadUnusedAssets();
                Loaded = false;
            }
            else if (Count > 0 && !Loaded)
            {
                yield return Load();
                Loaded = true;
            }
        }
    }
}