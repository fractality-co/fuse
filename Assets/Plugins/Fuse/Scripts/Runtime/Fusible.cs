/*
 * Copyright (2020) Fractality LLC - All Rights Reserved
 * 
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 */

using System.Collections;
using System.Reflection;

namespace Fuse
{
    /// <summary>
    /// Base interface that <see cref="Fuse" /> uses to mark attributes for processing within a specific <see cref="Lifecycle" />.
    /// </summary>
    public interface IFusible
    {
        uint Order { get; }
        Lifecycle Lifecycle { get; }
    }

    /// <summary>
    /// <see cref="N:Fuse" /> will process all attributes that implement this interface.
    /// </summary>
    /// <inheritdoc />
    [Document("Extendable",
        "Fuse will invoke all attributes that implement this interface with the matching Lifecycle. " +
        "Extend the IOC logic layer for modules by creating an implementation based on this." +
        "\n\nThis is useful for one-time invocations per a Lifecycle, such as resolving a dependency.")]
    public interface IFusibleInvoke : IFusible
    {
        void Invoke(MemberInfo target, object instance);
    }

    /// <summary>
    /// <see cref="N:Fuse" /> will process on a new coroutine all attributes that implement this interface.
    /// </summary>
    /// <inheritdoc />
    [Document("Extendable",
        "Fuse will run a coroutine on attributes that implement this interface. " +
        "The Setup or Cleanup lifecycle are blocking, the Active lifecycle are ran in parallel. " +
        "Extend the IOC logic layer for modules by creating an implementation based on this." +
        "\n\nThis is useful for asynchronous functionality, such as loading an asset or waiting for a result.")]
    public interface IFusibleCoroutine : IFusible
    {
        IEnumerator Invoke(MemberInfo target, object instance);
    }

    /// <summary>
    /// <see cref="N:Fuse" /> invokes the attribute when entering or exiting the assigned <see cref="T:Fuse.Lifecycle" />.
    /// </summary>
    /// <inheritdoc />
    [Document("Extendable",
        "Fuse will call either OnEnter or OnExit when entering or exiting the assigned Lifecycle, to all attributes that implement this. " +
        "Extend the IOC logic layer for modules by creating an implementation based on this." +
        "\n\nThis is useful for more advanced functionality that requires an enter and exit to manage it.")]
    public interface IFusibleLifecycle : IFusible
    {
        void OnEnter(MemberInfo target, object instance);
        void OnExit(MemberInfo target, object instance);
    }
}