/* Copyright (2021) Fractality LLC - All Rights Reserved
 * 
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 */

using System.Collections;
using UnityEngine;

namespace Fuse
{
    /// <summary>
    /// Implement this to build your own custom Loader when <see cref="Fuse"/> is loading.
    /// </summary>
    [Document("Loader",
        "Create an implementation based on this to build your own custom display while Fuse is loading and resolving dependencies." +
        "\n\nFuse will invoke a blocking coroutine for Show and Hide, this enables full control over how you want to visualize loading.")]
    public abstract class Loader : MonoBehaviour
    {
        /// <summary>
        /// Implement custom logic for showing (e.g. fade in alpha).
        /// </summary>
        public abstract IEnumerator Show();

        /// <summary>
        /// Implement custom logic for showing progress.
        /// </summary>
        public virtual void Progress(float progress) { }
        
        /// <summary>
        /// Implement custom logic for displaying loading step.
        /// </summary>
        public virtual void Step(string message) { }

        /// <summary>
        /// Implement custom logic for showing (e.g. fade out alpha).
        /// </summary>
        public abstract IEnumerator Hide();
    }
}