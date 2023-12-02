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