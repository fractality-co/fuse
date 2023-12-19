/*
 * Copyright (2021) Fractality LLC - All Rights Reserved
 * 
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 */

using System;
using System.Reflection;
using System.Threading;
using JetBrains.Annotations;

namespace Fuse
{
    /// <summary>
    /// Invokes the method into the <see cref="ThreadPool" />.
    /// By default, this executes on the Active <see cref="Lifecycle" />.
    /// WARNING: You are responsible for maintaining scope. Make sure you know what you are doing is thread-safe (Unity is not).
    /// </summary>
    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Method)]
    [DefaultLifecycle(Lifecycle.Active)]
    [Document("Module",
        "Queues a new thread from the thread pool at the Lifecycle specified (default is Active). " + 
        "\n\nWARNING: You are responsible for maintaining scope. Make sure you know what you are doing is thread-safe (Unity is not)." + 
        "\n\n[Thread] private void Threaded() { ... }")]
    public class ThreadAttribute : Attribute, IFusibleInvoke
    {
        public uint Order { get; [UsedImplicitly] private set; }

        public Lifecycle Lifecycle { get; [UsedImplicitly] private set; }

        public void Invoke(MemberInfo target, object instance)
        {
            ThreadPool.QueueUserWorkItem(context => { ((MethodInfo) target).Invoke(instance, null); });
        }
    }
}