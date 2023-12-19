/*
 * Copyright (2021) Fractality LLC - All Rights Reserved
 * 
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 */

using System;
using System.Reflection;
using JetBrains.Annotations;

namespace Fuse
{
    /// <summary>
    /// When <see cref="Events"/> are published, this will listen for matching events and invoke when found.
    /// Events are only processed while in the Active phase of the <see cref="Lifecycle" />.
    /// </summary>
    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Event)]
    [Document("Events",
        "Subscribe to an event received through the Relay assignable to method or events. " +
        "You may receive custom event args through the system to encapsulate results or state." +
        "\n\n[Subscribe(\"event.id\")] private void OnEvt() { }" +
        "\n[Subscribe(\"event.id\")] ... void OnEvt(EventArgs args) { }")]
    public sealed class SubscribeAttribute : Attribute, IFusibleLifecycle
    {
        private readonly string _id;

        /// <summary>
        /// When <see cref="Events"/> are published, this will listen for matching events and invoke when found.
        /// Events are only processed while in the Active phase of the <see cref="Lifecycle" />.
        /// </summary>
        public SubscribeAttribute(string eventId)
        {
            _id = eventId;
        }

        public SubscribeAttribute(Type eventType)
        {
            _id = eventType.ToString();
        }

        public uint Order => 0;

        public Lifecycle Lifecycle => Lifecycle.Active;

        public void OnEnter(MemberInfo target, object instance)
        {
            Events.Subscribe(_id, new Tuple<MethodBase, object>(GetMethod(target), instance));
        }

        public void OnExit(MemberInfo target, object instance)
        {
            Events.Unsubscribe(_id, new Tuple<MethodBase, object>(GetMethod(target), instance));
        }

        private static MethodBase GetMethod(MemberInfo memberInfo)
        {
            switch (memberInfo)
            {
                case MethodInfo methodInfo:
                    return methodInfo;
                case EventInfo eventInfo:
                    return eventInfo.GetRaiseMethod();
                default:
                    return default;
            }
        }
    }
}