/* Copyright (2021) Fractality LLC - All Rights Reserved
 * 
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 */

using UnityEngine;

namespace Fuse
{
    /// <summary>
    /// Abstraction for singleton design pattern for <see cref="MonoBehaviour"/>s.
    /// </summary>
    [DisallowMultipleComponent]
    public abstract class SingletonBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
            Instance = (T) (object) this;
        }
    }
}