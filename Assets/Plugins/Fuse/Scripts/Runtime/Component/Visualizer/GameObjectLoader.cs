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
    /// Simple implementation of visualizer that turns on and off a <see cref="GameObject"/>.
    /// </summary>
    [Document("Loader",
        "Simple implementation of Loader that turns on and off a GameObject that this is attached to. " +
        "\n\nPut this on the root of a prefab with your custom display, then assign it to Configuration (home).")]
    public class GameObjectLoader : Loader
    {
        public override IEnumerator Show()
        {
            gameObject.SetActive(true);
            yield break;
        }

        public override IEnumerator Hide()
        {
            gameObject.SetActive(false);
            yield break;
        }
    }
}