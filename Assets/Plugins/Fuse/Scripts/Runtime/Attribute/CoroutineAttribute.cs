/*
 * Copyright (2021) Fractality LLC - All Rights Reserved
 * 
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 */

using System;
using System.Collections;
using System.Reflection;
using JetBrains.Annotations;

namespace Fuse
{
    /// <summary>
    /// Starts a coroutine on a method that has a return type of <see cref="T:System.Collections.IEnumerator" />.
    /// The coroutine is started at the <see cref="Lifecycle" /> specified, and manually stopped when <see cref="Module" /> is (if running).
    /// Coroutines started on Setup/Cleanup will be blocking which will prevent <see cref="Fuse"/> from moving forward until complete.
    /// Otherwise, those started on Active will be non-blocking and ran in parallel.
    /// </summary>
    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Method)]
    [DefaultLifecycle(Lifecycle.Active)]
    [Document("Module","Starts a coroutine at the Lifecycle specified (default is Active), and manually stopped upon exit (if running). " + 
                       "Coroutines started on Setup/Cleanup will be blocking which will prevent Fuse from moving forward until complete." + 
                       "\n\n[Coroutine] private IEnumerator Active() { ... }")]
    public sealed class CoroutineAttribute : Attribute, IFusibleCoroutine
    {
        public uint Order { get; [UsedImplicitly] set; }
        public Lifecycle Lifecycle { get; [UsedImplicitly] set; }

        public IEnumerator Invoke(MemberInfo target, object instance)
        {
            var methodInfo = target as MethodInfo;
            if (methodInfo == null)
            {
                Logger.Error("Coroutine not started! Unsupported target (" + target.GetType().Name +
                             "); must be a method.");
                yield break;
            }

            if (methodInfo.ReturnType != typeof(IEnumerator))
            {
                Logger.Error("Coroutine not started! Only a return type of " + typeof(IEnumerator) + " is supported.");
                yield break;
            }

            switch (Lifecycle)
            {
                // when setting/cleaning up, coroutines are blocking to ensure no race conditions on custom logic
                case Lifecycle.Setup:
                case Lifecycle.Cleanup:
                    yield return methodInfo.Invoke(instance, null);
                    break;
                // while active, coroutines are ran in parallel to avoid blocking logic
                default:
                    Fuse.Instance.StartCoroutine((IEnumerator) methodInfo.Invoke(instance, null));
                    yield break;
            }
        }
    }
}