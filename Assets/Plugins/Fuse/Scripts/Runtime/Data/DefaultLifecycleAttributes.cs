using System;

namespace Fuse
{
    /// <summary>
    /// When marked onto an attribute for <see cref="Fuse"/> it will use this value as the default lifecycle to live in.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class DefaultLifecycleAttribute : Attribute
    {
        public readonly Lifecycle Lifecycle;

        public DefaultLifecycleAttribute(Lifecycle lifecycle)
        {
            Lifecycle = lifecycle;
        }
    }
}